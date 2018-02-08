compiletoflash
compiletoram

\
\ set DC/DC converter on TIM4 OC1 as PWM output for the inductor FET driver
\ and port A1 as the output value analog sampling input, and port A2 as the
\ reference input 
\ the main application is to increese the voltage on A1 is equal to the
\ reference voltage on A2 and to regulate the voltage on A1 to be equal
\ to that on A2 via the PWM pulse on B6
\
NVIC_BASE NVIC_ISER0 + constant NVIC_EN0_R
$20000000 constant NVIC_EN0_INT29          \ TIM3 interrupt 29
$40000000 constant NVIC_EN0_INT30          \ TIM4 interrupt 30

$00000800 constant NVIC_EN0_INT11          \ DMA1CH1 interrupt 11
$00001000 constant NVIC_EN0_INT12          \ DMA1CH2 interrupt 12

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

tim4_base tim_arr + constant tim4_arr_r
tim4_base tim_ccr1 + constant tim4_ccr1_r
tim4_base tim_ccr1 + constant tim4_ccr2_r

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
	1 r> tim_cr1 + hbis!
;

\ set tim n oc# output to PWM1 mode with clocks to ccr
: set-tim-oc ( clocks oc# n -- )
	tim-base-addr-2to4 dup 0= if drop 2drop exit then 
	>r
	case
	1 of
		$60 r@ tim_ccmr1 + dup $ff swap hbic! hbis!
		$1 r@ tim_ccer + dup $3 swap hbic! hbis!
		r> tim_ccr1 + h! 
	endof
	2 of 
		$6000 r@ tim_ccmr1 + dup $ff00 swap hbic! hbis!
		$10 r@ tim_ccer + dup $30 swap hbic! hbis!
		r> tim_ccr2 + h! 
	endof
	3 of 
		$60 r@ tim_ccmr2 + dup $ff swap hbic! hbis!
		$100 r@ tim_ccer + dup $300 swap hbic! hbis!
		r> tim_ccr3 + h! 
	endof
	4 of 
		$6000 r@ tim_ccmr2 + dup $ff00 swap hbic! hbis!
		$1000 r@ tim_ccer + dup $3000 swap hbic! hbis!
		r> tim_ccr4 + h! 
	endof
	r> drop 2drop exit
	endcase 
;

0 variable a1sum
0 variable a2sum
0 variable a1num
0 variable a2num
4 buffer: adcarr


: dmach1-isr
	49 emit
	$f dma_base dma_ifcr + bis!
;

: dmach2-isr
	50 emit
	$f0 dma_base dma_ifcr + bis!
;

: dmach3-isr
	51 emit
	$f00 dma_base dma_ifcr + bis!
;

: dmach4-isr
	52 emit
	$f000 dma_base dma_ifcr + bis!
;

: dmach5-isr
	53 emit
	$f0000 dma_base dma_ifcr + bis!
;

: dmach6-isr
	54 emit
	$f00000 dma_base dma_ifcr + bis!
;

: dmach7-isr
	55 emit
	$f000000 dma_base dma_ifcr + bis!
;

: dma-adc-avg-isr
	1 a1num +!
	32 a1num @ < if 
		-1 a1num +!
		a1sum @ a1num @ / negate a1sum +!
	then
	adcarr h@ + a1sum +!
	1 a2num +!
	128 a2num @ < if 
		-1 a2num +!
		a2sum @ a2num @ / negate a2sum +!
	then
	2 adcarr + h@ + a2sum +!
	49 emit
	$f dma_base dma_ifcr + bis!
;

: init-dma
	['] dmach1-isr irq-dma1ch1 !
	['] dmach2-isr irq-dma1ch2 !
	['] dmach3-isr irq-dma1ch3 !
	['] dmach4-isr irq-dma1ch4 !
	['] dmach5-isr irq-dma1ch5 !
	['] dmach6-isr irq-dma1ch6 !
	['] dmach7-isr irq-dma1ch7 !
\	['] dma-adc-avg-isr irq-dma1ch1 !
	1 rcc_ahbenr_r hbis!
	20 0 do i drop loop
	$25a0 dma_base dma_ccr1 + h!
	2 dma_base dma_cndtr1 + h!
	adc1_base adc_dr + dma_base dma_cpar1 + !
	adcarr dma_base dma_cmar1 + !
	NVIC_EN0_INT11 NVIC_EN0_R bis!
	1 dma_base dma_ccr1 + hbis!
	$f dma_base dma_ifcr + bis!
;

: enable-dma1s0-isr
	2 dma_base dma_ccr1 + hbis!
;

: setup_1
	100000
	GetSysClock swap / 1-
	4 set-tim-2to4-timebase
	2 6 $b init-p-n
	60 1 4 set-tim-oc
	0 dup a1sum ! a2sum !
	0 dup a1num ! a2num !
;

: setup_2
	2 1 2 initADCIns
	adc1_base 1 0 setInCh
	adc1_base 2 1 setInCh
	adc1_base 1 1 setInSmp
	adc1_base 1 2 setInSmp
	2 setNumOfChans
	$180100 adc1_base adc_cr2 + @ $106f0 and or adc1_base adc_cr2 !
	$cfffff adc1_base adc_cr1 + bic!
	$100 adc1_base adc_cr1 + bis!
	$1f adc1_base adc_sr + bic!
;

: setup_3
	19012
	GetSysClock swap / 1-
	3 set-tim-2to4-timebase
	$20 tim3_base tim_cr2 + dup $f8 swap hbic! hbis!
	1 tim3_base tim_cr1 + hbic!
;

: tim3-on 1 tim3_base tim_cr1 + hbis! ;

\ setup the DC/DC converter
: setup_now
	setup_1
\ setup ADC1 for normal conversion on 2 channels A1,A2 using DMA transfer
\ triggered by EOC, start of conversion by TIM3 Update event
	setup_2
	1 adc1_base adc_cr2 + bis!
\ setup timer TIM3 to generate TRGO on update event at rate of 19khz
	setup_3
	1 6 $b init-p-n
	60 1 3 set-tim-oc
	tim3-on
	init-dma
	enable-dma1s0-isr
;

compiletoram

