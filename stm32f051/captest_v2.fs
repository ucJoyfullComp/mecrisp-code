\ Was ok before!
\ Currently has problems in latching the time in comp2

0 variable low-th-count
0 variable high-th-count
0 variable sample-done

10 constant #results
#results 5 * cells buffer: result-array

0 variable captest-compensate

: init.comp.io
    $0000AFFF $00C0FFFF GPIOA_MODER tuck bic! bis!
    $00C0 GPIOA_OTYPER bic!
    $0000F000 GPIOA_OSPEEDR bis!
    $0000FFFF GPIOA_PUPDR bic!
    $00FF GPIOA_ODR bic!
    $77000000 $FF000000 GPIOA_AFRL tuck bic! bis!
;

: init.comp.comp
    \ enable clock for COMP
    $1 RCC_APB2ENR bis!
    30 0 do loop
    $04B10411 COMP_CSR !
\    3 21 lshift EXTI_EMR bis!
;

: init.comp.gen
    $00000010 $0000003C GPIOB_MODER tuck bic! bis!
    $0006 GPIOB_OTYPER bic!
    $0000003C GPIOB_OSPEEDR bis!
    $0000003C GPIOB_PUPDR bic!
    $0006 GPIOB_ODR bic!
;

: adc.isr
;

: adc_comp.isr
    $00200000 EXTI_PR bit@ if
	\ read TIM2 (32bit timer - @48MHz gives up to 89 seconds measurement time)
	5 0 do loop
	TIM2_CCR4 @ low-th-count !
	$00200000 EXTI_PR !
	$0040 GPIOA_ODR bis!
    then
    $00400000 EXTI_PR bit@ if
	\ read TIM2 (32bit timer - @48MHz gives up to 89 seconds measurement time)
	5 0 do loop
	TIM2_CCR4 @ high-th-count !
	$00400000 EXTI_PR !
	$0080 GPIOA_ODR bis!
	true sample-done !
    then
    ADC_ISR @ 0= not if 
	adc.isr
    then
;

