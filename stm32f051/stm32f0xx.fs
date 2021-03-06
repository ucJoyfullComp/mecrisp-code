$40023000 constant CRC  
  CRC $0 + constant CRC_DR
  CRC $4 + constant CRC_IDR
  CRC $8 + constant CRC_CR
  CRC $C + constant CRC_INIT
        
	
$48001400 constant GPIOF  
  GPIOF $0 + constant GPIOF_MODER
  GPIOF $4 + constant GPIOF_OTYPER
  GPIOF $8 + constant GPIOF_OSPEEDR
  GPIOF $C + constant GPIOF_PUPDR
  GPIOF $10 + constant GPIOF_IDR
  GPIOF $14 + constant GPIOF_ODR
  GPIOF $18 + constant GPIOF_BSRR
  GPIOF $1C + constant GPIOF_LCKR
  GPIOF $20 + constant GPIOF_AFRL
  GPIOF $24 + constant GPIOF_AFRH
  GPIOF $28 + constant GPIOF_BRR
        
	
$48000C00 constant GPIOD  
        
	
$48000800 constant GPIOC  
  GPIOC $0 + constant GPIOC_MODER
  GPIOC $4 + constant GPIOC_OTYPER
  GPIOC $8 + constant GPIOC_OSPEEDR
  GPIOC $C + constant GPIOC_PUPDR
  GPIOC $10 + constant GPIOC_IDR
  GPIOC $14 + constant GPIOC_ODR
  GPIOC $18 + constant GPIOC_BSRR
  GPIOC $1C + constant GPIOC_LCKR
  GPIOC $20 + constant GPIOC_AFRL
  GPIOC $24 + constant GPIOC_AFRH
  GPIOC $28 + constant GPIOC_BRR
        
	
$48000400 constant GPIOB  
  GPIOB $0 + constant GPIOB_MODER
  GPIOB $4 + constant GPIOB_OTYPER
  GPIOB $8 + constant GPIOB_OSPEEDR
  GPIOB $C + constant GPIOB_PUPDR
  GPIOB $10 + constant GPIOB_IDR
  GPIOB $14 + constant GPIOB_ODR
  GPIOB $18 + constant GPIOB_BSRR
  GPIOB $1C + constant GPIOB_LCKR
  GPIOB $20 + constant GPIOB_AFRL
  GPIOB $24 + constant GPIOB_AFRH
  GPIOB $28 + constant GPIOB_BRR
        
	
$48001000 constant GPIOE  
        
	
$48000000 constant GPIOA  
  GPIOA $0 + constant GPIOA_MODER
  GPIOA $4 + constant GPIOA_OTYPER
  GPIOA $8 + constant GPIOA_OSPEEDR
  GPIOA $C + constant GPIOA_PUPDR
  GPIOA $10 + constant GPIOA_IDR
  GPIOA $14 + constant GPIOA_ODR
  GPIOA $18 + constant GPIOA_BSRR
  GPIOA $1C + constant GPIOA_LCKR
  GPIOA $20 + constant GPIOA_AFRL
  GPIOA $24 + constant GPIOA_AFRH
  GPIOA $28 + constant GPIOA_BRR
        
	
$40013000 constant SPI1  
  SPI1 $0 + constant SPI1_CR1
  SPI1 $4 + constant SPI1_CR2
  SPI1 $8 + constant SPI1_SR
  SPI1 $C + constant SPI1_DR
  SPI1 $10 + constant SPI1_CRCPR
  SPI1 $14 + constant SPI1_RXCRCR
  SPI1 $18 + constant SPI1_TXCRCR
  SPI1 $1C + constant SPI1_I2SCFGR
  SPI1 $20 + constant SPI1_I2SPR
        
	
$40003800 constant SPI2  
        
	
$40007400 constant DAC  
  DAC $0 + constant DAC_CR
  DAC $4 + constant DAC_SWTRIGR
  DAC $8 + constant DAC_DHR12R1
  DAC $C + constant DAC_DHR12L1
  DAC $10 + constant DAC_DHR8R1
  DAC $2C + constant DAC_DOR1
  DAC $34 + constant DAC_SR
        
	
