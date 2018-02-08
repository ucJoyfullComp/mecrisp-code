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

\ the us@72M gives approximately 1 microsecond per loop
\   for a system clock of 72MHz ( default clock )
: us@8M  ( us -- )
    0 do 1 drop loop
;

: us@72M ( us -- )
    5 * 0 do i 1+ 2 + 3 + drop loop
;

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
1024 buffer: spi1-rx-buf
1024 buffer: spi1-tx-buf

: spi1.en
    %1000000 SPI1_CR1 hbis!
;

: spi1.dis
    %1000000 SPI1_CR1 hbic!
;

: spi1.nss.on?
    %10000 $40010808 hbit@ not
;

: spi1.nss.on
\    spi1.nss.on? if
\	exit
\    then
    \ enable NSS=0
    %10000 $4001080C hbic!
    $100 SPI1_CR1 hbis!
;

: spi1.nss.off
    \ disable NSS=1
    $100 SPI1_CR1 hbic!
    %10000 $4001080C hbis!
;

: spi1.tx.buffer ( addr length -- )
    \ check BSY flag
    %10000000 SPI1_SR cbit@ if
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
    drop
;

: spi1.tx ( addr length -- )
    spi1.en
    spi1.tx.buffer
    spi1.dis
;    

: spi1.tx.cmd ( command -- )
    spi1.en
    begin %10 SPI1_SR cbit@ until
    SPI1_DR c!
    \ wait for end transmit
    begin $80 SPI1_SR cbit@ not until
    spi1.dis
;    
    
: spi1.rx.buffer ( addr length -- )
    \ for receive we need to transmit 0xXX,
    \   as long as we want to keep receiving
    dup >r
    0 do
	begin $2 SPI1_SR cbit@ until
	$1 SPI1_SR cbit@ if
	    SPI1_DR c@ over i + c!
	then
	$FF SPI1_DR c!
	$1 SPI1_SR cbit@ if
	    SPI1_DR c@ over i + c!
	then
    loop
    r> +
    begin $80 SPI1_SR cbit@ until
    3 us@72M
    $1 SPI1_SR cbit@ if
	SPI1_DR c@ swap c!
    else
	drop
    then
;

: spi1.tx.cmd.rx ( rx-length addr length -- )
\    spi1.nss.on? if
\	drop 2drop
\	-1
\	exit
\    then
    spi1.en
    spi1.tx.buffer
    spi1-rx-buf swap spi1.rx.buffer
    spi1.dis
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
    $B4BB0000 $0000FFFF $40010800 @ and or $40010800 !
    \ configure SPI1
    \ no bidirectional, no CRC, 8bit frame, TXRX, MSB first, 1.125Mbps clock
    \ master mode, CPOL=0, CPHA=0
    $022C SPI1_CR1 h!
    $4 SPI1_CR2 h!
;

\ driver for Winbond serial flash w25qXX - XX=80,16,32
\   8MBit, 16MBit, 32MBit accordingly
: w25qXX.readST1 ( -- status1 )
    $05 spi1-tx-buf c!
    1 spi1-tx-buf 1 spi1.tx.cmd.rx
    spi1-rx-buf c@
;

: w25qXX.readST2 ( -- status2 )
    $35 spi1-tx-buf c!
    1 spi1-tx-buf 1 spi1.tx.cmd.rx
    spi1-rx-buf c@
;

: w25qXX.Wr.En ( -- )
    $06 spi1.tx.cmd
;

: w25qXX.Wr.Dis ( -- )
    $04 spi1.tx.cmd
;

: w25qXX.writeST ( st1 st2 -- ) \ st1,st2 are 8bit
    spi1-tx-buf
    $01 over c!
    1+ swap over 1+ c! c!
    spi1-tx-buf 3 spi1.tx
;

: w25qXX.PageProgram

;

\ : w25qXX.QuadPageProgram ;

: w25qXX.BlkErase64k ( src-addr -- )
    w25qXX.Wr.En
    dup 8 rshift
    dup 8 rshift
    spi1-tx-buf
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
    spi1-tx-buf
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
    spi1-tx-buf
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
    spi1-tx-buf
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

: w25qXX.readDevID ( -- devID )
    spi1-tx-buf
    $AB over
    0 over 1+
    2dup 1+
    2dup 1+
    c! c! c! c!
    1 swap 4 spi1.tx.cmd.rx
    spi1-rx-buf c@
    3 us@72M
;

: w25qXX.readManufDevID ( -- manufID devID )
    spi1-tx-buf
    $90 over
    0 over 1+
    2dup 1+
    2dup 1+
    c! c! c! c!
    2 swap 4 spi1.tx.cmd.rx
    spi1-rx-buf dup c@ 1+ c@
;

: w25qXX.readUID ( -- addr-UID length )
    spi1-tx-buf
    $4B over
    0 over 1+
    2dup 1+
    2dup 1+
    2dup 1+
    c! c! c! c! c!
    8 swap 5 spi1.tx.cmd.rx
    spi1-rx-buf 8
;

: w25qXX.readJEDECID ( -- addr-JEDECID length )
    spi1-tx-buf
    $9F over c!
    3 swap 1 spi1.tx.cmd.rx
    spi1-rx-buf 3
;

\ src-addr - is the address of the first byte in the Serial Flash
\ dst-addr - is the starting buffer address to store the read data
\   in the memory space of the uC
\ length - is the number of bytes to read from the flash
: w25qXX.read.Data ( src-addr dst-addr length -- )
    rot >r
    spi1-tx-buf
    $03 over c!
    r@ 16 rshift over 1+ c!
    r@ 8 rshift over 2 + c!
    r> over 3 + c! ( ... -- dst-addr length spi1-tx-buffer-addr )
    4
    spi1.en
    spi1.tx.buffer ( ... -- dst-addr length )
    spi1.rx.buffer
    spi1.dis
;

: w25qXX.read.FastData
    rot >r
    spi1-tx-buf
    $03 over c!
    r@ 16 rshift over 1+ c!
    r@ 8 rshift over 2 + c!
    r> over 3 + c! ( ... -- dst-addr length spi1-tx-buffer-addr )
    0 over 4 + c! 
    5
    spi1.en
    spi1.tx.buffer ( ... -- dst-addr length )
    spi1.rx.buffer
    spi1.dis
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

