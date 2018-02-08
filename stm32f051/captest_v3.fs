\ Version 3 of Capacitor Test. Not working correctly.
\  Probably event generation in TIM2 ISR not functioning correctly.
\  See Version 2. same problem probably, but here using the TIM2 ISR
\  instead of the EXTI interrupt.

0 variable low-th-count
0 variable high-th-count
-1 variable cap-charging
0 variable meas-done

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
    $04B10411 COMP_CSR !
;

: init.comp.gen
    $00000010 $0000003C GPIOB_MODER tuck bic! bis!
    $0006 GPIOB_OTYPER bic!
    $0000003C GPIOB_OSPEEDR bis!
    $0000003C GPIOB_PUPDR bic!
    $0006 GPIOB_ODR bic!
;

: init.comp.intpt
    \ Setting the EXTI controller for the comparators to generate Events
    $00600000 EXTI_EMR bis!
    $00600000 EXTI_RTSR bis!
    $00600000 EXTI_FTSR bic!
;

: tim2.isr
    TIM2_SR
    1 over bit@ if \ UI interrupt
	1 over bic!
	$FFFFFFFF TIM2_ARR !
	meas-done @ if
	    1 TIM2_CR1 bic!
	    1 TIM2_EGR hbis!
	    0 TIM2_CCR4 !
	then
    then
    $10 over bit@ if \ CC4I interrupt
	TIM2_CCR4 @
	COMP_CSR @ $40004000 and $40004000 = if
	    \ quick discharge
	    $6 GPIOB_ODR bic!
	    $8 GPIOB_MODER bic!
	    TIM2_CNT @ dup 8 rshift + TIM2_ARR !
	    high-th-count !
	    true meas-done !
	else
	    COMP_CSR @ $40004000 and $00004000 = if
		low-th-count !
	    else
		drop
	    then
	then
    then
    0 swap h!
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
    \ it will operate in a one pulse mode ( every time it stops,
    \ CEN needs to be asserted again.
    $0000 TIM2_CR1 h!
    $0100 TIM2_CCMR2_Input h!
    $1000 TIM2_CCER h!
    $0011 TIM2_DIER h!
    0 TIM2_CNT !
    0 TIM2_PSC h!
    $FFFFFFFF TIM2_ARR !
    \ irq setup
    ['] tim2.isr irq-tim2 !
    $80000000 $FF000000 NVIC_IPR3 tuck bic! bis!
    $00008000 NVIC_ISER bis!
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
    $0002 GPIOB_OTYPER bic!
    100 * us
    \ restore PB1 to Input
    $00000000 $0000000C GPIOB_MODER tuck bic! bis!
    $0002 GPIOB_OTYPER bic!
;

: captest.start
    \ start TIM2 counting
    1 TIM2_CR1 bis!
    \ start charging the Capacitor under test
    $0000000C GPIOB_MODER bis!
    $0004 GPIOB_ODR bis!
;

: captest.test ( #meas -- )
    init.comp.io
    init.comp.comp
    init.comp.gen
    init.comp.intpt
    init.comp.tim2
    captest.start
    0 do
	begin meas-done @ key? or until
	meas-done @ if \ process measurement
	    cr low-th-count . high-th-count .
	    false meas-done !
	    captest.start
	then
    loop
;