$40007000 constant PWR  
  PWR $0 + constant PWR_CR
  PWR $4 + constant PWR_CSR
        
	
$40005400 constant I2C1  
  I2C1 $0 + constant I2C1_CR1
  I2C1 $4 + constant I2C1_CR2
  I2C1 $8 + constant I2C1_OAR1
  I2C1 $C + constant I2C1_OAR2
  I2C1 $10 + constant I2C1_TIMINGR
  I2C1 $14 + constant I2C1_TIMEOUTR
  I2C1 $18 + constant I2C1_ISR
  I2C1 $1C + constant I2C1_ICR
  I2C1 $20 + constant I2C1_PECR
  I2C1 $24 + constant I2C1_RXDR
  I2C1 $28 + constant I2C1_TXDR
        
	
$40005800 constant I2C2  
        
	
$40003000 constant IWDG  
  IWDG $0 + constant IWDG_KR
  IWDG $4 + constant IWDG_PR
  IWDG $8 + constant IWDG_RLR
  IWDG $C + constant IWDG_SR
  IWDG $10 + constant IWDG_WINR
        
	
$40002C00 constant WWDG  
  WWDG $0 + constant WWDG_CR
  WWDG $4 + constant WWDG_CFR
  WWDG $8 + constant WWDG_SR
        
	
$40012C00 constant TIM1  
  TIM1 $0 + constant TIM1_CR1
  TIM1 $4 + constant TIM1_CR2
  TIM1 $8 + constant TIM1_SMCR
  TIM1 $C + constant TIM1_DIER
  TIM1 $10 + constant TIM1_SR
  TIM1 $14 + constant TIM1_EGR
  TIM1 $18 + constant TIM1_CCMR1_Output
  TIM1 $18 + constant TIM1_CCMR1_Input
  TIM1 $1C + constant TIM1_CCMR2_Output
  TIM1 $1C + constant TIM1_CCMR2_Input
  TIM1 $20 + constant TIM1_CCER
  TIM1 $24 + constant TIM1_CNT
  TIM1 $28 + constant TIM1_PSC
  TIM1 $2C + constant TIM1_ARR
  TIM1 $30 + constant TIM1_RCR
  TIM1 $34 + constant TIM1_CCR1
  TIM1 $38 + constant TIM1_CCR2
  TIM1 $3C + constant TIM1_CCR3
  TIM1 $40 + constant TIM1_CCR4
  TIM1 $44 + constant TIM1_BDTR
  TIM1 $48 + constant TIM1_DCR
  TIM1 $4C + constant TIM1_DMAR
        
	
$40000000 constant TIM2  
  TIM2 $0 + constant TIM2_CR1
  TIM2 $4 + constant TIM2_CR2
  TIM2 $8 + constant TIM2_SMCR
  TIM2 $C + constant TIM2_DIER
  TIM2 $10 + constant TIM2_SR
  TIM2 $14 + constant TIM2_EGR
  TIM2 $18 + constant TIM2_CCMR1_Output
  TIM2 $18 + constant TIM2_CCMR1_Input
  TIM2 $1C + constant TIM2_CCMR2_Output
  TIM2 $1C + constant TIM2_CCMR2_Input
  TIM2 $20 + constant TIM2_CCER
  TIM2 $24 + constant TIM2_CNT
  TIM2 $28 + constant TIM2_PSC
  TIM2 $2C + constant TIM2_ARR
  TIM2 $34 + constant TIM2_CCR1
  TIM2 $38 + constant TIM2_CCR2
  TIM2 $3C + constant TIM2_CCR3
  TIM2 $40 + constant TIM2_CCR4
  TIM2 $48 + constant TIM2_DCR
  TIM2 $4C + constant TIM2_DMAR
        
	
