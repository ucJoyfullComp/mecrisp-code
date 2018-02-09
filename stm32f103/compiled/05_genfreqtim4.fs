: between ( n min max -- flag )
    >r over 1+ < swap r> 1+ < and
;

: valid-pin? ( n -- flag )
    0 15 between
;

: tim-base-addr-2to4 ( n -- a-addr flag )
    case
	2 of tim2_base true endof
	3 of tim3_base true endof
	4 of tim4_base true endof
	>r 0 false r>
    endcase
;

: set-tim-2to4-timebase ( ticks n -- )
    dup 2 4 between if 
	1 over 2 - lshift rcc_apb1enr_r bis!
    else
	2drop exit
    then
    20 0 do i drop loop
    tim-base-addr-2to4 if >r else drop exit then
    0 r@ tim_cr1 + h!
    0 r@ tim_cr2 + h!
    0 r@ tim_smcr + h!
    0 r@ tim_dier + h!
    $1e5f r@ tim_sr + hbic!
    r@ tim_arr + h! 
    1 r> tim_cr1 + hbis!
;

: init-pb-n ( n -- ) 
    dup valid-pin? if
	dup 8 < if
	    4 * 
	    dup $b swap lshift swap $f swap lshift 
	    gpiob_base gpio_crl + dup >r @ swap bic or r> !
	else
	    8 - 4 *
	    dup $b swap lshift swap $f swap lshift 
	    gpiob_base gpio_crh + dup >r @ swap bic or r> !
	then
    else ." pin must be between 0..15"
    then
;

: set-tim-oc ( clocks oc# n -- )
    tim-base-addr-2to4
    0= if drop 2drop exit then
    swap
    case
	1 of
	    $60 over tim_ccmr1 + dup $ff swap hbic! hbis!
	    $1 over tim_ccer + dup $3 swap hbic! hbis!
	    tim_ccr1 + h! 
	endof
	2 of 
	    $6000 over tim_ccmr1 + dup $ff00 swap hbic! hbis!
	    $10 over tim_ccer + dup $30 swap hbic! hbis!
	    tim_ccr2 + h! 
	endof
	3 of 
	    $60 over tim_ccmr2 + dup $ff swap hbic! hbis!
	    $100 over tim_ccer + dup $300 swap hbic! hbis!
	    tim_ccr3 + h! 
	endof
	4 of 
	    $6000 over tim_ccmr2 + dup $ff00 swap hbic! hbis!
	    $1000 over tim_ccer + dup $3000 swap hbic! hbis!
	    tim_ccr4 + h! 
	endof
	drop 2drop exit
    endcase 
;

: set-tim4-oc ( clocks oc# -- )
    4 set-tim-oc
;

: genFreqInPB6 ( freq -- )
    GetSysClock swap / dup 1-
    4 set-tim-2to4-timebase
    6 init-pb-n
    2/ 1- 1 set-tim4-oc
;

: gf genFreqInPB6 ;

tim4_base tim_arr + constant tim4_arr_r
tim4_base tim_ccr1 + constant tim4_ccr1_r
tim4_base tim_ccr1 + constant tim4_ccr2_r

: init
    init
    48000 
    GetSysClock swap / dup 1-
    4 set-tim-2to4-timebase
    2/ 1- 4 set-tim4-oc
;

