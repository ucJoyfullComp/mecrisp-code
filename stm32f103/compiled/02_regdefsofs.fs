\ compiletoflash

$00 constant TIM_CR1
$04 constant TIM_CR2
$08 constant TIM_SMCR
$0C constant TIM_DIER
$10 constant TIM_SR
$14 constant TIM_EGR
$18 constant TIM_CCMR1
$1C constant TIM_CCMR2
$20 constant TIM_CCER
$24 constant TIM_CNT
$28 constant TIM_PSC
$2C constant TIM_ARR
$30 constant TIM_RCR
$34 constant TIM_CCR1
$38 constant TIM_CCR2
$3C constant TIM_CCR3
$40 constant TIM_CCR4
$44 constant TIM_BDTR
$48 constant TIM_DCR
$4C constant TIM_DMAR

$00 constant RTC_CRH
$04 constant RTC_CRL
$08 constant RTC_PRLH
$0C constant RTC_PRLL
$10 constant RTC_DIVH
$14 constant RTC_DIVL
$18 constant RTC_CNTH
$1C constant RTC_CNTL
$20 constant RTC_ALRH
$24 constant RTC_ALRL

$00 constant WWDG_CR
$04 constant WWDG_CFR
$08 constant WWDG_SR

$00 constant IWDG_KR
$04 constant IWDG_PR
$08 constant IWDG_RLR
$0C constant IWDG_SR

$00 constant SPI_CR1
$04 constant SPI_CR2
$08 constant SPI_SR
$0C constant SPI_DR
$10 constant SPI_CRCPR
$14 constant SPI_RXCRCR
$18 constant SPI_TXCRCR
$1C constant SPI_I2SCFGR
$20 constant SPI_I2SPR

$00 constant USART_SR
$04 constant USART_DR
$08 constant USART_BRR
$0C constant USART_CR1
$10 constant USART_CR2
$14 constant USART_CR3
$18 constant USART_GTPR

$00 constant I2C_CR1
$04 constant I2C_CR2
$08 constant I2C_OAR1
$0C constant I2C_OAR2
$10 constant I2C_DR
$14 constant I2C_SR1
$18 constant I2C_SR2
$1C constant I2C_CCR
$20 constant I2C_TRISE

$00 constant USB_EP0R
$04 constant USB_EP1R
$08 constant USB_EP2R
$0C constant USB_EP3R
$10 constant USB_EP4R
$14 constant USB_EP5R
$18 constant USB_EP6R
$1C constant USB_SP7R
$40 constant USB_CNTR
$44 constant USB_ISTR
$48 constant USB_FNR
$4C constant USB_DADDR
$50 constant USB_BTABLE

