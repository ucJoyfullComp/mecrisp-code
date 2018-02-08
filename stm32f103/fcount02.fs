NVIC_BASE NVIC_ISER0 + constant NVIC_EN0_R
$20000000 constant NVIC_EN0_INT29          \ TIM3 interrupt 29
$00000040 constant NVIC_EN1_INT06          \ USART2 interrupt 38

TIM1_BASE TIM_CR1 + constant TIM1_CR1_R
TIM3_BASE TIM_CR1 + constant TIM3_CR1_R
TIM3_BASE TIM_ARR + constant TIM3_ARR_R

0 variable FreqMeas
0 variable FreqRdy
0 variable Tim3ArrVal
0 variable Tim4ArrVal

: usart2-init
	$20000 RCC_APB1ENR_R bis! \ enable clock to TIM1
	10 0 do loop
	$20000 RCC_APB1RSTR_R bis!
	20 0 do loop
	$20000 RCC_APB1RSTR_R bic!
	\ configure USART2 TX,RX
	GPIOA_BASE GPIO_CRL + dup @ $FF00 bic $4A00 or swap !
	\ enable timer TIM3 interrupt in NVIC
\	NVIC_EN1_INT06 NVIC_EN1_R ! \ Enable USART2 Interrupt in global Interrupt Controller

	\ USART2 setup
	\  
	GetPClk1 115200 / USART2_BASE USART_BRR + h!
	$2000 USART2_BASE USART_CR1 + h!
	$0000 USART2_BASE USART_CR2 + h!
	$0000 USART2_BASE USART_CR3 + h!
	$03FF USART2_BASE USART_SR + hbic!
;

: usart2-wait-to-tx ( -- )
	begin $80 USART2_BASE USART_SR + hbit@ not until
;

\ blocking transmit of char
: usart2-tx-char ( char -- )
	usart2-wait-to-tx
	USART2_BASE USART_DR + h!
	$8 USART2_BASE USART_CR1 + hbis!
;

\ blocking transmit of string
: usart2-tx-str ( c-addr len -- )
	0 do
		dup i + c@ usart2-tx-char
	loop
	drop
;

: usart2-type usart2-tx-str ; immediate

: tim3isr
	2 TIM3_BASE TIM_SR + hbit@ if
		2490 0 do i drop loop
		1 TIM1_CR1_R hbic! 
		2 TIM3_BASE TIM_SR + hbic!
		$10000 TIM2_BASE TIM_CNT + h@ * 
		TIM1_BASE TIM_CNT + h@ or 
		FreqMeas ! 
		-1 FreqRdy ! 
		0 TIM_CNT 2dup 
		TIM2_BASE + h! 
		TIM1_BASE + h! 
		$2000 GPIOC_BASE GPIO_ODR + hxor! 
	then

	1 TIM3_BASE TIM_SR + hbit@ if
		$0001 TIM1_CR1_R hbis!
		1 TIM3_BASE TIM_SR + hbic!
	then
;

: init-count
	$800 RCC_APB2ENR_R bis! \ enable clock to TIM1
	10 0 do loop
	$800 RCC_APB2RSTR_R bis!
	20 0 do loop
	$800 RCC_APB2RSTR_R bic!
	$3 RCC_APB1ENR_R bis! \ enable clock to TIM2, TIM3
	10 0 do loop
	$3 RCC_APB1RSTR_R bis!
	20 0 do loop
	$3 RCC_APB1RSTR_R bic!
	\ configure TIM1 ETR input
	GPIOA_BASE GPIO_CRH + dup @ $F0000 bic $40000 or swap !
	\ set the TIM3 isr
	['] tim3isr irq-tim3 !
	\ enable timer TIM3 interrupt in NVIC
	NVIC_EN0_INT29 NVIC_EN0_R ! \ Enable TIM3 Interrupt in global Interrupt Controller

	\ enable ETR input for TIM1
	GPIOA_BASE GPIO_CRL + dup @ $F bic $4 or swap ! 
	\ TIM3 setup the gate time ( 1 Second )
	
	$0004 TIM3_BASE TIM_CR1 + h!
	$0040 TIM3_BASE TIM_CR2 + h!
	$0000 TIM3_BASE TIM_SMCR + h!
	$0003 TIM3_BASE TIM_DIER + h!
	$0000 TIM3_BASE TIM_SR + h!
	$0000 TIM3_BASE TIM_CNT + h!
	9999  TIM3_BASE TIM_PSC + h!
	7299  TIM3_BASE TIM_ARR + h!
	$60   TIM3_BASE TIM_CCMR1 + h!
	7199  TIM3_BASE TIM_CCR1 + h!

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

	usart2-init
;

: start-count
	0 FreqRdy !
	0 FreqMeas !
	0 TIM_CNT 2dup 2dup
	TIM1_BASE + !
	TIM2_BASE + !
	TIM3_BASE + !
	\ enable timer TIM2
	$0001 TIM2_BASE TIM_CR1 + hbis!
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
	\ disable timer TIM2
	$0001 TIM2_BASE TIM_CR1 + hbic!
	0 TIM_CNT 2dup 2dup
	TIM1_BASE + !
	TIM2_BASE + !
	TIM3_BASE + !
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
			s" Freq: " usart2-type
			0 <# # #S #> usart2-type
		then
	again
	stop-count
;

\ : init
\ 	115200 BaudRate !
\ 	8000000 9 allinit
\ 	fcounter
\ ;

