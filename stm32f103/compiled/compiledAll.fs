compiletoflash

\ Calculate Bitlog and Bitexp - close relatives to logarithm and exponent to base 2.

: bitlog ( u -- u )

 \ Invented by Tom Lehman at Invivo Research, Inc., ca. 1990
 \ Gives an integer analog of the log function
 \ For large x, B(x) = 8*(log(base 2)(x) - 1)

  dup 8 u<= if 1 lshift 
            else 
              ( u )
              30 over clz - 3 lshift 
              ( u s1 )
              swap 
              ( s1 u ) 
              28 over clz - rshift 7 and
              ( s1 s2 ) + 
            then 

  1-foldable ;


: bitexp ( u -- u )
    
  \ Returns an integer value equivalent to
  \ the exponential. For numbers > 16,
  \ bitexp(x) approx = 2^(x/8 + 1)

  \ B(E(x)) = x for 16 <= x <= 247.

  dup 247 u>  \ Overflow ?
  if drop $F0000000
  else

    dup 16 u<= if 1 rshift
               else
                 dup ( u u )
                 7 and 8 or ( u b )
                 swap ( b u )
                 3 rshift 2 - lshift
               then

  then 

  1-foldable ;


\ Emulate c, which is not available in hardware on some chips.

0 variable c,collection

: c, ( c -- )
  c,collection @ ?dup if $FF and swap 8 lshift or h,
                         0 c,collection !
                      else $100 or c,collection ! then ;

: calign ( -- )
  c,collection @ if 0 c, then ;


( Roman numerals taken from Leo Brodies "Thinking Forth" )

create romans
  (      ones ) char I c,  char V c,
  (      tens ) char X c,  char L c,
  (  hundreds ) char C c,  char D c,
  ( thousands ) char M c,  
  calign

  align   \ In this chip 16 bit writes are emulated internally, too, 
          \ as its Flash controller has 32-bits-at-once write access only.
          \ Aligning on 4 ensures that this is actually written.

0 variable column# ( current_offset )
: ones      0 column# ! ;
: tens      2 column# ! ;
: hundreds  4 column# ! ;
: thousands 6 column# ! ;

: column ( -- address-of-column ) romans column# @ + ;

: .symbol ( offset -- ) column + c@ emit ;
: oner  0 .symbol ;
: fiver 1 .symbol ;
: tener 2 .symbol ;

