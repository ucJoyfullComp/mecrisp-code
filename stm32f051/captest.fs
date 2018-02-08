\ Version Does not work!

\ Version 4 - Routing the Comp outputs on pins PA6 (comp1) PA7 (comp2)
\  to TIM2 TI3, TI4 pins Alternate routed to pins PB10, PB11 using wires
\  trigrring capture of the CNT register of TIM2 into CCR3, CCR4.
\  Generating two interrupts One on capture of TI4 input,
\  at which we transfer the contents of the CCR3, CCR4 to variables
\  low-th-count, high-th-count, signalling the availability of measurment
\  with mead-done flag variable, starting quick discharge and
\  updating the value of ARR to CCR4*9/8+100.
\  The second interrupt request in the ISR of TIM2 is UI at reaching
\  the value in ARR will generate this interrupt while updating the CNT value
\  and we will stop the counter, stop the quick discharge
\  To start a measurement we will wait for meas-done, then start the TIM2,
\  and start charging the cap.

0 variable low-th-count
0 variable high-th-count
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
    $0004 GPIOB_ODR bic!
;

\ change: here we will not use internal comparators events.
\  or interrupts
: init.comp.intpt
    \ Setting the EXTI controller for the comparators to generate Events
    $00600000 EXTI_IMR bic!
\    $00600000 EXTI_IMR bis!
\    $00600000 EXTI_EMR bis!
\    $00600000 EXTI_RTSR bis!
\    $00600000 EXTI_FTSR bic!
;

: tim2.isr
    TIM2_SR
    1 over bit@ if \ UI interrupt
	1 over bic!
	\ $FFFFFFFF TIM2_ARR !
	\ Changed: measurement gate no more than 1 second
	SysClockVar @ TIM2_ARR !
	meas-done @ if
	    1 TIM2_CR1 bic!
	    1 TIM2_EGR hbis!
	    0 TIM2_CCR4 !
	then
    then
    $10 over bit@ if \ CC4I interrupt
	\ quick discharge
	$6 GPIOB_ODR bic!
	$4 GPIOB_MODER bis!
	TIM2_CNT @ dup 8 rshift + 100 + TIM2_ARR !
	true meas-done !
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
    \ setup capture for ch3, ch4
    $0101 TIM2_CCMR2_Input h!
    $1100 TIM2_CCER h!
    \ init PB10, PB11 to input the TI3, TI4 events
    $00F00000 GPIOB_MODER bic!
    $00F00000 GPIOB_PUPDR bic!
    $00002200 $0000FF00 GPIOB_AFRH tuck bic! bis!
    \ enable interrupt request on UI, CC4 events
    $0011 TIM2_DIER h!
    \ init counter values
    0 TIM2_CNT !
    0 TIM2_PSC h!
    \ init reload value register
    \    $FFFFFFFF TIM2_ARR !
    \ change to maximum gate time of 1 second.
    GetSysClock TIM2_ARR !
    \ irq setup
    ['] tim2.isr irq-tim2 !
    $80000000 $FF000000 NVIC_IPR3 tuck bic! bis!
    $00008000 NVIC_ISER bis!
;

: captest.start
    false meas-done !
    \ start TIM2 counting
    0 TIM2_CNT !
    1 TIM2_CR1 bis!
    \ start charging the Capacitor under test
    $0000000C GPIOB_MODER bic!
    $0004 GPIOB_ODR hbis!
;

: captest.test ( #meas -- )
    captest.start
    0 do
	begin meas-done @ key? or until
	meas-done @ if \ process measurement
	    cr low-th-count @ . high-th-count @ .
	    false meas-done !
	    captest.start
	then
    loop
;

: captest.init
    init.comp.io
    init.comp.comp
    init.comp.gen
    init.comp.intpt
    init.comp.tim2
    false meas-done !
    0 dup
    low-th-count !
    high-th-count !
;

\ captest.init

