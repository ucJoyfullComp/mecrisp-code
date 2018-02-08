: init_pc1415
	%0101 14 2* lshift 				( -- n )
	gpioc_base gpio_moder +	!		( n -- )
	%11 14 lshift					( -- n )
	gpioc_base gpio_otyper + !		( n -- )
	%0101 14 2* lshift				( -- n )
	gpioc_base gpio_ospeeder + !	( n -- )
	%0101 14 2* lshift				( -- n )
	gpioc_base gpio_pupdr + !		( n -- )
;

: togd_pc1415 ( n -- )
	gpioc_base gpio_odr + dup dup	( n -- n a a a )
	@ $3FFF and >r					( S: n a a a -- S: n a a R: n )
	$C000 r@ or swap !				( S: n a a R: n -- S: n a R: n )
	over 0 do loop					( S: n a R: n -- S: n a R: n )
	$0000 r> or swap !				( S: n a R: n -- n )
	2- 0 do loop					( n -- )
;

: tog_pc1415 ( -- )
	1000 0 do
		10 togd_pc1415
	loop
;

: port_test ( n -- ) \ use port C14, C15 to toggle as past as possible to check cpu freq
	0										( -- 0 )
	100000 0 do \ loop 100K times
		not dup $C000 and					( n -- !n n1 )
		gpioc_base gpio_odr + @				( n1 n2 -- n1 n2 n3 )
		$3FFF and or						( n1 n2 n3 -- n1 n4 )
		gpioc_base gpio_odr + !				( n1 n2 -- n )
		over 0 do loop
	loop
	2drop									( n1 -- )
;

: tbit ( n a -- ) \ toggle the bit n on output port whose base address is a
	gpio_odr + tuck ( n a -- a1 n a1 )
	@ swap          ( a1 n a1 -- a1 odrVal n )
	\ test bit number between 0..15
	dup dup -1 > swap 16 < and ( a1 odrVal n -- a1 odrVal n flag )
	if
		1 swap lshift dup not 	( ... odrVal n -- ... odrVal 2^n ~2^n )
		>r over r> and >r dup >r 
		xor r> and r> or ( ... odrVal n1 n2 -- a1 odrNewVal )
		swap !
	else
		drop 2drop
	then
;

: pulseit ( n -- )
\ switch pc15 on for n loops
gpioc_base dup gpio_odr + dup @ $c000 not and swap !
15 swap tbit
0 do loop
15 gpioc_base tbit
;

init_pc1415

