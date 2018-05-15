64 constant sintab_size

: do-sintab ( max haddr -- )
	sintab_size 0 do
		over
		i 360000000 sintab_size */ deg2rad sin
		1000000 + 2000000 */
		dup 0 < if
			drop 0
		then
		over i 2* + h!
	loop
	2drop cr
;

sintab_size 2* buffer: sintab

\ 1s timer toogles green led with interrupt
$20000000 constant NVIC_ISER0_TIM3       \ TIM3 Interrupt 29

$00000002 constant TIM3EN				\TIM3 clock enable

$0001 constant CEN					\ Counter enable
$0080 constant ARPE 				\ Auto-reload preload enable
$0001 constant UIE					\ Update interrupt enable
$0001 constant UIF					\ Update interrupt flag
$0001 constant UG					\ Update generation

TIM3_BASE TIM1_CCR1 + constant TIM3_CCR1
TIM3_BASE TIM1_SR + constant TIM3_SR
TIM3_BASE TIM1_DIER + constant TIM3_DIER
TIM3_BASE TIM1_ARR + constant TIM3_ARR

100000 variable Fs

: tim3-init ( -- )
	TIM3EN RCC_APB1ENR_R bis! \ TIM3 clock enabled
	10 0 do loop
	TIM3EN RCC_APB1RSTR_R bis! \ TIM3 reset enabled
	10 0 do loop
	TIM3EN RCC_APB1RSTR_R bic! \ TIM3 reset cleared
	10 0 do loop

	ARPE TIM3_BASE TIM1_CR1 + bis! 			\ Auto-reload preload enable
	0 TIM3_BASE TIM1_PSC + h!
	GetApb1TimClock Fs @ / 1- TIM3_BASE TIM1_ARR + !	\ timebase - 100khz
	UG TIM3_BASE TIM1_EGR + !				\ Update generation
	$0060 TIM3_BASE TIM1_CCMR1 + h! \ CH1 PWM output
	$0001 TIM3_BASE TIM1_CCER + h!	\ enable CH1 output
	0 TIM3_CCR1 + !
	0 TIM3_DIER h!
	CEN TIM3_BASE TIM1_CR1 + bis! 	\ counter enable
;

: tim3-ccr1-init
	2 6 2* lshift GPIOA_BASE GPIO_MODER + 3 6 2* lshift over bic! bis!
	1 6 lshift GPIOA_BASE GPIO_OTYPER + bic!
	GPIOA_BASE GPIO_OSPEEDR + dup @ $00030000 or swap !
	3 6 2* lshift GPIOA_BASE GPIO_PUPDR + bic!
	GPIOA_BASE GPIO_AFRL + dup @ $0F000000 bic $02000000 or swap !
;

: tim3-dbg-init
	1 15 2* lshift GPIOE_BASE GPIO_MODER + 3 15 2* lshift over bic! bis!
	1 15 lshift GPIOE_BASE GPIO_OTYPER + bic!
	GPIOE_BASE GPIO_OSPEEDR + dup @ $C0000000 or swap !
	3 15 2* lshift GPIOE_BASE GPIO_PUPDR + bic!
	1 15 lshift GPIOE_BASE GPIO_ODR + bic!
;

: tim3-pwm ( n -- )
	TIM3_BASE TIM1_CCR1 + h!
;

1 variable dp
0 variable stopgen
0 variable arridx

: old1-tim3-isr
	TIM3_CCR1 @ dp @ +
	dup	TIM3_ARR @ > if
		0 dp @ - dup dp !
		+
	then
	dup 9 < if
		0 dp @ - dup dp !
		+
	then
	TIM3_CCR1 h!
	\ $8000 GPIOE_BASE GPIO_ODR + xor!
	UIF TIM3_SR bic! 			\ clear interrupt source
;

: old2-tim3-isr
	TIM3_CCR1 @ dp @ +
	dup	TIM3_ARR @ > if
		drop 0
		0 dp !
		1 TIM3_BASE TIM1_CR1 + hbic!
	then
	TIM3_CCR1 h!
	\ $8000 GPIOE_BASE GPIO_ODR + xor!
	UIF TIM3_SR bic! 			\ clear interrupt source
;

: tim3-isr
	arridx @ 2* sintab + h@
	TIM3_CCR1 h!
	arridx @ 1+ dup sintab_size >= if sintab_size - then
	arridx !
	$8000 GPIOE_BASE GPIO_ODR + xor!
	UIF TIM3_SR bic! 			\ clear interrupt source
;

: my_key?
	$20 USART2_BASE USART_SR + hbit@ if
		-1
	else
		0
	then
;

: test-tim3
	0 arridx !
	['] my_key? hook-key? !
	['] tim3-isr irq-tim3 !
	1 29 lshift NVIC_ISER0_R bis!
	tim3-init
	tim3-ccr1-init
	tim3-dbg-init

	TIM3_ARR h@ 9 - sintab do-sintab
	sintab_size 0 do
		9 i 2* sintab + h+!
	loop

	9 TIM3_CCR1 h!
	UIF TIM3_DIER bis!
	begin
	key? until
	0 TIM3_DIER h!
	1 29 lshift NVIC_ISER0_R bic!
	['] serial-key? hook-key? !
	0 TIM3_CCR1 h!
;

: freq-gen ( fmax fmin -- )
	GetApb1TimClock >r
	r>
	drop 2drop
;

: test01
	['] tim3-isr irq-tim3 !
	1 29 lshift NVIC_ISER0_R bis!
	tim3-init
	tim3-ccr1-init
	1 TIM3_CCR1 h!
	UIF TIM3_DIER bis!

	100 1- TIM3_BASE TIM1_PSC + h!
	2 TIM3_CCR1 h!
	50 0 do
		i 1+ 130 + TIM3_ARR h!
		100 ms
		i 1+ 180 + TIM3_ARR h!
		100 ms
	loop
	0 TIM3_DIER h!
	1 29 lshift NVIC_ISER0_R bic!
;