: oners ( #-of-oners )
  ?dup if 0 do oner loop then ;

: almost ( quotient-of-5 -- )
  oner if tener else fiver then ;

: romandigit ( digit -- )
  5 /mod over 4 = if almost drop else if fiver then oners then ;

: roman ( number -- )
  1000 /mod thousands romandigit
   100 /mod  hundreds romandigit
    10 /mod      tens romandigit
                 ones romandigit ;


\ From Gerry Jackson on comp.lang.forth

: sm/rem ( d n -- r q ) m/mod inline 3-foldable ;

: fm/mod ( d n -- r q )
   dup >r 2dup xor 0< >r sm/rem
   over r> and if 1- swap r> + swap exit then r> drop
   3-foldable
;

\ #######   RANDOM   ##########################################

\ setseed   sets the random number seed
\ random    returns a random 32-bit number
\
\ based on "Xorshift RNGs" by George Marsaglia
\ http://www.jstatsoft.org/v08/i14/paper

$7a92764b variable seed

: setseed   ( u -- )
    dup 0= or       \ map 0 to -1
    seed !
;

: random    ( -- u )
    seed @
    dup 13 lshift xor
    dup 17 rshift xor
    dup 5  lshift xor
    dup seed !
    57947 *
;

: randrange  ( u0 -- u1 ) \ u1 is a random number less than u0
    random um* nip
;

compiletoflash

\ Sine and Cosine with Cordic algorithm

: numbertable <builds does> swap 2 lshift + @ ;

hex
numbertable e^ka

C90FDAA2 ,
76B19C15 ,
3EB6EBF2 ,
1FD5BA9A ,
0FFAADDB ,
07FF556E ,
03FFEAAB ,
01FFFD55 ,

00FFFFAA ,
007FFFF5 ,
003FFFFE ,
001FFFFF ,
000FFFFF ,
0007FFFF ,
0003FFFF ,
0001FFFF ,

0000FFFF ,
00007FFF ,
00003FFF ,
00001FFF ,
00000FFF ,
000007FF ,
000003FF ,
000001FF ,

000000FF ,
0000007F ,
0000003F ,
0000001F ,
0000000F ,
00000007 ,
00000003 ,
00000001 ,

decimal

: 2rshift 0 ?do d2/ loop ;

: cordic ( f-angle -- f-error f-sine f-cosine )
         ( Angle between -Pi/2 and +Pi/2 ! )
  0 0 $9B74EDA8 0
  32 0 do
    2rot dup 0<
    if
      i e^ka 0 d+ 2rot 2rot
            2over i 2rshift 2rot 2rot
      2swap 2over i 2rshift
      d- 2rot 2rot d+
    else
      i e^ka 0 d- 2rot 2rot
            2over i 2rshift 2rot 2rot
      2swap 2over i 2rshift
      d+ 2rot 2rot 2swap d-
    then
  loop
2-foldable ;

: sine   ( f-angle -- f-sine )   cordic 2drop 2nip   2-foldable ;
: cosine ( f-angle -- f-cosine ) cordic 2nip  2nip   2-foldable ;

3,141592653589793  2constant pi
pi 2,0 f/ 2constant pi/2
pi 4,0 f/ cosine f. \ Displays cos(Pi/4)

: widecosine ( f-angle -- f-cosine )
  dabs
  pi/2 ud/mod drop 3 and ( Quadrant f-angle )

  case
    0 of                 cosine         endof
    1 of dnegate pi/2 d+ cosine dnegate endof
    2 of                 cosine dnegate endof
    3 of dnegate pi/2 d+ cosine         endof
  endcase

  2-foldable ;

: widesine ( f-angle -- f-sine )
  dup >r \ Save sign
  dabs
  pi/2 ud/mod drop 3 and ( Quadrant f-angle )

  case
    0 of                 sine          endof
    1 of dnegate pi/2 d+ sine          endof
    2 of                 sine  dnegate endof
    3 of dnegate pi/2 d+ sine  dnegate endof
  endcase

  r> 0< if dnegate then
  2-foldable ;


\ Fast integer square root. Algorithm from the book "Hacker's Delight".

: sqrt ( u -- u^1/2 )
  [
  $2040 h, \   movs r0, #0x40
  $0600 h, \   lsls r0, #24
  $2100 h, \   movs r1, #0
  $000A h, \ 1:movs r2, r1
  $4302 h, \   orrs r2, r0
  $0849 h, \   lsrs r1, #1
  $4296 h, \   cmp r6, r2
  $D301 h, \   blo 2f
  $1AB6 h, \   subs r6, r2
  $4301 h, \   orrs r1, r0
  $0880 h, \ 2:lsrs r0, #2
  $D1F6 h, \   bne 1b
  $000E h, \   movs r6, r1
  ]
  1-foldable
;

: sqr ( n -- n^2 ) dup * 1-foldable ;

\ Small examples for all targets

: invert  not  inline 1-foldable ;
: octal 8 base ! ;
: sqr ( n -- n^2 ) dup * 1-foldable ;
: star 42 emit ; 

: listrangehex ( a n -- )
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
$40023000 constant CRC_BASE_BASE

$E000E010 constant SYSTICK_BASE
$E000E100 constant NVIC_BASE
$E000EF00 constant NVIC_STIR_R
$E000ED00 constant SCB_BASE
$E000ED90 constant MPU_BASE

$E0042000 constant DBGMCU_BASE
$E0042000 constant DBGMCU_IDCODE
$E0042004 constant DBGMCU_CR

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

\ $00  constant bxCAN_MCR
\ $04  constant bxCAN_MSR
\ $08  constant bxCAN_TSR
\ $0C  constant bxCAN_RF0R
\ $10  constant bxCAN_RF1R
\ $14  constant bxCAN_IER
\ $18  constant bxCAN_ESR
\ $1C  constant bxCAN_BTR
\ $180 constant bxCAN_TI0R
\ $184 constant bxCAN_TDT0R
\ $188 constant bxCAN_TDL0R
\ $18C constant bxCAN_TDH0R
\ $190 constant bxCAN_TI1R
\ $194 constant bxCAN_TDT1R
\ $198 constant bxCAN_TDL1R
\ $19C constant bxCAN_TDH1R
\ $1A0 constant bxCAN_TI2R
\ $1A4 constant bxCAN_TDT2R
\ $1A8 constant bxCAN_TDL2R
\ $1AC constant bxCAN_TDH2R
\ $1B0 constant bxCAN_RI0R
\ $1B4 constant bxCAN_RDT0R
\ $1B8 constant bxCAN_RDL0R
\ $1BC constant bxCAN_RDH0R
\ $1C0 constant bxCAN_RI1R
\ $1C4 constant bxCAN_RDT1R
\ $1C8 constant bxCAN_RDL1R
\ $1CC constant bxCAN_RDH1R
\ $200 constant bxCAN_FMR
\ $204 constant bxCAN_FM1R
\ $20C constant bxCAN_FS1R
\ $214 constant bxCAN_FFA1R
\ $21C constant bxCAN_FA1R
\ $240 constant bxCAN_F00R1
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

$00 constant FLASH_ACR
$04 constant FLASH_KEYR
$08 constant FLASH_OPTKEYR
$0C constant FLASH_SR
$10 constant FLASH_CR
$14 constant FLASH_AR
$1C constant FLASH_OBR
$20 constant FLASH_WRPR

$00 constant CRC_DR
$04 constant CRC_IDR
$08 constant CRC_CR
$10 constant CRC_INIT

$000 constant NVIC_ISER0
$004 constant NVIC_ISER1
$008 constant NVIC_ISER2
$080 constant NVIC_ICER0
$084 constant NVIC_ICER1
$088 constant NVIC_ICER2
$100 constant NVIC_ISPR0
$104 constant NVIC_ISPR1
$108 constant NVIC_ISPR2
$180 constant NVIC_ICPR0
$184 constant NVIC_ICPR1
$188 constant NVIC_ICPR2
$200 constant NVIC_IABR0
$204 constant NVIC_IABR1
$208 constant NVIC_IABR2
$300 constant NVIC_IPR0
$304 constant NVIC_IPR1
$308 constant NVIC_IPR2
$30C constant NVIC_IPR3
$310 constant NVIC_IPR4
$314 constant NVIC_IPR5
$318 constant NVIC_IPR6
$31C constant NVIC_IPR7
$320 constant NVIC_IPR8
$324 constant NVIC_IPR9
$328 constant NVIC_IPR10
$32C constant NVIC_IPR11
$330 constant NVIC_IPR12
$334 constant NVIC_IPR13
$338 constant NVIC_IPR14
$33C constant NVIC_IPR15
$340 constant NVIC_IPR16
$344 constant NVIC_IPR17
$348 constant NVIC_IPR18
$34C constant NVIC_IPR19
$350 constant NVIC_IPR20
$E00 constant NVIC_STIR

RCC_BASE RCC_CR         + constant RCC_CR_R
RCC_BASE RCC_CFGR       + constant RCC_CFGR_R
RCC_BASE RCC_CIR        + constant RCC_CIR_R
RCC_BASE RCC_APB2RSTR   + constant RCC_APB2RSTR_R
RCC_BASE RCC_APB1RSTR   + constant RCC_APB1RSTR_R
RCC_BASE RCC_AHBENR     + constant RCC_AHBENR_R
RCC_BASE RCC_APB2ENR    + constant RCC_APB2ENR_R
RCC_BASE RCC_APB1ENR    + constant RCC_APB1ENR_R
RCC_BASE RCC_BDCR       + constant RCC_BDCR_R
RCC_BASE RCC_CSR        + constant RCC_CSR_R
RCC_BASE RCC_AHBRSTR    + constant RCC_AHBRSTR_R 
RCC_BASE RCC_CFGR2      + constant RCC_CFGR2_R 
RCC_BASE RCC_CFGR3      + constant RCC_CFGR3_R 
RCC_BASE RCC_CR2        + constant RCC_CR2_R 

$00 constant HCLK=SYSCLK/1
$80 constant HCLK=SYSCLK/2
$90 constant HCLK=SYSCLK/4
$A0 constant HCLK=SYSCLK/8
$B0 constant HCLK=SYSCLK/16
$C0 constant HCLK=SYSCLK/64
$D0 constant HCLK=SYSCLK/128
$E0 constant HCLK=SYSCLK/256
$F0 constant HCLK=SYSCLK/512

$000 constant PCLK1=HCLK/1
$400 constant PCLK1=HCLK/2
$500 constant PCLK1=HCLK/4
$600 constant PCLK1=HCLK/8
$700 constant PCLK1=HCLK/16

$0000 constant PCLK2=HCLK/1
$2000 constant PCLK2=HCLK/2
$2800 constant PCLK2=HCLK/4
$3000 constant PCLK2=HCLK/8
$3800 constant PCLK2=HCLK/16

$0000 constant ADCCLK=PCLK2/2
$4000 constant ADCCLK=PCLK2/4
$8000 constant ADCCLK=PCLK2/6
$C000 constant ADCCLK=PCLK2/8

8000000 constant NominalHSIFreq
8000000 variable ExternalClockFreq 
32768 constant ExternalLSEFreq

115200 variable BaudRate

NominalHSIFreq variable SystemCoreClock 
NominalHSIFreq variable SystemCoreClockOld 

9 variable pll_m 
HCLK=SYSCLK/1 variable pre_h
PCLK1=HCLK/2 variable pre_p1
PCLK2=HCLK/1 variable pre_p2
ADCCLK=PCLK2/6 variable pre_adc

: GetSysClockSrc ( -- n ) \ 0 - HSI, 1 - HSE, 2 - PLL, 3 - not relevant
	rcc_cfgr_r @ $C and 2 rshift ; 

: GetHSIFreq ( -- n )
\ need to improve this estimate by using the temp measurement
\ and the deviation by temp and VCC 
\ to get the approximate freq. of the internal oscillator. 
\ needs ADC driver and compensation algorithms.
	NominalHSIFreq ;

: GetPllSrcFreq ( -- Freq )
	rcc_cfgr_r @ dup $10000 and 0= if
		drop GetHSIFreq 2/
	else
		$20000 and 0= if
			0
		else
			1
		then
		ExternalClockFreq @ swap rshift
	then ; 

: GetPllFreq ( -- Freq  - 0 means pll not on )
  $2000000 rcc_cr_r bit@ not if 0 exit then
  GetPllSrcFreq 
  rcc_cfgr_r @ 18 rshift 
  dup 15 = if 1+ else 2 + then
  * ;

: GetSysClock ( -- Freq ) 
	GetSysClockSrc 
	case 
		1 of ExternalClockFreq @ endof 
		2 of GetPllSrcFreq 
			rcc_cfgr_r @ 18 rshift $f and 
			dup 15 = if 1- then 2 + * endof 
		dup of GetHSIFreq endof 
	endcase ; 

: SystemCoreClockUpdate ( -- )
	GetSysClock SystemCoreClock ! ; inline

: rescaleToClock ( n -- n )
	GetSysClock SystemCoreClockOld */ ; inline

: GetHClk ( -- Freq ) 
	GetSysClock
	rcc_cfgr_r @ 4 rshift $f and
	dup $8 and $8 = if
		dup $4 and $4 = if
			64 
		else
			2 
		then
		swap $C bic lshift /
	else
		drop
	then ; 

: GetPClk1 ( -- Freq ) 
	GetHClk 
	rcc_cfgr_r @ 8 rshift $7 and
	dup $4 and
	if 
		$3 and 2 swap lshift /
	else
	    drop
	then ; 

: getTIM2-4Clk ( -- Freq ) 
	GetPClk1 
	rcc_cfgr_r @ 8 rshift $4 and 0= not 
	if 
		2 * 
	then ; 

: GetPClk2 ( -- Freq ) 
	GetHClk 
	rcc_cfgr_r @ 11 rshift $7 and
	dup $4 and
	if 
		$3 and 2 swap lshift /
	else
	    drop
	then ; 

: GetADCClk ( -- Freq ) 
	GetPClk2 
	rcc_cfgr_r @ 14 rshift $3 and 
	1+ 2 * / ; 

: GetUSBClk ( -- Freq ) 
	rcc_cr_r @ $2000000 and 0= if 
		\ PLL is off - USB clock is off 
		0 
	else 
		GetPllSrcFreq 
		rcc_cfgr_r @ 18 rshift $f and 
		dup 15 = if 1- then 2 + * 
		rcc_cfgr_r @ $400000 and 0= if 
			2 3 */ 
		then 
	then ; 

: GetI2SClockFreq 
	GetSysClock ; inline

: GetSysTickTimerClk ( -- Freq ) 
	GetHClk $E000E010 @ 4 and 0= if 8 / then ;

: SetHPRE ( -- )
	pre_h 
	$F0 and 
	rcc_cfgr_r @ $F0 bic or rcc_cfgr_r ! ;

: SetPPRE1 ( -- )
	pre_p1
	$700 and 
	rcc_cfgr_r @ $700 bic or rcc_cfgr_r ! ;

: SetPPRE2 ( -- )
	pre_p2
	$3800 and 
	rcc_cfgr_r @ $3800 bic or rcc_cfgr_r ! ;

: SetADCPRE	( -- )
	pre_adc
	$C000 and
	rcc_cfgr_r @ $C000 bic or rcc_cfgr_r ! ;

: SetSysClockInt
	$1 rcc_cr_r bis! 
	$500 
	begin 
		1- dup 0= 
		rcc_cr_r @ $2 and $2 = or 
	until 
	0= if 
		0 
		." HSI clock is off" cr
		exit 
	then 
	\ HSI is on
	0 rcc_cfgr_r ! 
	50 0 do loop 
	\ switch SysClock to HSI ( 8000000Hz )
	$2000 usart1_base usart_cr1 + bic! 
	GetPClk2 
	BaudRate @ 16 * /mod 16 * swap BaudRate @ / or
	usart1_base usart_brr + h! 
	$2000 usart1_base usart_cr1 + bis! 
	-1 ;

: TurnPllClockOn
	$1000000 rcc_cr_r bic!
	$500 
	begin 
		1- dup 0= 
		rcc_cr_r @ $2000000 and 0= or 
	until 
	0= if 0 exit then 
	$10000 rcc_cr_r bis! 
	$500 
	begin 
		1- dup 0= 
		rcc_cr_r @ $20000 and $20000 = or 
	until 
	0= if 0 exit then 
	rcc_cfgr_r @ $3F0000 bic 
	pll_m @ 2 - 18 lshift or 
	$10000 or
	rcc_cfgr_r ! 
	$1000000 rcc_cr_r bis! 
	$5000 begin 
		1- dup 0= 
		rcc_cr_r @ $02020000 and $02020000 = or 
	until 
	0= if 
		." PLL is not on. HSI is sysclock" 
		cr 10 emit 0 exit 
	then 
	-1 ;

: SetSysClockToPll
	rcc_apb2enr_r @
	rcc_apb1enr_r @
	0 rcc_apb2enr_r !
	0 rcc_apb1enr_r !
	rcc_cfgr_r @ $3 bic 2 or rcc_cfgr_r ! 
	$5000 begin 
		1- dup 0= 
		rcc_cfgr_r @ $C and $8 = or 
	until 
	0= if 
	  ." PLL is not sysclock" cr 
	  rcc_apb1enr_r !
	  rcc_apb2enr_r !
	  0 
	  exit 
	then
	rcc_apb1enr_r !
	rcc_apb2enr_r !
	-1 ;

: ChangeUsart1BaudRate
	$2000 usart1_base usart_cr1 + bic!
	GetPClk2 BaudRate @ /
	usart1_base usart_brr + h! 
	$2000 usart1_base usart_cr1 + bis! ;

: SetSysClock 
	SetSysClockInt not if
		." unable to switch to internal clock (HSI) aborting." cr
		exit
	then
	TurnPllClockOn not if
		." unable to turn PLL on! aborting." cr
		exit
	else
		." PLL frequency: " GetPllFreq . cr
	then
	flash_base flash_acr + dup @ 23 bic 18 or swap !
	SetSysClockToPll
	0= if
		." PLL is not system clock! Exiting." cr exit
	then
	ChangeUsart1BaudRate ;

( Systick-Interrupt )

0 variable SysTick-Count
-1 variable SysTick-Flag

: def-stk-isr 
	SysTick-Count @ 0 > if
		-1 SysTick-Count +!
	else
	    0 not SysTick-Flag !
	then
;

: systick ( ticks -- )
    $E000E014 ! \ How many ticks between interrupts ?
  $E000E010 dup @ %10 bic swap ! \ Enable the systick interrupt.
;

( Delay with Systick-Timer )

$E000E010 constant NVIC_ST_CTRL_R
$E000E014 constant NVIC_ST_RELOAD_R      
$E000E018 constant NVIC_ST_CURRENT_R

: systick-1Hz ( -- ) \ Tick every second with 8 MHz clock
	GetHClk 
	NVIC_ST_CTRL_R @ %100 and 0= if 
		8 / 
	then
	systick 
;

: delay-init ( -- )
    \ Start free running Systick Timer without Interrupts
  
    \ Disable SysTick during setup
    0 NVIC_ST_CTRL_R !

    \ Maximum reload value for 24 bit timer
    $00FFFFFF NVIC_ST_RELOAD_R !

    \ Any write to current clears it
    0 NVIC_ST_CURRENT_R !

    ['] def-stk-isr irq-systick ! 

    \ Enable SysTick with (HClk / 8)MHz clock
    %11 NVIC_ST_CTRL_R ! \ Use %111 instead for core clock
;

: delay-ticks ( ticks -- ) \  Tick = 1/1MHz = 1 us
  NVIC_ST_CURRENT_R @ \ Get the starting time
  swap -              \ Subtract ticks to wait
  $00FFFFFF and       \ Handle possible counter roll over
  	    	      \ by converting to 24-bit subtraction
  ( finish )

  begin
    dup
    NVIC_ST_CURRENT_R @ \ Get current time
    ( finish finish current )
    u>= \ Systick counts backwards
  until
  drop
;

: us ( us -- ) 
    NVIC_ST_RELOAD_R @
	%11 NVIC_ST_CTRL_R bic! 
    GetHClk 
	NVIC_ST_CTRL_R @ %100 and 0= if 
		8 / 
	then 
	1000000 */ 1- 
	dup 
	NVIC_ST_RELOAD_R ! 
	NVIC_ST_CURRENT_R ! 
	0 SysTick-Count ! 
	0 SysTick-Flag ! 
	%11 NVIC_ST_CTRL_R bis!
	begin
	    SysTick-Flag @
	until
    NVIC_ST_RELOAD_R !
;	
: setup-ms ( -- )
	GetHClk 
	NVIC_ST_CTRL_R @ %100 and 0= if 
		8 / 
	then
	1000 / 1-
    NVIC_ST_RELOAD_R !
; 
: ms ( ms -- ) 
    1- SysTick-Count !
    0 SysTick-Flag !
    begin
        SysTick-Flag @
    until
;
: cornerstone ( Name ) ( -- )
  <builds begin here $3FF and while 0 h, repeat
  does>   begin dup  $3FF and while 2+   repeat 
          eraseflashfrom
;

: init04 ( ExternalClock pll_m -- )
	pll_m !
	ExternalClockFreq !
	SetSysClock
	dint
	delay-init
	setup-ms
	eint
	100 0 do loop
	." Hello World My Sys Clock Frequency is " GetSysClock . cr
;

cornerstone Rewind-to-Basis

\ push the ADC Clock period in picoseconds on the stack
: ADCClk->T ( -- N[picoseconds] )
  1000000 dup getADCClk */mod 
  swap getADCClk 2* > if
    1+ 
  then 
;

\ convert a time period in picoseconds to ADC clock half period counts
: T->ADCClk/2 ( T[picoseconds] -- N[number of half period units] )
  ADCClk->T 2/ /mod 
  swap 0= not if
    1+
  then
;

\ set pin type gpioport[pin] to state
: config-gpio-pin ( gpioport pin state -- )
  $F and 
  swap 
  dup 15 > if
      2drop drop
      exit 
  then 
  dup 8 < if 
      gpio_crl 
  else 
      8 - gpio_crh 
  then
  >r rot r> + >r 
  4 * $F over 
  lshift r@ bic! 
  lshift r> bis! 
;

: setSmp ( addr smp u -- )
  3 * >r 
  $7 swap over and swap 
  r@ lshift swap
  r> lshift swap
  rot swap over
  bic! bis!
;

\ set sampling time for n channel for ADCi with adci_base address
: setInSmp ( adci_base smp n -- )
  dup case
    dup 10 < ?of 
      rot adc_smpr2 + -rot
      setSmp
    endof
    dup 18 < ?of 
      10 -
      rot adc_smpr1 + -rot
      setSmp
    endof
  endcase
;

\ enable i gpio pins as analog inputs.
: initADCIns ( n1 ... ni i -- )
  dup 1 < if exit then
  0 do 
    case
      dup dup 8 < ?of gpioa_base swap 0 config-gpio-pin endof
      dup 10 < ?of gpiob_base swap 8 - 0 config-gpio-pin endof
      dup 16 < ?of gpioc_base swap 10 - 0 config-gpio-pin endof
      drop
    endcase
  loop
;

\ return a flag of the End Of Conversion status if ADC# i
: testEOC ( adcI_base -- flag )
  2 swap adc_sr + bit@ ; inline

\ clear all the status flags of ADC# i
: clearSR ( adcI_base -- )
  $1F swap adc_sr + bic! ; inline

\ wait for End Of Conversion of ADC# i for 500 tests
: waitForEOC ( adcI_base -- )
  500 0 do
    dup testEOC if drop unloop  exit then
  loop
  drop
;

\ test EOC for ADC1 
: testEOC1 adc1_base testEOC ;

\ clear all status flags for ADC1
: clearSR1 adc1_base clearSR ;

\ wait for EOC of ADC1 for 500 tests
: waitForEOC1 adc1_base waitForEOC ;

\ test EOC for ADC2
: testEOC2 adc2_base testEOC ;

\ clear all status flags for ADC2
: clearSR2 adc2_base clearSR ;

\ wait for EOC of ADC2 for 500 tests
: waitForEOC2 adc2_base waitForEOC ;

1 constant ADON

\ set prescaler for all ADCs to maximum clock rate <14MHz
: setPrescalerToMaximum
  GetPClk2 14000000 /mod swap 
  0= not if 1+ then
  dup 0= if 1 then
  2/ 1- 14 lshift 
  $C000 rcc_cfgr_r bic! 
  rcc_cfgr_r bis!
;

\ turn ADC1 on and wait atleast 1uSec
: turnADC1-On ( -- )
  1 adc1_base adc_cr2 + bit@ not if
    1 adc1_base adc_cr2 + bis!
    getsysclock 1000000 /mod 
    swap 500000 > if 1+ then 
    0 do loop
  then
;


: initADC1 ( -- )
  setPrescalerToMaximum
  \ enable clock to ADC1 and reset it   
  $0200 rcc_apb2enr_r bis!
  100 0 do loop \ delay for device enable to take effect (not sure needed)
  \ reset ADC 1 in RCC unit
  $0200 rcc_apb2rstr_r bis!
  100 0 do loop \ wait a while for reset to take effect
  $0200 rcc_apb2rstr_r bic!
  \ init ADCi
  $00000000 adc1_base adc_cr1 + !
  $00000000 adc1_base adc_cr2 + !
  $00FFFFFF adc1_base adc_smpr1 + !
  $3FFFFFFF adc1_base adc_smpr2 + !
  $FFF adc1_base 
  2dup adc_jofr1 + hbic!
  2dup adc_jofr2 + hbic!
  2dup adc_jofr3 + hbic!
  2dup adc_jofr4 + hbic!
  2dup adc_htr + hbis!
  adc_ltr + hbic!
  $00FFFFFF adc1_base adc_sqr1 + bic!
  $3FFFFFFF adc1_base 
  2dup adc_sqr2 + bic!
  adc_sqr3 + bic!
  $1 adc1_base adc_sqr3 + bis!
  $003FFFFF adc1_base adc_jsqr + bic!
  \ calibrate ADC1
  $1F adc1_base adc_sr + bic!
  $4 adc1_base adc_cr2 + >r r@ bis!
  begin
    $4 r@ bit@
  until
  r> drop
  \ set to single conversion mode
  \ set default conversion channels and sampling time per channel
  \ set data alignment
  turnADC1-On
; 

\ do one A/D conversion on channel 1 and place result on stack
: convertADC1-single ( -- u )
  turnADC1-On
  1 adc1_base adc_cr2 + bis!
  waitForEOC1
  adc1_base adc_dr + h@
  $1F adc1_base adc_sr + hbic!
;  

3315000 variable Vdda

\ display the number on stack in #.###### format
: .1R6 
  1000000 /mod 
  <# [char] . hold dup s>d dabs #S rot sign #> type
  <# 0 over
  case
    dup base @ < ?of # 
      5 0 do 0 .digit hold loop endof
    dup base @ dup * < ?of #S 
      4 0 do 0 .digit hold loop endof
    dup base @ dup dup * * < ?of #S 
      3 0 do 0 .digit hold loop endof
    dup base @ dup * dup * < ?of #S 
      2 0 do 0 .digit hold loop endof
    dup base @ dup dup * dup * * < ?of #S 
      0 .digit hold endof
    dup base @ dup * dup dup * * < ?of #S endof
  endcase
  #> type space
;

\ do one A/D conversion and translate to voltage and then display in .1R6 format
: convIn1 convertADC1-single Vdda @ 4095 */ .1R6 ;

32 buffer: mynumber

\ do a manual callibration of the Vdda value and translate into Vdda variable
: calibrate-vdda
  depth >r
  ." enter the value of Vdda in uV ( 1V=1000000uV ) "
  mynumber 32 accept
  mynumber swap evaluate
  depth r> = not if Vdda ! then
;

\ translate the sequence number ( in multi channel conversion ) to the relative
\ slot in the sqr register
: Seq-to-sqr
    dup 6 < if
    else dup 12 < if 
        6 -
    else dup 16 < if 
        12 -
    then
    then
    then
;

\ translate the sequence number ( in multi channel conversion ) to the relevant
\ sqr register
: Seq-to-Reg
    dup 6 < if
      drop
      adc_sqr3
    else
    dup 12 < if 
      drop
      adc_sqr2
    else
    dup 16 < if 
      drop
      adc_sqr1
    then
    then
    then
;

\ set the analog channel (channel: 0..17) to convert in a multiconversion 
\ sequence to sequence# n (0..15) in ADCi (adci_base address)
: setInCh ( adci_base channel n -- )
  dup 0 < if 
    2drop drop
  else
    dup 15 > if
      2drop drop
    else
      dup Seq-to-sqr
      5 * $1F over lshift >r 
      rot swap lshift swap
      Seq-to-Reg rot
      + r> over
      bic! bis!
    then
  then
;

\ set the number of conversions in a multi conversion sequence in ADC1
: setNumOfChans ( n -- )
  dup 0 > over 17 < and if
    1- 20 lshift 
    adc1_base adc_sqr1 + 
    dup $f 20 lshift swap bic!
    bis!
  else
    drop
  then
;

\ set the number of channel to be converted first
: setInCh1 ( adci_base channel -- )
  dup 0 swap > if exit then
  dup 17 > if exit then
  swap adc_sqr3 + dup @ $1F bic rot or swap
  !
;

\ do a calibration in ADC1
: calibrate-adc1
    $800000 adc1_base adc_cr2 + bis!
    1 ms
    adc1_base 4 16 setInSmp
    adc1_base 4 17 setInSmp
    17 adc1_base adc_sqr3 + $1F over bic! bis!
    1204000 4095
    convertADC1-single
    */ dup Vdda ! 
    ." Computed Vdda: " . cr
;

\ initialize the system for USART1: 115200,8N1
\ system clock 72MHz
\ initialize ADC1: 1 channel conversion, with conversion time of 1.666uSec
: init
  115200 BaudRate !
  8000000 9 init04
  initADC1
  calibrate-adc1
  1 1 initADCIns
  adc1_base 1 0 setInCh
  adc1_base 1 1 setInSmp
  1 setNumOfChans
;

compiletoram

init

