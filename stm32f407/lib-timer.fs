\ 1s timer toogles green led with interrupt 
$E000E100 constant NVIC_EN0_R              	\ IRQ 0 to 31 Set Enable Register
$10000000 constant NVIC_EN0_INT28       \ TIM2 Interrupt 28

$40023800 constant RCC_Base
RCC_Base $30 + constant RCC_AHB1ENR
RCC_Base $40 + constant RCC_APB1ENR
$0001 constant TIM2EN				\TIM2 clock enable

$40020C00 constant PORTD_Base
PORTD_BASE $00 + constant PORTD_MODER   \ Port Mode Register - 00=Input  01=Output  10=Alternate  11=Analog
%01 24 lshift constant MODER12		\ PD12 -> %01: Output mode
PORTD_BASE $14 + constant PORTD_ODR     \ Output Data Register
$1000 constant led-green

$40000000 constant TIM2_Base
TIM2_Base $00 + constant TIM2_CR1
$0001 constant CEN					\ Counter enable
$0080 constant ARPE 				\ Auto-reload preload enable
TIM2_Base $0C + constant TIM2_DIER
$0001 constant UIE					\ Update interrupt enable
TIM2_Base $10 + constant TIM2_SR
$0001 constant UIF					\ Update interrupt flag
TIM2_Base $14 + constant TIM2_EGR
$0001 constant UG					\ Update generation
TIM2_Base $28 + constant TIM2_PSC
TIM2_Base $2C + constant TIM2_ARR


: tim2-init ( -- )
	TIM2EN RCC_APB1ENR bis! \ TIM2 clock enabled
	ARPE TIM2_CR1 bis! 			\ Auto-reload preload enable
	#8000 TIM2_PSC !			\ prescaler -> 8'000'000 Hz / 8000 -> 1000 Hz
	#1000 TIM2_ARR !			\ period -> reload after 1000 ms 
	UG TIM2_EGR !				\ Update generation
	CEN TIM2_CR1 bis! 			\ counter enable
	MODER12 PORTD_MODER bis! 	\ green LED as output
;

: tim2-irq-handler
	led-green PORTD_ODR xor! 	\ toggle green led
	UIF TIM2_SR bic! 			\ clear interrupt source
;

: tim2-irq-enable
	UIE TIM2_DIER bis!					\ Update interrupt enabled
	['] tim2-irq-handler irq-tim2 !  	\ Hook for handler
	NVIC_EN0_INT28 NVIC_EN0_R ! 		\ Enable TIM2 Interrupt in global Interrupt Controller
;

\ initalize hw and starts timer with intterupts
: tim2-start ( -- )
	tim2-init
	tim2-irq-enable
;

\ polling test
: tim2-polling ( -- )
	tim2-init
	begin 
		UIF TIM2_SR bit@  if
			tim2-irq-handler
		then
	key? until
;




