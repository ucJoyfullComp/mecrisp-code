compiletoflash

: listrangehex ( a n -- )
	cr
	0 do
		dup 
		i cells 
		dup hex. 
		+ @ hex.
		cr
	loop
	drop
;

: @|+! ( n mask addr -- )
	dup >r ( n mask addr -- n mask addr R: addr )
	@ swap ( n mask addr R: addr -- n [addr] mask R: addr )
	not and ( n [addr] mask R: addr -- n masked-val R: addr )
	or r> ! ( n masked-val R: addr -- )
;

$40000000 constant TIM2_BASE
$40000400 constant TIM3_BASE
$40000800 constant TIM4_BASE
$40002800 constant RTC_BASE
$40002C00 constant WWDG_BASE
$40003000 constant IWDG_BASE
$40003800 constant SPI2_BASE
$40004400 constant USART2_BASE
$40004800 constant USART3_BASE
$40005400 constant I2C1_BASE
$40005800 constant I2C2_BASE
$40005C00 constant USB_BASE
$40006000 constant SHARED_REG_BASE
$40006400 constant bxCAN_BASE
$40006C00 constant BKP_BASE
$40007000 constant PWR_BASE
$40010000 constant AFIO_BASE
$40010400 constant EXTI_BASE
$40010800 constant GPIOA_BASE
$40010C00 constant GPIOB_BASE
$40011000 constant GPIOC_BASE
$40011400 constant GPIOD_BASE
$40011800 constant GPIOE_BASE
$40012400 constant ADC1_BASE
$40012800 constant ADC2_BASE
$40012C00 constant TIM1_BASE
$40013000 constant SPI1_BASE
$40013800 constant USART1_BASE
$40020000 constant DMA_BASE
$40021000 constant RCC_BASE
$40022000 constant FLASH_BASE
$40023000 constant CRC_BASE_BASE

$E000E010 constant SYSTICK_BASE
$E000E100 constant NVIC_BASE
$E000ED00 constant SCB_BASE
$E000ED90 constant MPU_BASE

$E0042000 constant DBGMCU_BASE
$E0042000 constant DBGMCU_IDCODE
$E0042004 constant DBGMCU_CR

compiletoram


