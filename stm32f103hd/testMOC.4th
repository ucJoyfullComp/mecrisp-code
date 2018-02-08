: A12out
	gpioa_base gpio_crh + 
	dup @ 
	$F0000 bic $20000 or 
	swap ! 
;

: A12in
	gpioa_base gpio_crh + 
	dup @ 
	$F0000 bic $20000 or 
	swap ! 
;

: ANtog ( N -- )
	1 swap lshift gpioa_base gpio_odr + hxor!
;

: ANon ( N -- )
	1 swap lshift gpioa_base gpio_odr + hbis!
;

: ANoff ( N -- )
	1 swap lshift gpioa_base gpio_odr + hbic!
;

: A11out
	gpioa_base gpio_crh + 
	dup @ 
	$F000 bic $2000 or 
	swap ! 
;

: A11tog
	$800 gpioa_base gpio_odr + hxor!
;

: A11On
	$800 gpioa_base gpio_odr + hbis!
;

: A11Off
	$800 gpioa_base gpio_odr + hbic!
;

8000000 variable SystemClockFreq
1330000 variable LoopsPerSec

: MeasEmptyLoop ( N -- )
	12 ANon
	0 do loop
	12 ANoff
;

: ms ( mSec -- ) \ mSec - must be non negative
	LoopsPerSec @ swap 1000 */ 
	0 do loop 
;

: A11HL ( hi-ms low-ms -- )
	swap
	A11On 
	ms
	A11Off 
	ms 
;

: testopto ( n on-ms -- ) \ on-ms must be between 1 and 20
	dup 0 >
	if
		dup 20 < if 
			swap 0 do
				dup 20 over - a11hl
			loop
		else
			drop
			A11on
		then
	else
		drop
		A11off
	then
;

a11out