$40000400 constant TIM3  
        
	
$40002000 constant TIM14  
  TIM14 $0 + constant TIM14_CR1
  TIM14 $C + constant TIM14_DIER
  TIM14 $10 + constant TIM14_SR
  TIM14 $14 + constant TIM14_EGR
  TIM14 $18 + constant TIM14_CCMR1_Output
  TIM14 $18 + constant TIM14_CCMR1_Input
  TIM14 $20 + constant TIM14_CCER
  TIM14 $24 + constant TIM14_CNT
  TIM14 $28 + constant TIM14_PSC
  TIM14 $2C + constant TIM14_ARR
  TIM14 $34 + constant TIM14_CCR1
  TIM14 $50 + constant TIM14_OR
        
	
$40001000 constant TIM6  
  TIM6 $0 + constant TIM6_CR1
  TIM6 $4 + constant TIM6_CR2
  TIM6 $C + constant TIM6_DIER
  TIM6 $10 + constant TIM6_SR
  TIM6 $14 + constant TIM6_EGR
  TIM6 $24 + constant TIM6_CNT
  TIM6 $28 + constant TIM6_PSC
  TIM6 $2C + constant TIM6_ARR
        
	
$40010400 constant EXTI  
  EXTI $0 + constant EXTI_IMR
  EXTI $4 + constant EXTI_EMR
  EXTI $8 + constant EXTI_RTSR
  EXTI $C + constant EXTI_FTSR
  EXTI $10 + constant EXTI_SWIER
  EXTI $14 + constant EXTI_PR
        
	
$E000E100 constant NVIC  
  NVIC $0 + constant NVIC_ISER
  NVIC $80 + constant NVIC_ICER
  NVIC $100 + constant NVIC_ISPR
  NVIC $180 + constant NVIC_ICPR
  NVIC $300 + constant NVIC_IPR0
  NVIC $304 + constant NVIC_IPR1
  NVIC $308 + constant NVIC_IPR2
  NVIC $30C + constant NVIC_IPR3
  NVIC $310 + constant NVIC_IPR4
  NVIC $314 + constant NVIC_IPR5
  NVIC $318 + constant NVIC_IPR6
  NVIC $31C + constant NVIC_IPR7
        
	
$40020000 constant DMA  
  DMA $0 + constant DMA_ISR
  DMA $4 + constant DMA_IFCR
  DMA $8 + constant DMA_CCR1
  DMA $C + constant DMA_CNDTR1
  DMA $10 + constant DMA_CPAR1
  DMA $14 + constant DMA_CMAR1
  DMA $1C + constant DMA_CCR2
  DMA $20 + constant DMA_CNDTR2
  DMA $24 + constant DMA_CPAR2
  DMA $28 + constant DMA_CMAR2
  DMA $30 + constant DMA_CCR3
  DMA $34 + constant DMA_CNDTR3
  DMA $38 + constant DMA_CPAR3
  DMA $3C + constant DMA_CMAR3
  DMA $44 + constant DMA_CCR4
  DMA $48 + constant DMA_CNDTR4
  DMA $4C + constant DMA_CPAR4
  DMA $50 + constant DMA_CMAR4
  DMA $58 + constant DMA_CCR5
  DMA $5C + constant DMA_CNDTR5
  DMA $60 + constant DMA_CPAR5
  DMA $64 + constant DMA_CMAR5
  DMA $6C + constant DMA_CCR6
  DMA $70 + constant DMA_CNDTR6
  DMA $74 + constant DMA_CPAR6
  DMA $78 + constant DMA_CMAR6
  DMA $80 + constant DMA_CCR7
  DMA $84 + constant DMA_CNDTR7
  DMA $88 + constant DMA_CPAR7
  DMA $8C + constant DMA_CMAR7
        
	
