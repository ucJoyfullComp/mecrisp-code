NVIC_BASE NVIC_ISER0 + constant NVIC_EN0_R
$20000000 constant NVIC_EN0_INT29          \ TIM3 interrupt 29
$40000000 constant NVIC_EN0_INT30          \ TIM4 interrupt 30

TIM1_BASE TIM_CR1 + constant TIM1_CR1_R
TIM3_BASE TIM_CR1 + constant TIM3_CR1_R
TIM3_BASE TIM_ARR + constant TIM3_ARR_R

0 variable FreqMeas
0 variable FreqRdy
0 variable Tim3ArrVal
0 variable Tim4ArrVal

: tim3isr
	1 TIM1_CR1_R hbic! 
	1 TIM3_CR1_R hbic! 
	1 TIM3_BASE TIM_SR + hbic! 
	$FFFF TIM3_ARR_R h! 
	1 TIM_DIER 2dup
	TIM3_BASE + hbic!
	TIM4_BASE + hbis!

	$10000 TIM2_BASE TIM_CNT + h@ * 
	TIM1_BASE TIM_CNT + h@ or 
	FreqMeas ! 
	-1 FreqRdy ! 
	0 TIM_CNT 2dup 
	TIM2_BASE + h! 
	TIM1_BASE + h! 
	$2000 GPIOC_BASE GPIO_ODR + hxor! 
	
	$0001 TIM1_CR1_R hbis! 
	$0001 TIM3_CR1_R hbis! 
;

: tim4isr
	Tim3ArrVal @ TIM3_ARR_R !
	1 TIM4_BASE 2dup 
	TIM_DIER + hbic!
	TIM_SR + hbic!
	1 TIM3_BASE TIM_DIER + hbis!
;

: init-count
	$800 RCC_APB2ENR_R bis! \ enable clock to TIM1
	10 0 do loop
	$800 RCC_APB2RSTR_R bis!
	20 0 do loop
	$800 RCC_APB2RSTR_R bic!
	$7 RCC_APB1ENR_R bis! \ enable clock to TIM2, TIM3, TIM4
	10 0 do loop
	$7 RCC_APB1RSTR_R bis!
	20 0 do loop
	$7 RCC_APB1RSTR_R bic!
	\ configure TIM1 ETR input
	GPIOA_BASE GPIO_CRH + dup @ $F0000 bic $40000 or swap !
	\ set the TIM3 isr
	['] tim3isr irq-tim3 !
	\ enable timer TIM3 interrupt in NVIC
	NVIC_EN0_INT29 NVIC_EN0_R ! \ Enable TIM3 Interrupt in global Interrupt Controller

	\ set the TIM4 isr
	['] tim4isr irq-tim4 !
	\ enable timer TIM4 interrupt in NVIC
	NVIC_EN0_INT30 NVIC_EN0_R ! \ Enable TIM4 Interrupt in global Interrupt Controller

	\ enable ETR input for TIM1
	GPIOA_BASE GPIO_CRL + dup @ $F bic $4 or swap ! 
	\ TIM3 setup the gate time ( 1 Second )
	GetTIM2-4Clk $10000 /mod 1- Tim4ArrVal ! 1- Tim3ArrVal !
	
	$0004 TIM3_BASE TIM_CR1 + h!
	$0020 TIM3_BASE TIM_CR2 + h!
	$0000 TIM3_BASE TIM_SMCR + h!
	$0000 TIM3_BASE TIM_DIER + h!
	$0000 TIM3_BASE TIM_SR + h!
	$0000 TIM3_BASE TIM_CNT + h!
	$0000 TIM3_BASE TIM_PSC + h!
	$FFFF TIM3_BASE TIM_ARR + h!

	$0000 TIM4_BASE TIM_CR1 + h!
	$0000 TIM4_BASE TIM_CR2 + h!
	$0020 TIM4_BASE TIM_SMCR + h!
	TIM4_BASE TIM_SMCR + dup @ $0007 bic $0007 or swap h!
	$0001 TIM4_BASE TIM_DIER + h!
	$0000 TIM4_BASE TIM_SR + h!
	$0000 TIM4_BASE TIM_CNT + h!
	$0000 TIM4_BASE TIM_PSC + h!
	Tim4ArrVal @ TIM4_BASE TIM_ARR + h!
	
	\ TIM1 setup counting events on pin A0, generate event UI to control
	\  
	$0004 TIM1_BASE TIM_CR1 + h!
	$0020 TIM1_BASE TIM_CR2 + h!
	$4000 TIM1_BASE TIM_SMCR + h!
	$0000 TIM1_BASE TIM_DIER + h!
	$0000 TIM1_BASE TIM_SR + h!
	$0000 TIM1_BASE TIM_CNT + h!
	$0000 TIM1_BASE TIM_PSC + h!
	$FFFF TIM1_BASE TIM_ARR + h!

	\ TIM2 setup counting events from TIM1 as slave
	\  
	$0000 TIM2_BASE TIM_CR1 + h!
	$0000 TIM2_BASE TIM_CR2 + h!
	$0000 TIM2_BASE TIM_SMCR + h!
	TIM2_BASE TIM_SMCR + dup @ $0007 bic $0007 or swap h!
	$0000 TIM2_BASE TIM_DIER + h!
	$0000 TIM2_BASE TIM_SR + h!
	$0000 TIM2_BASE TIM_CNT + h!
	$0000 TIM2_BASE TIM_PSC + h!
	$FFFF TIM2_BASE TIM_ARR + h!

	\ init PC13 for output PP to control internal LED
	GPIOC_BASE GPIO_CRH + dup
	@ $00F00000 bic $200000 or swap !
	$2000 GPIOC_BASE GPIO_ODR + hbis!
;

: start-count
	0 FreqRdy !
	0 FreqMeas !
	0 TIM_CNT 2dup 2dup 2dup
	TIM1_BASE + !
	TIM2_BASE + !
	TIM3_BASE + !
	TIM4_BASE + !
	\ enable timer TIM2
	$0001 TIM2_BASE TIM_CR1 + hbis!
	\ enable timer TIM4
	$0001 TIM4_BASE TIM_CR1 + hbis!
	\ enable timer TIM1
	$0001 TIM1_BASE TIM_CR1 + hbis!
	\ enable timer TIM3
	$0001 TIM3_BASE TIM_CR1 + hbis!
;

: stop-count
	\ disable timer TIM3
	$0001 TIM3_BASE TIM_CR1 + hbic!
	\ disable timer TIM1
	$0001 TIM1_BASE TIM_CR1 + hbic!
	\ disable timer TIM4
	$0001 TIM4_BASE TIM_CR1 + hbic!
	\ disable timer TIM2
	$0001 TIM2_BASE TIM_CR1 + hbic!
	0 TIM_CNT 2dup 2dup 2dup
	TIM1_BASE + !
	TIM2_BASE + !
	TIM3_BASE + !
	TIM4_BASE + !
	0 FreqRdy !
	0 FreqMeas !
;

: fcounter
	init-count
	start-count
	eint
	begin
		FreqRdy @ if
			FreqMeas @
			0 FreqRdy !
			cr
			." Freq: " .
		then
	again
	stop-count
;

: init
	115200 BaudRate !
	8000000 9 allinit
	fcounter
;

