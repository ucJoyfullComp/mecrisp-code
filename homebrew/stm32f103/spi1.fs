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
	2drop
	exit
    then
    \ enable NSS=0
    %10000 $4001080C hbic!
;

: spi1.nss.off
    \ enable NSS=0
    %10000 $4001080C hbic!
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
    spi1.tx.buffer
    spi1.nss.off
    spi1-err @
;    

: spi1.txcmd-rx ( rx-length addr length -- error-code -1|addr length )
    -1 spi1-rx-en !
    0 spi1-err !
    spi1.nss.on
    spi1.tx.buffer
    \ for receive we need to transmit 0xXX,
    \   as long as we want to keep receiving
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
    \ get status register of SPI1
    SPI1_SR h@
    \ check errors
    dup %01100000 and 0= not if
	dup %01000000 and 0= not if
	    0 spi1-rx-en !
	    SPI1_DR h@
	    SPI1_SR h@
	    %10 spi1-err bis!
	else
	    0 SPI1_CR2 hbic!
	    %100 spi1-err bis!
	then
    then
    dup %00000001 and 0= not if
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
;

\ SPI1 in clock distribution enviroonment APB2
\ SPI1 address base $40013000
\ SPI1 default pins PA4:PA7
\ SPI1 NSS - logic is negative logic and SW control
: spi1-init ( -- )
    \ enable clock for SPI1
    $1000 $40021018 bis!
    \ enable clock for GPIOA
    %100 $40021018 bis!
    \ assign ports for SPI1 ( SS:PA4 SCK:PA5 MISO:PA6 MOSI:PA7 )
    $B4B70000 $0000FFFF $40010800 @ and or $40010800 !
    \ configure SPI1
    \ no bidirectional, no CRC, 8bit frame, TXRX, MSB first, 9Mbps clock
    \ master mode, CPOL=0, CPHA=0
    $0014 SPI1_CR1 h!
    \ install interrupt handler for SPI1
    %1000 $E000E184 bis!
    0 $E000E400 35 + c!
    ['] spi1-int irq-spi1 !
    %1000 $E000E104 bis!
    \ enable interrupt on RX ready + Errors (OVR, MODF, CRCERR also but no CRC )
    %01100000 SPI1_CR2 h!
    \ init global variables
    0 spi1-rx-en !
    0 spi1-tx-len !
    0 spi1-err !
    \ SPI1 is disabled!
    
;

