\ application for charge pump regulated voltage mutiplier
\ the pump by default works at a clock of 200KHz
\  pin PA6 is phase 1 of the clock
\  pin PA7 is phase 2 of the clock
\  pin PC1 is the analog input to sample the output for the feedback loop
\
\ uzi

\ Bits for RCC_APB2ENR:
1  8 lshift constant ADC1EN
1  9 lshift constant ADC2EN
1 10 lshift constant ADC3EN

\ Bits for ADC_SR:
  2 constant ADC_SR_EOC
$10 constant ADC_SR_OVR

\ Bits for ADC_CR2:
1           constant ADC_CR2_ADON
2           constant ADC_CR2_CONT
1 30 lshift constant ADC_CR2_SWSTART

1 23 lshift constant TSVREFE  \ Temperature sensor and Vrefint enable
1 22 lshift constant VBATE    \ Vbat enable

\ Tim3
$20000000 constant NVIC_ISER0_TIM3       \ TIM3 Interrupt 29

$00000002 constant TIM3EN				\ TIM3 clock enable

$0001 constant TIM_CR1_CEN					\ Counter enable
$0080 constant TIM_CR1_ARPE 				\ Auto-reload preload enable
$0001 constant TIM_DIER_UIF					\ Update interrupt enable
$0001 constant TIM_SR_UIF					\ Update interrupt flag
$0001 constant TIM_EGR_UG					\ Update generation

TIM3_BASE TIM1_CCR1 + constant TIM3_CCR1
TIM3_BASE TIM1_CCR2 + constant TIM3_CCR2
TIM3_BASE TIM1_CCR3 + constant TIM3_CCR3
TIM3_BASE TIM1_CCR4 + constant TIM3_CCR4
TIM3_BASE TIM1_SR + constant TIM3_SR
TIM3_BASE TIM1_DIER + constant TIM3_DIER
TIM3_BASE TIM1_ARR + constant TIM3_ARR

200000 variable Fs

: init-analog ( channel -- ) \ Enable clock for ADC1
	ADC1EN RCC_APB2ENR_R bis!

  0 ADC1_BASE ADC_CR1 + !
  ADC_CR2_ADON ADC1_BASE ADC_CR2 + ! \ Enable ADC

  1 20 lshift ADC1_BASE ADC_SQR1 + ! \ One conversion for this sequence
  ADC1_BASE ADC_SQR3 + !             \ Select channel
;

: close-analog
  0 ADC1_BASE ADC_CR2 + ! \ Disable ADC
;

: analog ( -- u )
  ADC_CR2_SWSTART ADC1_BASE ADC_CR2 + bis! \ Start conversion

  \ Wait for conversion to finish:
  begin ADC_SR_EOC ADC1_BASE ADC_SR + bit@ until

  ADC1_BASE ADC_DR + @ \ Fetch measurement
;

: GetTim3PrescaledClock ( -- n )
	GetApb1TimClock TIM3_BASE TIM1_PSC + h@ 1+ /
;

: tim3-init ( -- )
	TIM3EN RCC_APB1ENR_R bis! \ TIM3 clock enabled
	10 0 do loop
	TIM3EN RCC_APB1RSTR_R bis! \ TIM3 reset enabled
	10 0 do loop
	TIM3EN RCC_APB1RSTR_R bic! \ TIM3 reset cleared
	10 0 do loop

	TIM_CR1_ARPE TIM3_BASE TIM1_CR1 + bis! 			\ Auto-reload preload enable
	1 5 lshift TIM3_BASE TIM1_CR1 + 3 5 lshift over bic! bis! \ CMS mode 1 sett ( center aligned mode selection )
	GetApb1TimClock 65536 Fs @ * < if
		GetApb1TimClock Fs @ / 1- TIM3_ARR !	\ timebase - 100khz
	else
		GetApb1TimClock 65536 Fs @ * / TIM3_BASE TIM1_PSC + h!
		GetApb1TimClock TIM3_BASE TIM1_PSC + h@ 1+ / Fs @ / 1- TIM3_ARR h!
	then
	TIM_EGR_UG TIM3_BASE TIM1_EGR + !				\ Update generation
	$7060 TIM3_BASE TIM1_CCMR1 + h! \ CH1 PWM output, CH2 High on Match
	$0011 TIM3_BASE TIM1_CCER + h!	\ enable CH1 and CH2 output
	0 TIM3_CCR1 + !
	TIM3_ARR h@ TIM3_CCR2 h!
	1 TIM3_DIER h!	\ enable UP interrupt
	TIM_CR1_CEN TIM3_BASE TIM1_CR1 + bis! 	\ counter enable
