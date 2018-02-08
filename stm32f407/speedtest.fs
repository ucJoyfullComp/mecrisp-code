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

\ AHB3 bus
$A0000000 constant FSMC_CTRL_R_BASE

\ AHB2 bus
$50060800 constant RNG_BASE
RNG_BASE $00 + constant RNG_CR_R
RNG_BASE $04 + constant RNG_SR_R
RNG_BASE $08 + constant RNG_DR_R

$50060400 constant HASH_BASE
HASH_BASE $00 + constant HASH_CR_R
HASH_BASE $04 + constant HASH_DIN_R
HASH_BASE $08 + constant HASH_STR0_R
HASH_BASE $0C + constant HASH_HR0_R
HASH_BASE $10 + constant HASH_HR1_R
HASH_BASE $14 + constant HASH_HR2_R
HASH_BASE $18 + constant HASH_HR3_R
HASH_BASE $1C + constant HASH_HR4_R
HASH_BASE $20 + constant HASH_IMR_R
HASH_BASE $24 + constant HASH_SR_R
HASH_BASE $F8 + constant HASH_CSR0_R

$40026400 constant DMA2_BASE
$40026000 constant DMA1_BASE
$00 constant DMA_LISR_R
$04 constant DMA_HISR_R
$08 constant DMA_LIFCR_R
$0C constant DMA_HIFCR_R
$10 constant DMA_S0CR_R
$14 constant DMA_S0NDTR_R
$18 constant DMA_S0PAR_R
$1C constant DMA_S0M0AR_R
$20 constant DMA_S0M1AR_R
$24 constant DMA_S0FCR_R
$28 constant DMA_S1CR_R
$2C constant DMA_S1NDTR_R
$30 constant DMA_S1PAR_R
$34 constant DMA_S1M0AR_R
$38 constant DMA_S1M1AR_R
$3C constant DMA_S1FCR_R
$40 constant DMA_S2CR_R
$44 constant DMA_S2NDTR_R
$48 constant DMA_S2PAR_R
$4C constant DMA_S2M0AR_R
$50 constant DMA_S2M1AR_R
$54 constant DMA_S2FCR_R
$58 constant DMA_S3CR_R
$5C constant DMA_S3NDTR_R
$60 constant DMA_S3PAR_R
$64 constant DMA_S3M0AR_R
$68 constant DMA_S3M1AR_R
$6C constant DMA_S3FCR_R
$70 constant DMA_S4CR_R
$74 constant DMA_S4NDTR_R
$78 constant DMA_S4PAR_R
$7C constant DMA_S4M0AR_R
$80 constant DMA_S4M1AR_R
$84 constant DMA_S4FCR_R
$88 constant DMA_S5CR_R
$8C constant DMA_S5NDTR_R
$90 constant DMA_S5PAR_R
$94 constant DMA_S5M0AR_R
$98 constant DMA_S5M1AR_R
$9C constant DMA_S5FCR_R
$A0 constant DMA_S6CR_R
$A4 constant DMA_S6NDTR_R
$A8 constant DMA_S6PAR_R
$AC constant DMA_S6M0AR_R
$B0 constant DMA_S6M1AR_R
$B4 constant DMA_S6FCR_R
$B8 constant DMA_S7CR_R
$BC constant DMA_S7NDTR_R
$C0 constant DMA_S7PAR_R
$C4 constant DMA_S7M0AR_R
$C8 constant DMA_S7M1AR_R
$CC constant DMA_S7FCR_R

$40023C00 constant FLASH_BASE
FLASH_BASE $00 + constant FLASH_ACR_R
FLASH_BASE $04 + constant FLASH_KEYR_R
FLASH_BASE $08 + constant FLASH_OPTKEYR_R
FLASH_BASE $0C + constant FLASH_SR_R
FLASH_BASE $10 + constant FLASH_CR_R
FLASH_BASE $14 + constant FLASH_OPTCR_R

$40023800 constant RCC_BASE
RCC_BASE $00 + constant RCC_CR_R
RCC_BASE $04 + constant RCC_PLLCFGR_R
RCC_BASE $08 + constant RCC_CFGR_R
RCC_BASE $0C + constant RCC_CIR_R
RCC_BASE $10 + constant RCC_AHB1RSTR_R
RCC_BASE $14 + constant RCC_AHB2RSTR_R
RCC_BASE $18 + constant RCC_AHB3RSTR_R
RCC_BASE $20 + constant RCC_APB1RSTR_R
RCC_BASE $24 + constant RCC_APB2RSTR_R
RCC_BASE $30 + constant RCC_AHB1ENR_R
RCC_BASE $34 + constant RCC_AHB2ENR_R
RCC_BASE $38 + constant RCC_AHB3ENR_R
RCC_BASE $40 + constant RCC_APB1ENR_R
RCC_BASE $44 + constant RCC_APB2ENR_R
RCC_BASE $50 + constant RCC_AHB1LPENR_R
RCC_BASE $54 + constant RCC_AHB2LPENR_R
RCC_BASE $58 + constant RCC_AHB3LPENR_R
RCC_BASE $60 + constant RCC_APB1LPENR_R
RCC_BASE $64 + constant RCC_APB2LPENR_R
RCC_BASE $70 + constant RCC_BDCR_R
RCC_BASE $74 + constant RCC_CSR_R
RCC_BASE $80 + constant RCC_SSCGR_R
RCC_BASE $84 + constant RCC_PLLI2SCFGR_R

