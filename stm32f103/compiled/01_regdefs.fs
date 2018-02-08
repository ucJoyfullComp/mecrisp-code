\ compiletoflash

: .[]$ ( a n -- )
    cr
    0 do
	dup 
	i cells 
	+ dup hex. 
	@ hex.
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
$40023000 constant CRC_BASE

$E000E010 constant SYSTICK_BASE
SYSTICK_BASE constant STK_BASE
STK_BASE $0 + constant STK_CTRL
STK_BASE $4 + constant STK_LOAD
STK_BASE $8 + constant STK_VAL
STK_BASE $C + constant STK_CALIB

$E000E100 constant NVIC_BASE
NVIC_BASE $000 + constant NVIC_ISER0
NVIC_BASE $004 + constant NVIC_ISER1
NVIC_BASE $008 + constant NVIC_ISER2
NVIC_BASE $080 + constant NVIC_ICER0
NVIC_BASE $084 + constant NVIC_ICER1
NVIC_BASE $088 + constant NVIC_ICER2
NVIC_BASE $100 + constant NVIC_ISPR0
NVIC_BASE $104 + constant NVIC_ISPR1
NVIC_BASE $108 + constant NVIC_ISPR2
NVIC_BASE $180 + constant NVIC_ICPR0
NVIC_BASE $184 + constant NVIC_ICPR1
NVIC_BASE $188 + constant NVIC_ICPR2
NVIC_BASE $200 + constant NVIC_IABR0
NVIC_BASE $204 + constant NVIC_IABR1
NVIC_BASE $208 + constant NVIC_IABR2
NVIC_BASE $300 + constant NVIC_IPR0
NVIC_BASE $304 + constant NVIC_IPR1
NVIC_BASE $308 + constant NVIC_IPR2
NVIC_BASE $30C + constant NVIC_IPR3
NVIC_BASE $310 + constant NVIC_IPR4
NVIC_BASE $314 + constant NVIC_IPR5
NVIC_BASE $318 + constant NVIC_IPR6
NVIC_BASE $31C + constant NVIC_IPR7
NVIC_BASE $320 + constant NVIC_IPR8
NVIC_BASE $324 + constant NVIC_IPR9
NVIC_BASE $328 + constant NVIC_IPR10
NVIC_BASE $32C + constant NVIC_IPR11
NVIC_BASE $330 + constant NVIC_IPR12
NVIC_BASE $334 + constant NVIC_IPR13
NVIC_BASE $338 + constant NVIC_IPR14
NVIC_BASE $33C + constant NVIC_IPR15
NVIC_BASE $340 + constant NVIC_IPR16
NVIC_BASE $344 + constant NVIC_IPR17
NVIC_BASE $348 + constant NVIC_IPR18
NVIC_BASE $34C + constant NVIC_IPR19
NVIC_BASE $350 + constant NVIC_IPR20
NVIC_BASE $E00 + constant NVIC_STIR

$E000ED00 constant SCB_BASE
SCB_BASE $00 + constant SCB_CPUID
SCB_BASE $04 + constant SCB_ICSR
SCB_BASE $08 + constant SCB_VTOR
SCB_BASE $0C + constant SCB_AIRCR
SCB_BASE $10 + constant SCB_SCR
SCB_BASE $14 + constant SCB_CCR
SCB_BASE $18 + constant SCB_SHPR1
SCB_BASE $1C + constant SCB_SHPR2
SCB_BASE $20 + constant SCB_SHPR3
SCB_BASE $24 + constant SCB_SHCRS
SCB_BASE $28 + constant SCB_CFSR
SCB_BASE $2C + constant SCB_HFSR
SCB_BASE $34 + constant SCB_MMAR
SCB_BASE $38 + constant SCB_BFAR

$E000ED90 constant MPU_BASE
MPU_BASE $00 + constant MPU_TYPER
MPU_BASE $04 + constant MPU_CR
MPU_BASE $08 + constant MPU_RNR
MPU_BASE $0C + constant MPU_RBAR
MPU_BASE $10 + constant MPU_RASR
MPU_BASE $14 + constant MPU_RBAR_A1
MPU_BASE $18 + constant MPU_RASR_A1
MPU_BASE $1C + constant MPU_RBAR_A2
MPU_BASE $20 + constant MPU_RASR_A2
MPU_BASE $1C + constant MPU_RBAR_A3
MPU_BASE $20 + constant MPU_RASR_A3

$E0042000 constant DBGMCU_BASE
$E0042000 constant DBGMCU_IDCODE
$E0042004 constant DBGMCU_CR

\ compiletoram


