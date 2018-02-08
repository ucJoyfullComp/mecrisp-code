\ start of dump.fth
: esc         27 emit ;

: red         esc ." [31;1m" ;
: green       esc ." [32;1m" ;
: yellow      esc ." [33;1m" ;
: blue        esc ." [34;1m" ;
: magenta     esc ." [35;1m" ;
: cyan        esc ." [36;1m" ;
: white       esc ." [37;1m" ;
: rst         esc ." [0m" ;

: dump.h      16 mod 0= if cr dup i + hex . decimal ." : " then ;
: nibbles     dup $f and swap $f0 and 4 rshift ;
: bs          8 emit ;

: color
    dup 0= if magenta exit then
    dup $20 < if blue exit then
    dup $7e > if cyan exit then
    dup $30 < if yellow exit then
    green
;

: .nibbles
    nibbles hex . bs . bs decimal
;

: dump ( addr length -- )
    cr 0 do
	i dump.h
	dup i + c@
	color .nibbles space rst
    loop
    cr drop
;

: dumpraw ( addr length -- )
    cr 0 do
	i 16 mod 0= if cr then
	dup i + c@ .nibbles
    loop
    cr drop
;

\ end of dump.fth

\ start of spi-core.fth
\ the us@72M gives approximately 1 microsecond per loop
\   for a system clock of 72MHz ( default clock )
: us@72M ( us -- )
    0 do i drop loop
;

: spi-buffer ( "name" u -- ) ( n -- addr|size|len )
  <builds dup 1 < if exit then dup , 0 , allot
  does> swap dup 0 < if
	dup -1 = if
	    drop @ exit
	then
	dup -2 = if
	    drop 1 cells + exit
	then
    then
    2 cells + +
;

\ end of spi-core.fth

$40013000 constant SPI1_CR1
$40013004 constant SPI1_CR2
$40013008 constant SPI1_SR
$4001300C constant SPI1_DR
\ The following are not used in this configuration
\
\ $40013010 SPI1_CRCPR
\ $40013014 SPI1_RXCRCR
\ $40013018 SPI1_TXCRCR
\ $4001301C SPI1_I2SCFGR
\ $40013020 SPI1_I2SPR

\ spi1-err is the SPI1 error indicator
\ bit0 : rx buffer is full. stopped the receive process. dropping all new rx data
\ bit1 : rx overflow. stopped rx process, dropping all new rx data.
\ bit2 : MODF flag trigerred. SPI1 is disabled
\ bit3 : tx not transmitting due to invalid tx buffer
\ bit4 : tx is BSY, not ready to begin transmit
\ bit5 : NSS is low, in a transaction, unable to begin new transaction
0 variable spi1-err
0 variable spi1-rx-en
64 spi-buffer spi1-rx-buffer
0 variable spi1-tx-len
64 buffer: spi1-tx-buffer

: spi1.rx.full? ( -- flag )
    -2 spi1-rx-buffer @ -1 spi1-rx-buffer < not
;

: spi1.rx.c!+ ( c -- )
    -2 spi1-rx-buffer
    dup @
    swap 1 swap +!
    spi1-rx-buffer c!
;

: spi1.rx.empty
    0 -2 spi1-rx-buffer !
;

: spi1.rx.hexdump
    0 spi1-rx-buffer -2 spi1-rx-buffer @
    dumpraw
;

: spi1.rx.dump
    0 spi1-rx-buffer -2 spi1-rx-buffer @
    dump
;

: spi1.en
    0 spi1-rx-en !
    0 spi1-tx-len !
    0 spi1-err !
    %1000000 SPI1_CR1 hbis!
;

: spi1.dis
    %1000000 SPI1_CR1 hbic!
    0 spi1-rx-en !
    0 spi1-tx-len !
    0 spi1-err !
;

: spi1.en.recv
    0 -2 spi1-rx-buffer !
    0 not spi1-rx-en !
;