$40023000 constant CRC_BASE
CRC_BASE $00 + constant CRC_DR_R
CRC_BASE $04 + constant CRC_IDR_R
CRC_BASE $08 + constant CRC_CR_R

$40022000 constant GPIOI_BASE
$40021C00 constant GPIOH_BASE
$40021800 constant GPIOG_BASE
$40021400 constant GPIOF_BASE
$40021000 constant GPIOE_BASE
$40020C00 constant GPIOD_BASE
$40020800 constant GPIOC_BASE
$40020400 constant GPIOB_BASE
$40020000 constant GPIOA_BASE
\ offsets for GPIOA/B/C/D/E/F/G/H/I
$00 constant GPIO_MODER
$04 constant GPIO_OTYPER
$08 constant GPIO_OSPEEDER
$0C constant GPIO_PUPDR
$10 constant GPIO_IDR
$14 constant GPIO_ODR
$18 constant GPIO_BSRR
$1C constant GPIO_LCKR
$20 constant GPIO_AFRL
$24 constant GPIO_AFRH

$40013C00 constant EXTI_BASE
EXTI_BASE $00 + constant EXTI_IMR_R
EXTI_BASE $04 + constant EXTI_EMR_R
EXTI_BASE $08 + constant EXTI_RTSR_R
EXTI_BASE $0C + constant EXTI_FTSR_R
EXTI_BASE $10 + constant EXTI_SWIER_R
EXTI_BASE $14 + constant EXTI_PR_R

$40013800 constant SYSCFG_BASE
SYSCFG_BASE $00 + constant SYSCFG_MEMRM_R
SYSCFG_BASE $04 + constant SYSCFG_PMC_R
SYSCFG_BASE $08 + constant SYSCFG_EXTICR1_R
SYSCFG_BASE $0C + constant SYSCFG_EXTICR2_R
SYSCFG_BASE $10 + constant SYSCFG_EXTICR3_R
SYSCFG_BASE $14 + constant SYSCFG_EXTICR4_R
SYSCFG_BASE $20 + constant SYSCFG_CMPCR_R

$40011400 constant USART6_BASE
$40011000 constant USART1_BASE
\ offsets for USART1/2/3/4/5/6
$00 constant USART_SR
$04 constant USART_DR
$08 constant USART_BRR
$0C constant USART_CR1
$10 constant USART_CR2
$14 constant USART_CR3
$18 constant USART_GTPR
$40005000 constant UART5_BASE
$40004C00 constant UART4_BASE
$40004800 constant USART3_BASE
$40004400 constant USART2_BASE

$40004000 constant I2S3ext_BASE
$40003C00 constant SPI3_I2S3_BASE
$40003800 constant SPI2_I2S2_BASE
$40003400 constant I2Sext_BASE
\ offsets for SPI2/3
$00 constant SPI_CR1_R
$04 constant SPI_CR2_R
$08 constant SPI_SR_R
$0C constant SPI_DR_R
$10 constant SPI_CRCPR_R
$14 constant SPI_RXCRCR_R
$18 constant SPI_TXCRCR_R
$1C constant SPI_I2SCFGR_R
$20 constant SPI_I2SPR_R

$40003000 constant IWDG_BASE
IWDG_BASE $00 + constant IWDG_KR_R
IWDG_BASE $04 + constant IWDG_PR_R
IWDG_BASE $08 + constant IWDG_RLR_R
IWDG_BASE $0C + constant IWDG_SR_R

$40002C00 constant WWDG_BASE
WWDG_BASE $00 + constant WWDG_CR_R
WWDG_BASE $04 + constant WWDG_CFR_R
WWDG_BASE $08 + constant WWDG_SR_R

