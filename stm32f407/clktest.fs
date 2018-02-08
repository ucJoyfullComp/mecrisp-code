$40020000 constant PORTA_Base
$40020400 constant PORTB_Base
$40020800 constant PORTC_Base
$40020C00 constant PORTD_Base
$40021000 constant PORTE_Base
$40021400 constant PORTF_Base
$40021800 constant PORTG_Base
$40021C00 constant PORTH_Base
$40022000 constant PORTI_Base

$00 constant PORT_MODER    \ Reset 0 Port Mode Register - 00=Input  01=Output  10=Alternate  11=Analog
$04 constant PORT_OTYPER   \ Reset 0 Port Output type register - (0) Push/Pull vs. (1) Open Drain
$08 constant PORT_OSPEEDR  \ Reset 0 Output Speed Register - 00=2 MHz  01=25 MHz  10=50 MHz  11=100 MHz
$0C constant PORT_PUPDR    \ Reset 0 Pullup / Pulldown - 00=none  01=Pullup  10=Pulldown
$10 constant PORT_IDR      \ RO      Input Data Register
$14 constant PORT_ODR      \ Reset 0 Output Data Register
$18 constant PORT_BSRR     \ WO      Bit set/reset register   31:16 Reset 15:0 Set
\ +$1C                                     ... is Lock Register, unused
$20 constant PORT_AFRL     \ Reset 0 Alternate function  low register
$24 constant PORT_AFRH     \ Reset 0 Alternate function high register

: test_freq ( -- ) \ use port C14, C15 to toggle as past as possible to check cpu freq
	%0101 14 2* shl 				( -- n )
	portc_base port_moder +	!		( n -- )
	%11 14 shl						( -- n )
	portc_base port_otyper + !		( n -- )
	%1111 14 2* shl					( -- n )
	portc_base port_ospeedr + !		( n -- )
	$0								( -- n )
	portc_base port_pupdr + !		( n -- )
	%11 14 shl						( -- n )
	>r								( n -- )
	0								( -- n )
	100000000 0 do \ loop 100M times
		not dup r@ and				( n -- !n n1 )
		portc_base port_odr + !		( n n -- n )
	loop
	drop							( n -- )
;