: init.comp.intpt
    \ hooking the comparator isr to the ISR hook
    ['] adc_comp.isr irq-adc !
    \ enabling the IRQ for ADC/COMP
    $80 $FF NVIC_IPR3 tuck bic! bis!
    1 12 lshift NVIC_ISER bis!
    \ Setting the EXTI controller for the comparators to generate IRQ's
    3 21 lshift EXTI_IMR bis!
    3 21 lshift EXTI_PR bis!
    3 21 lshift EXTI_RTSR bis!
    3 21 lshift EXTI_FTSR bic!
;

: init.comp.tim2
    \ enable clock to TIM2
    $1 RCC_APB1ENR bis!
    \ reset TIM2 to start from a known position
    $1 RCC_APB1RSTR bis!
    30 0 do loop
    $1 RCC_APB1RSTR bic!
    30 0 do loop
    \ TIM2 should be running without preload value
    \ it will start at the change of state of the Capacitor charging
    \ it will operate in autoreload.
    $0000 TIM2_CR1 h!
    $0100 TIM2_CCMR2_Input h!
    $1000 TIM2_CCER h!
    0 TIM2_CNT !
    0 TIM2_PSC h!
    $FFFFFFFF TIM2_ARR !
;

: captest.quickcharge ( 100us -- )
    \ set PB1 to push pull output with output value as PB2
\  test if stack is empty. If so, exit.
\    depth 0= if exit then
    $0002 GPIOB_ODR
    GPIOB_ODR @ $0004 and $0004 = if
	bis!
    else
	bic!
    then
    $00000004 $0000000C GPIOB_MODER tuck bic! bis!
    100 * us
    \ restore PB1 to Input
    $0000000C $0000000C GPIOB_MODER tuck bic! bis!
;

: captest.quickdischarge
    \ start quick discharge
    $2 GPIOB_ODR hbis!
    $00000004 $0000000C GPIOB_MODER tuck bic! bis!
    $6 GPIOB_ODR hbic!
    \ set debug outputs to LOW
    $00C0 GPIOA_ODR hbic!
    \ estimate the time to quick discharge
    high-th-count @ 
    1000000 GetSysClock */
    dup 10 < if
	drop
	5
    else
	5 /
    then
    us
    \ restore PB1 to Analog Input
    $0000000C $0000000C GPIOB_MODER tuck bic! bis!
;    

: adc.getVref.count
    1630
;

: captest.result.![] ( val array-addr #result col -- )
    swap 5 * + cells + !
;
    
: captest.display.@[] ( #result array-addr -- )
    cr
    over 5 * 0 + cells over + @ . ." uS and "
    over 5 * 1 + cells over + @ . ." nS"
    cr
    5 2 do
	over 5 * i + cells over + @ .
    loop
;

: captest.results! ( #result array-addr -- )
    2>r
    high-th-count @ 
    1000000 GetSysClock */mod
    2r@ swap 0 captest.result.![]
    1000 GetSysClock */
    2r@ swap 1 captest.result.![]
    low-th-count @ 2r@ swap 2 captest.result.![]
    high-th-count @ 2r@ swap 3 captest.result.![]
    adc.getVref.count 2r> swap 4 captest.result.![]
;

    
: captest.results.display ( -- )
    high-th-count @ 
    1000000 GetSysClock */mod
    cr . ." uS and " 1000 GetSysClock */ . ." nS"
    cr
    low-th-count @ .
    high-th-count @ .
    adc.getVref.count .
;

: captest.test ( #measures -- )
    \ discharge maximally the capacitor under test (0.5 seconds)
    GetSysClock 5 * high-th-count !
    captest.quickdischarge
    0 high-th-count !
    0 low-th-count !
    dup 0 do
	false sample-done !
	\ start the test - sharge the capacitor, start TIM2 counting
	$4 GPIOB_BSRR
	$1 TIM2_CR1 @ or TIM2_CR1
	! !
	begin
	    sample-done @
	until
	captest.quickdischarge
	$1 TIM2_CR1 bic!
	0 TIM2_CNT !
	$10 TIM2_EGR h!
	i #results < if
	    i result-array captest.results!
	then
    loop
    dup #results > if
	drop
	#results
    then
    0 do
	i result-array captest.display.@[]
    loop
;

: captest.callibrate.comparators ( -- compensation-val )
    \ generate an accurate pulse using one of the system timers,
    \ disconnect capacitor from test fixture,
    \ route TIM1CH3 to PB1 as it already is connected to the test
    \ pin in negative logic (normal state HIGH, pulse LOW) in PWM2 mode,
    \ Configure the TIM1 to output accurate pulses. Since TIM1 and TIM2
    \  have the same clock, the result of the comparators measure adds
    \  the distortions of the measurement (parasitic capacitance of the fixture,
    \  charge resistance of the driver, propagation delay of the comparators,
    \  rise time of the driver pulse, delay time from start of charging and
    \  start of measurement, etc)
    \ subtract the measure time from the pulse time of TIM1CH3 the difference
    \  is the compensation count we need to subtract form each measure due to
    \  comparator and driver distortions.
    \ Store the result in stack
;

\ The value is the multiply of the relation Vr=Vref/Vcc by 1000000
\  The computation result is a statistic Avg, Sigma of 1000 samples
\  over a time period of 0.1 second. This will give us a statistic
\  of the uncertainty in the measurement in terms of the reference
\  to the comparators.
: captest.callibrate.vref ( -- vref-avg vref-variance )
    \ measure Vref in relation to the current Vcc using the ADC
    
;

: captest.callibrate.ChargeRes
    \ measure the resistance of the charging driver in reference to a
    \  reference capacitor. This assures the measurement accuracy can't
    \  be better than the accuracy of the reference capacitor!
;


: captest.callibrate
    0 captest-compensate !
    captest.callibrate.comparators
    captest.callibrate.vref
    captest.callibrate.ChargeRes
;

: captest.calculate ( Vref Rval Tmeas -- ud-cap )
    
;
    
: captest.init
    init.comp.io
    init.comp.comp
    init.comp.gen
    init.comp.intpt
    init.comp.tim2
    false sample-done !
    0 dup low-th-count ! high-th-count !
    
;

captest.init

: test01
    1 TIM2_CR1 bis!
    10 us
    $10 TIM2_EGR h!
    $00200000 EXTI_SWIER bis!
    TIM2_CCR4 @ low-th-count !
    10 us
    $10 TIM2_EGR h!
    $00400000 EXTI_SWIER bis!
    TIM2_CCR4 @ high-th-count !
    2 us
    TIM2_CNT @
    1 TIM2_CR1 bic!
    0 TIM2_CNT !
    cr .
    captest.results.display
;