$40002800 constant RTC_BKP_BASE
RTC_BKP_BASE $00 + constant RTC_TR_R
RTC_BKP_BASE $04 + constant RTC_DR_R
RTC_BKP_BASE $08 + constant RTC_CR_R
RTC_BKP_BASE $0C + constant RTC_ISR_R
RTC_BKP_BASE $10 + constant RTC_PRER_R
RTC_BKP_BASE $14 + constant RTC_WUTR_R
RTC_BKP_BASE $18 + constant RTC_CALIBR_R
RTC_BKP_BASE $1C + constant RTC_ALRMAR_R
RTC_BKP_BASE $20 + constant RTC_ALRMBR_R
RTC_BKP_BASE $24 + constant RTC_WPR_R
RTC_BKP_BASE $28 + constant RTC_SSR_R
RTC_BKP_BASE $2C + constant RTC_SHIFTR_R
RTC_BKP_BASE $30 + constant RTC_TSTR_R
RTC_BKP_BASE $38 + constant RTC_TSSSR_R
RTC_BKP_BASE $3C + constant RTC_CALR_R
RTC_BKP_BASE $40 + constant RTC_TAFCR_R
RTC_BKP_BASE $44 + constant RTC_ALRMASSR_R
RTC_BKP_BASE $48 + constant RTC_ALRMBSSR_R
RTC_BKP_BASE $50 + constant RTC_BK0R_R

$40007000 constant PWR_BASE
PWR_BASE $00 + constant PWR_CR_R
PWR_BASE $04 + constant PWR_CSR_R

8000000 constant ExternalClockFreq
32768 constant External32KOsc

8 variable PLL_M
336 variable PLL_N
2 variable PLL_P
7 variable PLL_Q
258 variable PLLI2S_N
3 variable PLLI2S_R

: GetSysClock ( -- n )
	rcc_cfgr_r @ 2 rshift 3 and 
	dup 0= if drop 16000000 exit then
	dup 1 = if drop ExternalClockFreq exit then
	3 = if -1 exit then
	\ SysClock is from the PLL
	rcc_pllcfgr_r @ $400000 and 0=
	if 16000000 else ExternalClockFreq then
	rcc_pllcfgr_r @ dup 6 rshift 
	$1FF and swap $3F and */
	rcc_pllcfgr_r @ 16 rshift
	3 and 1+ 2* / 
; 

: GetApb2Clock ( -- n )
	GetSysClock
	rcc_cfgr_r @ 13 rshift 7 and
	dup 4 and 0= not if 3 and 2 swap lshift / then 
; 

: GetApb1Clock ( -- n )
	GetSysClock
	rcc_cfgr_r @ 10 rshift 7 and
	dup 4 and 0= not if 3 and 2 swap lshift / then 
; 

: GetAhbClock ( -- n )
	GetSysClock
	rcc_cfgr_r @ 13 rshift 7 and
	dup 4 and 0= not if 3 and 2 swap lshift / then 
; 

: SetMainPll ( -- )
	\ turn on HSE, HSI, turn off PLL 
	$01090001 not rcc_cr_r @ and $00090001 or rcc_cr_r ! 
	\ wait for HSE to be available 
	0 
	begin 
		1+ dup $500 = not 
		$00020000 rcc_cr_r @ and 0= 
		and not 
	until 
	drop 
	\ if HSE is available run the sysclock initialization 
	rcc_cr_r @ $00020000 and 0= if exit then 
	\ Select regulator voltage output Scale 1 mode, 
	\ System frequency up to 168 MHz 
	$10000000 rcc_apb1rstr_r @ or rcc_apb1rstr_r ! 
	$EFFFFFFF rcc_apb1rstr_r @ and rcc_apb1rstr_r ! 
	$10000000 rcc_apb1enr_r @ or rcc_apb1enr_r ! 
	$4000 pwr_cr_r @ or pwr_cr_r ! 
	begin $4000 pwr_csr_r @ and 0= not until 
	\ Configure the main PLL 
	pll_m @ 
	pll_n @ 6 lshift or 
	pll_p @ 1 rshift 1- 16 lshift or 
	$400000 or 
	pll_q @ 24 lshift or 
	rcc_pllcfgr_r ! 
	\ Enable the main PLL 
	$01000000 rcc_cr_r @ or rcc_cr_r ! 
	\ Wait till the main PLL is ready 
	begin rcc_cr_r @ $02000000 and 0= not until 
; 

: TurnOnMCO1 
	\ configure MCO1 function to pin A8 
	gpioa_base gpio_moder + dup @ $00030000 bic $00020000 or swap ! 
	gpioa_base gpio_otyper + dup @ $0100 bic swap ! 
	gpioa_base gpio_pupdr + dup @ $00030000 bic swap ! 
	gpioa_base gpio_ospeeder + dup @ $00030000 bic $00030000 or swap ! 
	gpioa_base gpio_afrh + dup @ $0000000F bic swap ! 
	\ set the MCO1 to output the PLLclock/5 
	rcc_cfgr_r dup @ $07000000 bic $07000000 or swap ! 