: spi1.dis.recv
    0 spi1-rx-en !
    0 -2 spi1-rx-buffer !
;

: spi1.reset-errors
    0 spi1-err !
;

: spi1.nss.on
    %10000 $40010808 hbit@ not if
	%100000 spi1-err bis!
	exit
    then
    \ enable NSS=0
    %10000 $4001080C hbic!
;

: spi1.nss.off
    \ disable NSS=1
    %10000 $4001080C hbis!
;

: spi1.tx.buffer ( addr length -- )
    \ check tc buffer length valid ( length>0 )
    dup 1 < if
	%1000 spi1-err bis!
	2drop
	exit
    then
    \ check BSY flag
    %10000000 SPI1_SR cbit@ if
	%10000 spi1-err bis!
	2drop
	exit
    then
    \ transmit the contents of tx buffer
    0 do
	begin %10 SPI1_SR cbit@ until
	dup c@ SPI1_DR c! 1+
    loop
    \ wait for end transmit
    begin $80 SPI1_SR cbit@ not until
;

: spi1.tx ( addr length -- error-code )
    0 spi1-rx-en !
    0 spi1-err !
    spi1.nss.on
    spi1-err @ 0= not if
	2drop
	spi1-err @
	exit
    then
    spi1.tx.buffer
    spi1.nss.off
    spi1-err @
;    

: spi1.tx.cmd ( command -- )
    \ check BSY flag
    %10000000 SPI1_SR cbit@ if
	%10000 spi1-err bis!
	drop
	exit
    then
    0 spi1-rx-en !
    0 spi1-err !
    spi1.nss.on
    spi1-err @ 0= not if
	drop
	exit
    then
    begin %10 SPI1_SR cbit@ until
    SPI1_DR c!
    \ wait for end transmit
    begin $80 SPI1_SR cbit@ not until
    spi1.nss.off
    spi1-err @
;    
    
: spi1.tx.cmd.rx ( rx-length addr length -- error-code -1|addr length )
    0 spi1-rx-en !
    0 spi1-err !
    spi1.nss.on
    spi1-err @ 0= not if
	drop 2drop
	spi1-err @ -1
	exit
    then
    spi1.tx.buffer
    \ for receive we need to transmit 0xXX,
    \   as long as we want to keep receiving
    -1 spi1-rx-en !
    0 do
	begin $2 SPI1_SR cbit@ until
	$FF SPI1_DR c!
    loop
    0 spi1-rx-en !
    spi1.nss.off
    spi1-err @ 0= if
	0 spi1-rx-buffer
	-2 spi1-rx-buffer
    else
	0 spi1-err @
    then
    0 spi1-err !
;

: spi1-int
    \ \\\\\\\\\\\\\ Uzi \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
    %1000000 $40010C0C hbis!
    \ \\\\\\\\\\\\\ Uzi \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

    \ get status register of SPI1
    SPI1_SR h@
    \ check RX buffer not empty
    %00000001 and 0= not if
	SPI1_DR h@
	spi1-rx-en @ if
	    spi1.rx.full? if
		0 spi1-rx-en !
		%1 spi1-err bis!
		drop
	    else
		spi1.rx.c!+
	    then
	else
	    drop
	then
    then
    \ get status register of SPI1
    SPI1_SR h@
    \ check OVR ( overrun ) error
    dup %01000000 and 0= not if
	0 spi1-rx-en !
	SPI1_DR h@
	SPI1_SR h@
	2drop
	%10 spi1-err bis!
    then
    \ check MODF ( mode fault ) error
    %00100000 and 0= not if
	SPI1_CR2 h@ SPI1_CR2 h!
	%100 spi1-err bis!
    then

    \ \\\\\\\\\\\\\ Uzi \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
    %1000000 $40010C0C hbic!
    \ \\\\\\\\\\\\\ Uzi \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
;

