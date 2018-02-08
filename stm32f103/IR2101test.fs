\ test that min <= n <= max return true|false
: between ( n min max -- flag )
    rot tuck swap 1+ < -rot 1+ < and
;

\ test than n is in range 0..15 return true|false
: valid-pin? ( n -- flag )
    0 15 between
;

: baseaddr-from-portnum ( portnum -- baseaddr )
    case
	1 of gpioa_base endof
	2 of gpiob_base endof
	3 of gpioc_base endof
	4 of gpiod_base endof
	5 of gpioe_base endof
	>r 0 r>
    endcase
;

\ set pin PORTi[n] mode, n between 0..15, i between 1..5
: init-p-n ( i pin mode -- )
    rot baseaddr-from-portnum dup 0= if
	drop 2drop exit
    then
    >r
    $f and >r
    dup valid-pin? if
	dup 8 < if
	    4 * 
	    dup r> swap lshift swap $f swap lshift 
	    r> gpio_crl + dup >r @ swap bic or r> !
	else
	    8 - 4 *
	    dup r> swap lshift swap $f swap lshift 
	    r> gpio_crh + dup >r @ swap bic or r> !
	then
    else r> 2drop r> drop ." pin must be between 0..15"
    then
;

\ get timer n (2..4) base address or 0 if invalid timer
: tim-base-addr-2to4 ( n -- a-addr )
    case
	2 of tim2_base endof
	3 of tim3_base endof
	4 of tim4_base endof
	>r 0 r>
    endcase
;

tim4_base tim_arr + constant tim4_arr
tim4_base tim_ccr1 + constant tim4_ccr1
tim4_base tim_ccr2 + constant tim4_ccr2

\ set timer TIM[n] as independent timer with timebase to ticks clocks
: set-tim-2to4-timebase ( ticks n -- )
    dup 2 4 between if 
	1 over 2 - lshift rcc_apb1enr_r bis!
    else
	2drop exit
    then
    20 0 do i drop loop
    tim-base-addr-2to4 dup 0= not if >r else 2drop exit then
    $3ff r@ tim_cr1 + hbic!
    $f8 r@ tim_cr2 + hbic!
    $fff7 r@ tim_smcr + hbic!
    $7f5f r@ tim_dier + hbic!
    $1e5f r@ tim_sr + hbic!
    r@ tim_arr + h!
    \ for H driver we setup for center aligned counting
    $60 r@ tim_cr1 + hbis!
    \ enable timer
    1 r> tim_cr1 + hbis!
;

\ set tim n oc# output to configuration mode with clocks to ccr
: set-tim-oc ( clocks config oc# n -- )
    tim-base-addr-2to4 dup 0= if drop 2drop exit then 
    >r
    case
	1 of
	    $ff and r@ tim_ccmr1 + dup $ff swap hbic! hbis!
	    $1 r@ tim_ccer + dup $3 swap hbic! hbis!
	    r> tim_ccr1 + h! 
	endof
	2 of 
	    $ff and 8 lshift r@ tim_ccmr1 + dup $ff00 swap hbic! hbis!
	    $10 r@ tim_ccer + dup $30 swap hbic! hbis!
	    r> tim_ccr2 + h! 
	endof
	3 of 
	    $ff and r@ tim_ccmr2 + dup $ff swap hbic! hbis!
	    $100 r@ tim_ccer + dup $300 swap hbic! hbis!
	    r> tim_ccr3 + h! 
	endof
	4 of 
	    $ff and 8 rshift r@ tim_ccmr2 + dup $ff00 swap hbic! hbis!
	    $1000 r@ tim_ccer + dup $3000 swap hbic! hbis!
	    r> tim_ccr4 + h! 
	endof
	r> 2drop 2drop exit
    endcase 
;

: setup_1
    10000
    GetSysClock swap / 1-
    4 set-tim-2to4-timebase
    \ set OC output Ch1, Ch2
    2 6 $b init-p-n
    2 7 $b init-p-n
    \ set CCR1, CCR2
    40 $60 1 4 set-tim-oc
    50 $70 2 4 set-tim-oc
;

\ setup the TIM4
: setup_now
    setup_1
;


