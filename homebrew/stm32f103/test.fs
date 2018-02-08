$40021018 constant RCC_APB2ENR
$40010800 constant GPIOA_CRL
$4001080C constant GPIOA_ODR

: gpio-init
    $00000004 $40021018 bis!
    $11111111 $40010800 !
;

: wait 200000 0 do i drop loop ;

0 variable val

\ val = ()val + 1) & 0xff;
: increment val @ 1+ $FF and val ! ;

: display val @ $4001080C c! ;

: counter begin
	increment
	display
	wait
	key? if exit then
    again ;

: main
    gpio-init counter ;