\ SPI1 in clock distribution enviroonment APB2
\ SPI1 address base $40013000
\ SPI1 default pins PA4:PA7
\ SPI1 NSS - logic is negative logic and SW control
: spi1-init ( -- )
    \ \\\\\\\\\\\\\ Uzi \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
    \ enable clock for GPIOB    
    %1000      $40021018 bis!
    \ assign port PB6 for interrupt spi1 indicator
    $03000000 $F0FFFFFF $40010800 @ and or $40010C00 !
    \ \\\\\\\\\\\\\ Uzi \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
    \ enable clock for SPI1
    $1000 $40021018 bis!
    \ enable clock for GPIOA
    %100       $40021018 bis!
    \ assign ports for SPI1 ( SS:PA4 SCK:PA5 MISO:PA6 MOSI:PA7 )
    $B4B70000 $0000FFFF $40010800 @ and or $40010800 !
    \ configure SPI1
    \ no bidirectional, no CRC, 8bit frame, TXRX, MSB first, 1.125Mbps clock
    \ master mode, CPOL=0, CPHA=0
    $002C SPI1_CR1 h!
    \ install interrupt handler for SPI1
    %1000 $E000E184 bis!
    1 $E000E400 35 + c!
    ['] spi1-int irq-spi1 !
    %1000 $E000E104 bis!
    \ enable interrupt on RX ready + Errors (OVR, MODF, CRCERR also but no CRC )
\    %01100000 SPI1_CR2 c!
    %01000000 SPI1_CR2 c!
    spi1.nss.off
    spi1.en
;

: test01
    spi1.rx.empty
    10 0 do
	i 48 + spi1.rx.c!+
    loop
    0 spi1-rx-buffer -2 spi1-rx-buffer @ type
    spi1.rx.hexdump
    spi1.rx.dump
;

: test02
    spi1.rx.empty
    -1 spi1-rx-buffer 0 do
	i spi1.rx.c!+
    loop
    spi1.rx.dump
    spi1.rx.empty
    -1 spi1-rx-buffer 0 do
	i -1 spi1-rx-buffer + spi1.rx.c!+
    loop
    spi1.rx.dump
    spi1.rx.empty
    -1 spi1-rx-buffer 0 do
	i -1 spi1-rx-buffer 2 * + spi1.rx.c!+
    loop
    spi1.rx.dump
    spi1.rx.empty
    -1 spi1-rx-buffer 0 do
	i -1 spi1-rx-buffer 3 * + spi1.rx.c!+
    loop
    spi1.rx.dump
    spi1.rx.empty
;

\ driver for Winbond serial flash w25qXX - XX=80,16,32
\   8MBit, 16MBit, 32MBit accordingly
: w25qXX.readST1 ( -- status1|-1 if error )
    1
    spi1-tx-buffer $05 over c!
    1 spi1.tx.cmd.rx
    -1 = if
	drop -1
    else
	c@
    then
;

: w25qXX.readST2 ( -- status2|-1 if error )
    1
    spi1-tx-buffer $35 over c!
    1 spi1.tx.cmd.rx
    -1 = if
	drop -1
    else
	c@
    then
;

: w25qXX.Wr.En ( -- )
    $06 spi1.tx.cmd
;

: w25qXX.Wr.Dis ( -- )
    $04 spi1.tx.cmd
;

: w25qXX.writeST ( st1 st2 -- ) \ st1,st2 are 8bit
    spi1-tx-buffer
    $01 over c!
    1+ swap over 1+ c! c!
    spi1-tx-buffer 3 spi1.tx
    drop
;

: w25qXX.PageProgram

;

\ : w25qXX.QuadPageProgram ;

: w25qXX.BlkErase64k ( src-addr -- )
    w25qXX.Wr.En
    dup 8 rshift
    dup 8 rshift
    spi1-tx-buffer
    $D8 over c!
    1+ tuck c!
    1+ tuck c!
    1+ tuck c!
    3 - 4 spi1.tx
    begin
	w25qXX.readST1 dup -1 = not
	$1 and 0 = and
    until
;

: w25qXX.BlkErase32k
    w25qXX.Wr.En
    dup 8 rshift
    dup 8 rshift
    spi1-tx-buffer
    $52 over c!
    1+ tuck c!
    1+ tuck c!
    1+ tuck c!
    3 - 4 spi1.tx
    begin
	w25qXX.readST1 dup -1 = not
	$1 and 0 = and
    until
;

: w25qXX.SectorErase
    w25qXX.Wr.En
    dup 8 rshift
    dup 8 rshift
    spi1-tx-buffer
    $20 over c!
    1+ tuck c!
    1+ tuck c!
    1+ tuck c!
    3 - 4 spi1.tx
    begin
	w25qXX.readST1 dup -1 = not
	$1 and 0 = and
    until
;

: w25qXX.ChipErase
    w25qXX.Wr.En
    $C7 spi1.tx.cmd
    begin
	w25qXX.readST1 dup -1 = not
	$1 and 0 = and
    until
;

: w25qXX.Erase.Suspend ( -- )
    $75 spi1.tx.cmd    
    22 us@72M
    begin
	w25qXX.readST1 dup 
	-1 =
	1 and 0= not and while
    repeat  
;

: w25qXX.Erase.Resume ( -- )
    $7A spi1.tx.cmd    
;

: w25qXX.Enable.PowerDown
    $B9 spi1.tx.cmd
    3 us@72M
;

: w25qXX.Enable.HighPerfMode
    spi1-tx-buffer
    $A3 over
    0 over 1+
    2dup 1+
    2dup 1+
    c! c! c! c!
    4 spi1.tx
    2 us@72M
;

: w25qXX.Release.PowerDown.HPM
    $AB spi1.tx.cmd
    3 us@72M
;

\ : w25qXX.ModeBitReset ;

: w25qXX.readDevID ( -- devID|-1 if error )
    spi1-tx-buffer
    $AB over
    0 over 1+
    2dup 1+
    2dup 1+
    c! c! c! c!
    1 swap 4 spi1.tx.cmd.rx
    -1 = if
	drop -1
    else
	c@
    then
    3 us@72M
;

: w25qXX.readManufDevID ( -- manufID devID|-1 )
    spi1-tx-buffer
    $90 over
    0 over 1+
    2dup 1+
    2dup 1+
    c! c! c! c!
    2 swap 4 spi1.tx.cmd.rx
    -1 = if
	drop -1
    else
	dup c@ swap 1+ c@
    then
;

: w25qXX.readUID ( -- addr-UID length|-1 )
    spi1-tx-buffer
    $4B over
    0 over 1+
    2dup 1+
    2dup 1+
    2dup 1+
    c! c! c! c! c!
    8 swap 5 spi1.tx.cmd.rx
    dup -1 = if
	2drop -1
    then
;

: w25qXX.readJEDECID ( -- addr-JEDECID length|-1 )
    spi1-tx-buffer
    $9F over c!
    3 swap 1 spi1.tx.cmd.rx
    dup -1 = if
	2drop -1
    then
;

\ src-addr - is the address of the first byte in the Serial Flash
\ dst-addr - is the starting buffer address to store the read data
\   in the memory space of the uC
\ length - is the number of bytes to read from the flash
\ length-read is the number of bytes read from the flash
: w25qXX.read.Data ( src-addr dst-addr length -- length-read )
    rot >r
    spi1-tx-buffer
    $03 over c!
    r@ 16 rshift over 1+ c!
    r@ 8 rshift over 2 + c!
    r> over 3 + c! ( ... -- dst-addr length spi1-tx-buffer-addr )
    4 
    0 spi1-rx-en !
    0 spi1-err !
    spi1.nss.on
    spi1.tx.buffer ( ... -- dst-addr length )
    \ disable receive interrupt. transfer data directly to destination
    %01100000 SPI1_CR2 hbic!
    0 swap
    0 do  ( ... -- dst-addr index=0 )
	$1 SPI1_SR cbit@ if
	    2dup +
	    SPI1_DR c@
	    swap c!
	    1+
	then
	begin $2 SPI1_SR cbit@ until
	$FF SPI1_DR c!
	$1 SPI1_SR cbit@ if
	    2dup +
	    SPI1_DR c@
	    swap c!
	    1+
	then
    loop
    begin
	SPI1_SR c@ dup 1 and 0= not if
	    2dup +
	    SPI1_DR c@
	    swap c!
	    1+
	then
    $80 and 0= until
    SPI1_SR c@ 1 and 0= not if
	+
	SPI1_DR c@
	swap c!
	1+
    else
	2drop
    then
    %01100000 SPI1_CR2 hbis!
    spi1.nss.off
    drop swap drop
    spi1-err @
;

: w25qXX.read.FastData
    rot >r
    spi1-tx-buffer
    $03 over c!
    r@ 16 rshift over 1+ c!
    r@ 8 rshift over 2 + c!
    r> over 3 + c! ( ... -- dst-addr length spi1-tx-buffer-addr )
    0 over 4 + c! 
    5
    0 spi1-rx-en !
    0 spi1-err !
    spi1.nss.on
    spi1.tx.buffer ( ... -- dst-addr length )
    \ disable receive interrupt. transfer data directly to destination
    %01100000 SPI1_CR2 hbic!
    0 swap
    0 do  ( ... -- dst-addr index=0 )
	$1 SPI1_SR cbit@ if
	    2dup +
	    SPI1_DR c@
	    swap c!
	    1+
	then
	begin $2 SPI1_SR cbit@ until
	$FF SPI1_DR c!
	$1 SPI1_SR cbit@ if
	    2dup +
	    SPI1_DR c@
	    swap c!
	    1+
	then
    loop
    begin
	SPI1_SR c@ dup 1 and 0= not if
	    2dup +
	    SPI1_DR c@
	    swap c!
	    1+
	then
    $80 and 0= until
    SPI1_SR c@ 1 and 0= not if
	+
	SPI1_DR c@
	swap c!
	1+
    else
	2drop
    then
    %01100000 SPI1_CR2 hbis!
    spi1.nss.off
    drop swap drop
    spi1-err @
;

: test.SF.01
    \ estimate the delay in us per 100, 1000, 10000, 100000
    \ by toggle pin pb6 until key pressed
    $40010C00 @
    $3 6 4 *
    $F over lshift $40010C00 bic!
    lshift $40010C00 bis!
    $40 $40010C0C hbic!
    begin
	$40 $40010C0C hbis!
	100 us@72M
	$40 $40010C0C hbic!
	100 us@72M
	$40 $40010C0C hbis!
	1000 us@72M
	$40 $40010C0C hbic!
	100 us@72M
	$40 $40010C0C hbis!
	10000 us@72M
	$40 $40010C0C hbic!
	100 us@72M
	$40 $40010C0C hbis!
	100000 us@72M
	$40 $40010C0C hbic!
	100 us@72M
	key? if exit then
    again
    $40010C00 !
;

: test.SF.02
    spi1-init
    cr
    ." status bytes read: "
    w25qXX.readST1 hex.
    w25qXX.readST2 hex.
    cr
    ." Device ID is: "
    w25qXX.readDevID hex.
    cr
    w25qXX.readManufDevID
    ." Manufacturer ID is: "
    swap hex.
    ." Device IS is: "
    hex.
    cr
    w25qXX.readUID
    ." Unique ID is: "
    dumpraw
    cr
    w25qXX.readJEDECID
    ." Manufacturer ID is: " 
    drop dup c@ hex. cr
    ." Memory Type is: "
    1+ dup c@ hex. cr
    ." Capacity Code is: "
    1+ c@ hex. cr
;