$40021000 constant RCC  
  RCC $0 + constant RCC_CR
  RCC $4 + constant RCC_CFGR
  RCC $8 + constant RCC_CIR
  RCC $C + constant RCC_APB2RSTR
  RCC $10 + constant RCC_APB1RSTR
  RCC $14 + constant RCC_AHBENR
  RCC $18 + constant RCC_APB2ENR
  RCC $1C + constant RCC_APB1ENR
  RCC $20 + constant RCC_BDCR
  RCC $24 + constant RCC_CSR
  RCC $28 + constant RCC_AHBRSTR
  RCC $2C + constant RCC_CFGR2
  RCC $30 + constant RCC_CFGR3
  RCC $34 + constant RCC_CR2
        
	
$40010000 constant SYSCFG  
  SYSCFG $0 + constant SYSCFG_CFGR1
  SYSCFG $8 + constant SYSCFG_EXTICR1
  SYSCFG $C + constant SYSCFG_EXTICR2
  SYSCFG $10 + constant SYSCFG_EXTICR3
  SYSCFG $14 + constant SYSCFG_EXTICR4
  SYSCFG $18 + constant SYSCFG_CFGR2
        
	
$40012400 constant ADC  
  ADC $0 + constant ADC_ISR
  ADC $4 + constant ADC_IER
  ADC $8 + constant ADC_CR
  ADC $C + constant ADC_CFGR1
  ADC $10 + constant ADC_CFGR2
  ADC $14 + constant ADC_SMPR
  ADC $20 + constant ADC_TR
  ADC $28 + constant ADC_CHSELR
  ADC $40 + constant ADC_DR
  ADC $308 + constant ADC_CCR
        
	
$40013800 constant USART1  
  USART1 $0 + constant USART1_CR1
  USART1 $4 + constant USART1_CR2
  USART1 $8 + constant USART1_CR3
  USART1 $C + constant USART1_BRR
  USART1 $10 + constant USART1_GTPR
  USART1 $14 + constant USART1_RTOR
  USART1 $18 + constant USART1_RQR
  USART1 $1C + constant USART1_ISR
  USART1 $20 + constant USART1_ICR
  USART1 $24 + constant USART1_RDR
  USART1 $28 + constant USART1_TDR
        
	
$40004400 constant USART2  
  USART2 $0 + constant USART2_CR1
  USART2 $4 + constant USART2_CR2
  USART2 $8 + constant USART2_CR3
  USART2 $C + constant USART2_BRR
  USART2 $10 + constant USART2_GTPR
  USART2 $14 + constant USART2_RTOR
  USART2 $18 + constant USART2_RQR
  USART2 $1C + constant USART2_ISR
  USART2 $20 + constant USART2_ICR
  USART2 $24 + constant USART2_RDR
  USART2 $28 + constant USART2_TDR
        
	
$4001001C constant COMP  
  COMP $0 + constant COMP_CSR
        
	
$40002800 constant RTC  
  RTC $0 + constant RTC_TR
  RTC $4 + constant RTC_DR
  RTC $8 + constant RTC_CR
  RTC $C + constant RTC_ISR
  RTC $10 + constant RTC_PRER
  RTC $1C + constant RTC_ALRMAR
  RTC $24 + constant RTC_WPR
  RTC $28 + constant RTC_SSR
  RTC $2C + constant RTC_SHIFTR
  RTC $30 + constant RTC_TSTR
  RTC $34 + constant RTC_TSDR
  RTC $38 + constant RTC_TSSSR
  RTC $3C + constant RTC_CALR
  RTC $40 + constant RTC_TAFCR
  RTC $44 + constant RTC_ALRMASSR
  RTC $50 + constant RTC_BKP0R
  RTC $54 + constant RTC_BKP1R
  RTC $58 + constant RTC_BKP2R
  RTC $5C + constant RTC_BKP3R
  RTC $60 + constant RTC_BKP4R
   