;

: tim3-ccr-init
	2 6 2* lshift GPIOA_BASE GPIO_MODER + 3 6 2* lshift over bic! bis!
	1 6 lshift GPIOA_BASE GPIO_OTYPER + hbic!
	GPIOA_BASE GPIO_OSPEEDR + dup @ 3 6 2* lshift or swap !
	3 6 2* lshift GPIOA_BASE GPIO_PUPDR + bic!
	GPIOA_BASE GPIO_AFRL + dup
	@	$F 6 4 * lshift bic $2 6 4 * lshift or
	swap !
	2 7 2* lshift GPIOA_BASE GPIO_MODER + 3 7 2* lshift over bic! bis!
	1 7 lshift GPIOA_BASE GPIO_OTYPER + hbic!
	GPIOA_BASE GPIO_OSPEEDR + dup @ 3 7 2* lshift or swap !
	3 7 2* lshift GPIOA_BASE GPIO_PUPDR + bic!
	GPIOA_BASE GPIO_AFRL + dup
	@	$F 7 4 * lshift bic $2 7 4 * lshift or
	swap !
;

: tim3-dbg-init
	1 15 2* lshift GPIOE_BASE GPIO_MODER + 3 15 2* lshift over bic! bis!
	1 15 lshift GPIOE_BASE GPIO_OTYPER + bic!
	GPIOE_BASE GPIO_OSPEEDR + dup @ 3 15 2* lshift or swap !
	3 15 2* lshift GPIOE_BASE GPIO_PUPDR + bic!
	1 15 lshift GPIOE_BASE GPIO_ODR + bic!
;

: tim3-pwm ( n -- )
	TIM3_BASE TIM1_CCR1 + h!
;

1 variable dp
0 variable Vout-target
0 variable Tim3-count
32 constant samp-buf-size
0 variable Vout-samp-arr-idx
samp-buf-size 2* buffer: Vout-samp

: tim3-isr
	1 Tim3-count +!
	1 TIM3_SR bic! 			\ clear interrupt source
	analog
	Vout-samp
	Vout-samp-arr-idx @ samp-buf-size = if
		0 Vout-samp-arr-idx !
	then
	Vout-samp-arr-idx @ 2* + h!
	1 Vout-samp-arr-idx +!
;

: cpump-boost ( outV -- )
  init-analog \ Enable clock for ADC1
  %11 1 2* lshift GPIOC_BASE GPIO_MODER + bis! \ Switch PC1 to analog mode

	['] tim3-isr irq-tim3 !
	1 29 lshift NVIC_ISER0_R bis!
	tim3-init
	tim3-ccr-init

	TIM3_ARR h@ 10 / TIM3_CCR1 h!
	TIM_DIER_UIF TIM3_DIER bis!
	begin
		Tim3-count @ 10 >= if
			0 Tim3-count !
			\ calculate the value to be put in CCR1 according to Vout
			Vout-samp-arr-idx @ 0 > if
				0 Vout-samp-arr-idx !
				0 Tim3-count !
				Vout-samp h@
				Vout-samp h@ TIM3_ARR h@ 4095 */
				\ store tha calculated value to TIM3_CCR1 and TIM3_CCR2
				TIM3_CCR1 over
				TIM3_ARR h@ swap - TIM3_CCR2
				h! h!
			then
		then
	again
	0 TIM3_DIER h!
	1 29 lshift NVIC_ISER0_R bic!
	0 TIM3_CCR1 h!
	TIM3_ARR h@ TIM3_CCR2 h!
	close-analog
;

: cpump ( -- )
  init-analog \ Enable clock for ADC1
  %11 1 2* lshift GPIOC_BASE GPIO_MODER + bis! \ Switch PC1 to analog mode

	['] tim3-isr irq-tim3 !
	1 29 lshift NVIC_ISER0_R bis!
	tim3-init
	tim3-ccr-init

	TIM3_ARR h@ 2/ 10 - TIM3_CCR1 h!
	TIM3_ARR h@ TIM3_CCR1 h@ - TIM3_CCR2 h!
;


