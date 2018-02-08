: port.setout ( base-addr -- ) \ set port as output on all pins
    dup >r $55555555 swap !
    0 r@ 4 + h!
    $55555555 r@ 8 + !
    0 r@ $c + !
    $5555 r> $14 + h!
;

: port.toggle ( base-addr -- )
    $14 + $FFFF swap hxor!
;

: mydelay ( ms -- )
    0 do
	2000 0 do loop
    loop
;

: test_ports
    SetSysClockToHSI
    RCC_CFGR_R @ 2 rshift 3 and 0= not if
	exit
    then
    $05000000 RCC_CR_R bic!
    begin
	RCC_CR_R @ 24 rshift $F and 0=
    until
    $000B0000 RCC_CR_R bic!
    begin
	RCC_CR_R @ 16 rshift $3 and 0=
    until
    $1F RCC_AHB1RSTR_R bis!
    100 0 do loop
    $1F RCC_AHB1RSTR_R bic!
    100 0 do loop
    $7E7411E0 RCC_AHB1ENR_R bic!
    $F1 RCC_AHB2ENR_R bic!
    $1 RCC_AHB3ENR_R bic!
    $36FEC9FF RCC_APB1ENR_R bic!
    $00075F33 RCC_APB2ENR_R bic!
    GPIOA_BASE port.setout
    GPIOB_BASE port.setout
    GPIOC_BASE port.setout
    GPIOD_BASE port.setout
    GPIOE_BASE port.setout
    begin
	10 mydelay
	GPIOA_BASE port.toggle
	GPIOB_BASE port.toggle
	GPIOC_BASE port.toggle
	GPIOD_BASE port.toggle
	GPIOE_BASE port.toggle
    again
;