$40014000 constant TIM15  
  TIM15 $0 + constant TIM15_CR1
  TIM15 $4 + constant TIM15_CR2
  TIM15 $8 + constant TIM15_SMCR
  TIM15 $C + constant TIM15_DIER
  TIM15 $10 + constant TIM15_SR
  TIM15 $14 + constant TIM15_EGR
  TIM15 $18 + constant TIM15_CCMR1_Output
  TIM15 $18 + constant TIM15_CCMR1_Input
  TIM15 $20 + constant TIM15_CCER
  TIM15 $24 + constant TIM15_CNT
  TIM15 $28 + constant TIM15_PSC
  TIM15 $2C + constant TIM15_ARR
  TIM15 $30 + constant TIM15_RCR
  TIM15 $34 + constant TIM15_CCR1
  TIM15 $38 + constant TIM15_CCR2
  TIM15 $44 + constant TIM15_BDTR
  TIM15 $48 + constant TIM15_DCR
  TIM15 $4C + constant TIM15_DMAR
        
	
$40014400 constant TIM16  
  TIM16 $0 + constant TIM16_CR1
  TIM16 $4 + constant TIM16_CR2
  TIM16 $C + constant TIM16_DIER
  TIM16 $10 + constant TIM16_SR
  TIM16 $14 + constant TIM16_EGR
  TIM16 $18 + constant TIM16_CCMR1_Output
  TIM16 $18 + constant TIM16_CCMR1_Input
  TIM16 $20 + constant TIM16_CCER
  TIM16 $24 + constant TIM16_CNT
  TIM16 $28 + constant TIM16_PSC
  TIM16 $2C + constant TIM16_ARR
  TIM16 $30 + constant TIM16_RCR
  TIM16 $34 + constant TIM16_CCR1
  TIM16 $44 + constant TIM16_BDTR
  TIM16 $48 + constant TIM16_DCR
  TIM16 $4C + constant TIM16_DMAR
        
	
$40014800 constant TIM17  
        
	
$40024000 constant TSC  
  TSC $0 + constant TSC_CR
  TSC $4 + constant TSC_IER
  TSC $8 + constant TSC_ICR
  TSC $C + constant TSC_ISR
  TSC $10 + constant TSC_IOHCR
  TSC $18 + constant TSC_IOASCR
  TSC $20 + constant TSC_IOSCR
  TSC $28 + constant TSC_IOCCR
  TSC $30 + constant TSC_IOGCSR
  TSC $34 + constant TSC_IOG1CR
  TSC $38 + constant TSC_IOG2CR
  TSC $3C + constant TSC_IOG3CR
  TSC $40 + constant TSC_IOG4CR
  TSC $44 + constant TSC_IOG5CR
  TSC $48 + constant TSC_IOG6CR
        
	
$40007800 constant CEC  
  CEC $0 + constant CEC_CR
  CEC $4 + constant CEC_CFGR
  CEC $8 + constant CEC_TXDR
  CEC $C + constant CEC_RXDR
  CEC $10 + constant CEC_ISR
  CEC $14 + constant CEC_IER
        
	
$40022000 constant Flash  
  Flash $0 + constant Flash_ACR
  Flash $4 + constant Flash_KEYR
  Flash $8 + constant Flash_OPTKEYR
  Flash $C + constant Flash_SR
  Flash $10 + constant Flash_CR
  Flash $14 + constant Flash_AR
  Flash $1C + constant Flash_OBR
  Flash $20 + constant Flash_WRPR
        
	
$40015800 constant DBGMCU  
  DBGMCU $0 + constant DBGMCU_IDCODE
  DBGMCU $4 + constant DBGMCU_CR
  DBGMCU $8 + constant DBGMCU_APBLFZ
  DBGMCU $C + constant DBGMCU_APBHFZ
        
	
$E000ED00 constant SCB
  SCB $0 + constant SCB_CPUID
  SCB $4 + constant SCB_ICSR
  SCB $C + constant SCB_AIRCR
  SCB $10 + constant SCB_SCR
  SCB $14 + constant SCB_CCR
  SCB $1C + constant SCB_SHPR2
  SCB $20 + constant SCB_SHPR3

$E000E010 constant STK
  STK $0 + constant STK_CSR
  STK $4 + constant STK_RVR
  STK $8 + constant STK_CVR
  STK $C + constant STK_CALIB

