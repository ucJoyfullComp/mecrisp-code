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

: adc.isr
;

0 variable low-th-count
0 variable high-th-count
-1 variable cap-charging

: adc_comp.isr
    $00200000 EXTI_PR bit@ if
	\ read TIM2 (32bit timer - @48MHz gives up to 89 seconds measurement time)
	TIM2_CCR4 @ low-th-count !
	$00200000 EXTI_PR !
    then
    $00400000 EXTI_PR bit@ if
	     \ read TIM2 (32bit timer - @48MHz gives up to 89 seconds measurement time)
	    TIM2_CCR4 @ High-th-count !
	    $00400000 EXTI_PR !
    then
    adc.isr
;

: init.comp.intpt
    \ hooking the comparator isr to the ISR hook
    ['] adc_comp.isr irq-adc !
    \ enabling the IRQ for ADC/COMP
    $80 $FF NVIC_IPR3 tuck bic! bis!
    $00001000 NVIC_ISER bis!
    \ Setting the EXTI controller for the comparators to generate IRQ's
    $00600000 EXTI_IMR bis!
    $00600000 EXTI_PR bis!
    $00600000 EXTI_RTSR bis!
    $00600000 EXTI_RTSR bic!
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
    $0008 TIM2_CR1 h!
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
    $0002 GPIOB_OTYPER bic!
    100 * us
    \ restore PB1 to Input
    $00000000 $0000000C GPIOB_MODER tuck bic! bis!
    $0002 GPIOB_OTYPER bic!
;    
    

: captest.test ( 100us -- )
    init.comp.io
    init.comp.comp
    init.comp.gen
    init.comp.intpt
    init.comp.tim2
    begin
	$4 GPIOB_BSRR !
	dup 0 do
	    100 us
	    key? if unloop drop exit then
	loop
	dup 10 / 1+ captest.quickcharge
	$4 GPIOB_BRR !
	dup 0 do
	    100 us
	    key? if unloop drop exit then
	loop
	dup 10 / 1+ captest.quickcharge
    again
    drop
;