; 

: SetMCO1Input ( n -- ) \ n: 0 - HSI, 1 - LSE, 2 - HSE, 3 - PLL 
	rcc_cfgr_r dup 
	@ $00600000 not and 
	rot 3 and 
	21 lshift or 
	swap ! 
; 

: TurnOnMCO2
	\ configure MCO2 function to pin C9 
	gpioc_base gpio_moder + dup @ $00070000 bic $00080000 or swap ! 
	gpioc_base gpio_otyper + dup @ $0200 bic swap ! 
	gpioc_base gpio_pupdr + dup @ $00070000 bic swap ! 
	gpioc_base gpio_ospeeder + dup @ $00070000 bic $00070000 or swap ! 
	gpioc_base gpio_afrh + dup @ $000000F0 bic swap ! 
	\ set the MCO2 to output the PLLclock/5 
	rcc_cfgr_r dup @ $38000000 bic $38000000 or swap ! 
; 

: SetMCO2Input ( n -- ) \ n: 0-System Clock (SYSCLK), 1-PLLI2S, 2-HSE, 3-PLL 
	rcc_cfgr_r dup 
	@ $C0000000 not and 
	rot 3 and 
	30 lshift or 
	swap ! 
; 

: SetI2SPll ( -- ) 
	\ PLLI2S clock used as I2S clock source 
	$00800000 not rcc_cfgr_r @ and rcc_cfgr_r ! 
	\ Configure PLLI2S 
	plli2s_n @ 6 lshift plli2s_r @ 28 lshift or rcc_plli2scfgr_r ! 
	\ Enable PLLI2S 
	$04000000 rcc_cr_r @ or rcc_cr_r ! 
	\ Wait till PLLI2S is ready 
	begin $08000000 rcc_cr_r @ and 0= not until 
; 

: SetSysClockToPll 
	\ HCLK = SYSCLK/1 PCLK2 = HCLK/2 PCLK1 = HCLK/4 
	$FCF0 not rcc_cfgr_r @ and $9400 or rcc_cfgr_r ! 
	\ Select the main PLL as system clock source 
	$3 not rcc_cfgr_r @ and $2 or rcc_cfgr_r ! 
	\ Wait till the main PLL is used as system clock source 
	begin rcc_cfgr_r @ $0000000c and $8 = until 
; 

: FixFlashWs 
	\ Configure Flash prefetch, Instruction cache, 
	\ Data cache and wait state 
	$00000200 $00000400 or 5 or flash_acr_r ! 
; 

\ This needs to be fixed!!! 
: FixUSART2 
	\ fix USART2 setup for new clock 
	$000C usart2_base usart_cr1 + dup @ 
	rot not and $2000 or swap ! 
	$016D usart2_base usart_brr + ! 
	$200C usart2_base usart_cr1 + dup @ rot or swap ! 
	usart2_base usart_sr + @ 8 and 
	if
		usart2_base usart_dr + @ drop 
	then 
; 

: SetSysClock ( -- ) 
	SetMainPll 
	\ TurnOnMCO1 
	\ 3 SetMCO1Input \ MCO1 = PLLclock/5 
	\ TurnOnMCO2 
	\ 0 SetMCO2Input \ MCO2 = SYSCLK/5 
	FixFlashWs 
	SetSysClockToPll 
	dint 
	FixUSART2 
	eint 
; 

: test_freq ( -- ) 
\ use port C14, C15 to toggle as past as possible to check cpu freq 
	%0101 14 2* lshift 				( -- n ) 
	$F0000000						( n -- n mask ) 
	gpioc_base gpio_moder +	@|+!	( n mask -- ) 
	%00 14 lshift					( -- n ) 
	$C000							( n -- n mask ) 
	gpioc_base gpio_otyper + @|+!	( n mask -- ) 
	%1111 14 2* lshift				( -- n ) 
	$F0000000						( n -- n mask ) 
	gpioc_base gpio_ospeeder + @|+!	( n mask -- ) 
	%0000 14 2* lshift				( -- n ) 
	$F0000000						( n -- n mask ) 
	gpioc_base gpio_pupdr + @|+!	( n mask -- ) 
	10000 0 do \ loop 10K times 
		i 2 mod 0= if 
			$0000 
		else 
			$C000 
		then 
		$C000 gpioc_base gpio_odr + @|+! ( n -- ) 
		\ 300 0 do loop 
	loop 
; 

: maketest ( -- ) 
	." Test default clock" cr 
	1000 0 do 85 emit loop 
	test_freq 
	SetSysClock 
	." Test pll clock" cr 
	test_freq 
	cr 
	1000 0 do 85 emit loop 
; 

setsysclock