$00  constant bxCAN_MCR
$04  constant bxCAN_MSR
$08  constant bxCAN_TSR
$0C  constant bxCAN_RF0R
$10  constant bxCAN_RF1R
$14  constant bxCAN_IER
$18  constant bxCAN_ESR
$1C  constant bxCAN_BTR
$180 constant bxCAN_TI0R
$184 constant bxCAN_TDT0R
$188 constant bxCAN_TDL0R
$18C constant bxCAN_TDH0R
$190 constant bxCAN_TI1R
$194 constant bxCAN_TDT1R
$198 constant bxCAN_TDL1R
$19C constant bxCAN_TDH1R
$1A0 constant bxCAN_TI2R
$1A4 constant bxCAN_TDT2R
$1A8 constant bxCAN_TDL2R
$1AC constant bxCAN_TDH2R
$1B0 constant bxCAN_RI0R
$1B4 constant bxCAN_RDT0R
$1B8 constant bxCAN_RDL0R
$1BC constant bxCAN_RDH0R
$1C0 constant bxCAN_RI1R
$1C4 constant bxCAN_RDT1R
$1C8 constant bxCAN_RDL1R
$1CC constant bxCAN_RDH1R
$200 constant bxCAN_FMR
$204 constant bxCAN_FM1R
$20C constant bxCAN_FS1R
$214 constant bxCAN_FFA1R
$21C constant bxCAN_FA1R
$240 constant bxCAN_F00R1
\ $244 constant bxCAN_F00R2
\ $248 constant bxCAN_F01R1
\ $24C constant bxCAN_F01R2
\ $250 constant bxCAN_F02R1
\ $254 constant bxCAN_F02R2
\ $258 constant bxCAN_F03R1
\ $25C constant bxCAN_F03R2
\ $260 constant bxCAN_F04R1
\ $264 constant bxCAN_F04R2
\ $268 constant bxCAN_F05R1
\ $26C constant bxCAN_F05R2
\ $270 constant bxCAN_F06R1
\ $274 constant bxCAN_F06R2
\ $278 constant bxCAN_F07R1
\ $27C constant bxCAN_F07R2
\ $280 constant bxCAN_F08R1
\ $284 constant bxCAN_F08R2
\ $288 constant bxCAN_F09R1
\ $28C constant bxCAN_F09R2
\ $290 constant bxCAN_F10R1
\ $294 constant bxCAN_F10R2
\ $298 constant bxCAN_F11R1
\ $29C constant bxCAN_F11R2
\ $2A0 constant bxCAN_F12R1
\ $2A4 constant bxCAN_F12R2
\ $2A8 constant bxCAN_F13R1
\ $2AC constant bxCAN_F13R2
\ $2B0 constant bxCAN_F14R1
\ $2B4 constant bxCAN_F14R2
\ $2B8 constant bxCAN_F15R1
\ $2BC constant bxCAN_F15R2
\ $2C0 constant bxCAN_F16R1
\ $2C4 constant bxCAN_F16R2
\ $2C8 constant bxCAN_F17R1
\ $2CC constant bxCAN_F17R2
\ $2D0 constant bxCAN_F18R1
\ $2D4 constant bxCAN_F18R2
\ $2D8 constant bxCAN_F19R1
\ $2DC constant bxCAN_F19R2
\ $2E0 constant bxCAN_F20R1
\ $2E4 constant bxCAN_F20R2
\ $2E8 constant bxCAN_F21R1
\ $2EC constant bxCAN_F21R2
\ $2F0 constant bxCAN_F22R1
\ $2F4 constant bxCAN_F22R2
\ $2F8 constant bxCAN_F23R1
\ $2FC constant bxCAN_F23R2
\ $300 constant bxCAN_F24R1
\ $304 constant bxCAN_F24R2
\ $308 constant bxCAN_F25R1
\ $30C constant bxCAN_F25R2
\ $310 constant bxCAN_F26R1
\ $314 constant bxCAN_F26R2
\ $318 constant bxCAN_F27R1
\ $31C constant bxCAN_F27R2

$04 constant BKP_DR1
\ $08 constant BKP_DR2
\ $0C constant BKP_DR3
\ $10 constant BKP_DR4
\ $14 constant BKP_DR5
\ $18 constant BKP_DR6
\ $1C constant BKP_DR7
\ $20 constant BKP_DR8
\ $24 constant BKP_DR9
\ $28 constant BKP_DR10
\ $2C constant BKP_RTCCR
\ $30 constant BKP_CR
\ $34 constant BKP_CSR
\ $40 constant BKP_DR11
\ $44 constant BKP_DR12
\ $48 constant BKP_DR13
\ $4C constant BKP_DR14
\ $50 constant BKP_DR15
\ $54 constant BKP_DR16
\ $58 constant BKP_DR17
\ $5C constant BKP_DR18
\ $60 constant BKP_DR19
\ $64 constant BKP_DR20
\ $68 constant BKP_DR21
\ $6C constant BKP_DR22
\ $70 constant BKP_DR23
\ $74 constant BKP_DR24
\ $78 constant BKP_DR25
\ $7C constant BKP_DR26
\ $80 constant BKP_DR27
\ $84 constant BKP_DR28
\ $88 constant BKP_DR29
\ $8C constant BKP_DR30
\ $90 constant BKP_DR31
\ $94 constant BKP_DR32
\ $98 constant BKP_DR33
\ $9C constant BKP_DR34
\ $A0 constant BKP_DR35
\ $A4 constant BKP_DR36
\ $A8 constant BKP_DR37
\ $AC constant BKP_DR38
\ $B0 constant BKP_DR39
\ $B4 constant BKP_DR40
\ $B8 constant BKP_DR41
\ $BC constant BKP_DR42

