
\ PWM with 1000ms period and 500ms duty cyle -> output  to PD15, blue LED

$40023800 constant RCC_Base
RCC_Base $30 + constant RCC_AHB1ENR
$00000008 constant GPIODEN		\ IO port D clock enable
RCC_Base $40 + constant RCC_APB1ENR
$00000004 constant TIM4EN		\ TIM4 clock enable

$40020C00 constant PORTD_Base
PORTD_BASE $00 + constant PORTD_MODER    \ Reset 0 Port Mode Register - 00=Input  01=Output  10=Alternate  11=Analog
$80000000 constant MODER15		\ PD15 -> %10: Alternate function mode
PORTD_BASE $04 + constant PORTD_OTYPER   \ Reset 0 Port Output type register - (0) Push/Pull vs. (1) Open Drain
PORTD_BASE $08 + constant PORTD_OSPEEDR  \ Reset 0 Output Speed Register - 00=2 MHz  01=25 MHz  10=50 MHz  11=100 MHz
$80000000 constant OSPEEDR15		\ PD15 -> %10: High speed
PORTD_BASE $0C + constant PORTD_PUPDR    \ Reset 0 Pullup / Pulldown - 00=none  01=Pullup  10=Pulldown
PORTD_BASE $10 + constant PORTD_IDR      \ RO      Input Data Register
PORTD_BASE $14 + constant PORTD_ODR      \ Reset 0 Output Data Register
PORTD_BASE $18 + constant PORTD_BSRR     \ WO      Bit set/reset register   31:16 Reset 15:0 Set
PORTD_BASE $20 + constant PORTD_AFRL     \ Reset 0 Alternate function  low register
PORTD_BASE $24 + constant PORTD_AFRH     \ Reset 0 Alternate function high register
$20000000 constant AFRH15		\ PD15 -> Alternate function: %0010: AF2 (TIM4) 

$40000800 constant TIM4_Base
TIM4_Base $00 + constant TIM4_CR1
$0001 constant CEN			\ Counter enable
$0080 constant ARPE 		\ Auto-reload preload enable
TIM4_Base $14 + constant TIM4_EGR
$0001 constant UG					\ Update generation
TIM4_Base $1C + constant TIM4_CCMR2
$0800 constant OC4PE		\ Output compare 4 preload enable
$6000 constant OC4M 		\ Output compare 4 mode -> 110: PWM mode 1
TIM4_Base $20 + constant TIM4_CCER
$1000 constant CC4E 		\ OC4 signal is output on the corresponding output pin
TIM4_Base $28 + constant TIM4_PSC
TIM4_Base $2C + constant TIM4_ARR
TIM4_Base $40 + constant TIM4_CCR4


: pd15-init ( -- )
	GPIODEN RCC_AHB1ENR bis!		\ IO port D clock enabled
	MODER15  PORTD_MODER bis! 		\ alternate function PD15
	OSPEEDR15 PORTD_OSPEEDR bis! 	\ high speed PD15
	AFRH15 PORTD_AFRH bis! 			\ AF2 (TIM4) to PD15
;

: tim4-init ( -- )
	TIM4EN RCC_APB1ENR bis!	\ TIM4 clock enabled
	ARPE TIM4_CR1 ! 				\ Auto-reload preload enable
	#8000 TIM4_PSC !				\ prescaler 8'000'000 Hz / 8000 -> 1000 Hz
	#1000 TIM4_ARR !				\ 1000 ms period
	#500 TIM4_CCR4 !				\ 500 ms duty cycle
	CC4E TIM4_CCER ! 				\ OC4 signal is output on the corresponding output pin
	OC4M OC4PE or TIM4_CCMR2 ! 		\ PWM mode 2, Output compare 4 preload enable
	UG TIM4_EGR !					\ Update generation
	CEN TIM4_CR1 bis! 				\ counter enable
;

\ initialize hw and starts pwm output without cpu load
: pwm-start ( -- )
	pd15-init
	tim4-init
;

\ set new period/dutycyle in ms for pwm
: set-pwm ( period dutycyle -- )
	TIM4_CCR4 !	\ duty cycle
	TIM4_ARR !	\ period
;



