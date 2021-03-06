
\ ADC of 4 channels
\ PA15 -> external trigger input for ADC
\ PC1 -> AIN11
\ PC2 -> AIN12
\ PC4 -> AIN14
\ PC5 -> AIN15
\ PD13 -> orange LED toggles in interrupt

$E000E100 constant NVIC_EN0_R              \ IRQ 0 to 31 Set Enable Register
$00040000 constant NVIC_EN0_INT18	\ ADC Interrupt 18

$40023800 constant RCC_Base
RCC_Base $30 + constant RCC_AHB1ENR
$00000001 constant GPIOAEN		\ IO port A clock enable
$00000004 constant GPIOCEN		\ IO port C clock enable
$00000008 constant GPIODEN		\ IO port D clock enable
RCC_Base $44 + constant RCC_APB2ENR
1 8 lshift constant ADC1EN				\ ADC1 clock enable
1 14 lshift constant SYSCFGEN		\ System configuration controller clock enable
	
$40020000 constant PORTA_Base
PORTA_BASE $00 + constant PORTA_MODER    \ Reset 0 Port Mode Register - 00=Input  01=Output  10=Alternate  11=Analog
\		%00 30 lshift constant MODER15		\ PA15 -> %11: Input mode
PORTA_BASE $10 + constant PORTA_IDR      \ Input Data Register
1 constant USER_BTN				\ PA0 -> User button
	
$40020800 constant PORTC_Base
PORTC_BASE $00 + constant PORTC_MODER    \ Reset 0 Port Mode Register - 00=Input  01=Output  10=Alternate  11=Analog
%11 2 lshift constant MODER1		\ PC1 -> %11: Analog mode
%11 4 lshift constant MODER2		\ PC2 -> %11: Analog mode
%11 8 lshift constant MODER4		\ PC4 -> %11: Analog mode
%11 10 lshift constant MODER5		\ PC5 -> %11: Analog mode
	
$40020C00 constant PORTD_Base
PORTD_BASE $00 + constant PORTD_MODER   \ Port Mode Register - 00=Input  01=Output  10=Alternate  11=Analog
%01 26 lshift constant MODER13				\ PD13 -> %01: Output mode
PORTD_BASE $14 + constant PORTD_ODR	\ Output Data Register
$2000 constant led-orange

$40012000 constant ADC1_Base
ADC1_Base $00 + constant ADC1_SR
$0004 constant JEOC		\ Injected channel end of conversion
$0008 constant JSTRT	\ Injected channel start flag
ADC1_Base $04 + constant ADC1_CR1
$0080 constant JEOCIE	\ Interrupt enable for injected channels
$0100 constant SCAN	\ Scan mode
ADC1_Base $08 + constant ADC1_CR2
$0001 constant ADON
1 22 lshift constant JSWSTART		\ Start conversion of injected channels
%01 20 lshift constant JEXTEN 		\ External trigger enable for injected channels -> 01: Trigger on rising edge
%1111 16 lshift constant JEXTSEL 	\ External event select for injected group -> 1111: EXTI line15
ADC1_Base $0C + constant ADC1_SMPR1
%010 15 lshift constant SMP15
%010 12 lshift constant SMP14
%010 06 lshift constant SMP12
%010 03 lshift constant SMP11
ADC1_Base $14 + constant ADC1_JOFR1
ADC1_Base $18 + constant ADC1_JOFR2
ADC1_Base $1C + constant ADC1_JOFR3
ADC1_Base $20 + constant ADC1_JOFR4
ADC1_Base $38 + constant ADC1_JSQR
%11 20 lshift constant JL 		\ Injected sequence length -> 11: 4 conversions
11 15 lshift constant JSQ4 		\ 4th conversion in injected sequence CH11
12 10 lshift constant JSQ3 		\ 3th conversion in injected sequence CH12
14 5 lshift constant JSQ2 		\ 2th conversion in injected sequence CH14
15 0 lshift constant JSQ1 		\ 1th conversion in injected sequence CH15
ADC1_Base $3C + constant ADC1_JDR1
ADC1_Base $40 + constant ADC1_JDR2
ADC1_Base $44 + constant ADC1_JDR3
ADC1_Base $48 + constant ADC1_JDR4
	
: ports-init
	GPIOAEN RCC_AHB1ENR bis!				\ IO port A clock enabled
	GPIOCEN RCC_AHB1ENR bis!				\ IO port C clock enabled
	GPIODEN RCC_AHB1ENR bis!				\ IO port D clock enabled
	MODER13  PORTD_MODER bis! 				\ output function PD13
	MODER1 MODER2 or MODER4 or MODER5 or PORTC_MODER bis! 	\ analog input PortC pins
;
	

: adc1-init ( -- )  
	ADC1EN RCC_APB2ENR bis! 							\ Enable clock for ADC1
	SCAN JEOCIE or ADC1_CR1 bis!						\ Scan mode
	JL ADC1_JSQR bis!	\ Injected sequence length: 4 conversions
	JSQ4 JSQ3 or JSQ2 or JSQ1 or ADC1_JSQR bis!	\ injected sequence CH11, CH12, CH14, CH15
	SMP11 SMP12 or SMP14 or SMP15 or ADC1_SMPR1  bis!	\ Channel x sampling time selection
	ADON ADC1_CR2 bis! 									\ Enable ADC
;

: adc1-irq-handler
	led-orange PORTD_ODR xor! 	\ toggle orange led
	\ Fetch measurement
	ADC1_JDR4 @ u.
	ADC1_JDR3 @ u.
	ADC1_JDR2 @ u.
	ADC1_JDR1 @ u.
	cr
	JEOC ADC1_SR bic!		\ clear interrupt source
;

: adc1-irq-enable
	JEOCIE ADC1_CR1 bis!	\ Interrupt enable for injected channels
	['] adc1-irq-handler irq-adc !	\ Hook for handler
	NVIC_EN0_INT18 NVIC_EN0_R ! 	\ Enable ADC Interrupt in global Interrupt Controller
;	


\ initalize hw and starts adc with intterupts -> external triggered with PA15 input
: adc1-start ( -- )
	ports-init
	adc1-init
	JEXTEN ADC1_CR2 bis!								\ external trigger on rising edge
	JEXTSEL ADC1_CR2 bis!								\ select line 15 for external trigger
	adc1-irq-enable
;

\ polling test -> adc external triggered with PA15 input
: adc1-start-pollext
	ports-init
	adc1-init
	JEXTEN ADC1_CR2 bis!								\ external trigger on rising edge
	JEXTSEL ADC1_CR2 bis!								\ select line 15 for external trigger
	begin 
		JEOC ADC1_SR bit@  if
			adc1-irq-handler
		then
	key? until
;

\ polling test -> adc sw triggered
: adc1-start-poll
	ports-init
	adc1-init
	JSWSTART ADC1_CR2 bis! \ Start conversion
	begin 
		JEOC ADC1_SR bit@  if
			adc1-irq-handler
			JSWSTART ADC1_CR2 bis! \ Start conversion
		then
	key? until
;