$00 constant PWR_CR
$04 constant PWR_CSR

$00 constant AFIO_EVCR
$04 constant AFIO_MAPR
$08 constant AFIO_EXTICR1
$0C constant AFIO_EXTICR2
$10 constant AFIO_EXTICR3
$14 constant AFIO_EXTICR4
$1C constant AFIO_MAPR2

$00 constant EXTI_IMR
$04 constant EXTI_EMR
$08 constant EXTI_RTSR
$0C constant EXTI_FTSR
$10 constant EXTI_SWER
$14 constant EXTI_PR

$00 constant GPIO_CRL
$04 constant GPIO_CRH
$08 constant GPIO_IDR
$0C constant GPIO_ODR
$10 constant GPIO_BSRR
$14 constant GPIO_BRR
$18 constant GPIO_LCKR

$00 constant ADC_SR
$04 constant ADC_CR1
$08 constant ADC_CR2
$0C constant ADC_SMPR1
$10 constant ADC_SMPR2
$14 constant ADC_JOFR1
$18 constant ADC_JOFR2
$1C constant ADC_JOFR3
$20 constant ADC_JOFR4
$24 constant ADC_HTR
$28 constant ADC_LTR
$2C constant ADC_SQR1
$30 constant ADC_SQR2
$34 constant ADC_SQR3
$38 constant ADC_JSQR
$3C constant ADC_JDR1
$40 constant ADC_JDR2
$44 constant ADC_JDR3
$48 constant ADC_JDR4
$4C constant ADC_DR

$00 constant DMA_ISR
$04 constant DMA_IFCR
$08 constant DMA_CCR1
$0C constant DMA_CNDTR1
$10 constant DMA_CPAR1
$14 constant DMA_CMAR1
$1C constant DMA_CCR2
$20 constant DMA_CNDTR2
$24 constant DMA_CPAR2
$28 constant DMA_CMAR2
$30 constant DMA_CCR3
$34 constant DMA_CNDTR3
$38 constant DMA_CPAR3
$3C constant DMA_CMAR3
$44 constant DMA_CCR4
$48 constant DMA_CNDTR4
$4C constant DMA_CPAR4
$50 constant DMA_CMAR4
$58 constant DMA_CCR5
$5C constant DMA_CNDTR5
$60 constant DMA_CPAR5
$64 constant DMA_CMAR5
$6C constant DMA_CCR6
$70 constant DMA_CNDTR6
$74 constant DMA_CPAR6
$78 constant DMA_CMAR6
$80 constant DMA_CCR7
$84 constant DMA_CNDTR7
$88 constant DMA_CPAR7
$8C constant DMA_CMAR7

$00 constant RCC_CR
$04 constant RCC_CFGR
$08 constant RCC_CIR
$0C constant RCC_APB2RSTR
$10 constant RCC_APB1RSTR
$14 constant RCC_AHBENR
$18 constant RCC_APB2ENR
$1C constant RCC_APB1ENR
$20 constant RCC_BDCR
$24 constant RCC_CSR
$28 constant RCC_AHBRSTR
$2C constant RCC_CFGR2
$30 constant RCC_CFGR3
$34 constant RCC_CR2

FLASH_BASE $00 + constant FLASH_ACR
FLASH_BASE $04 + constant FLASH_KEYR
FLASH_BASE $08 + constant FLASH_OPTKEYR
FLASH_BASE $0C + constant FLASH_SR
FLASH_BASE $10 + constant FLASH_CR
FLASH_BASE $14 + constant FLASH_AR
FLASH_BASE $1C + constant FLASH_OBR
FLASH_BASE $20 + constant FLASH_WRPR

CRC_BASE $00 + constant CRC_DR
CRC_BASE $04 + constant CRC_IDR
CRC_BASE $08 + constant CRC_CR
CRC_BASE $10 + constant CRC_INIT


\ compiletoram

