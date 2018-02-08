\ compiletoflash 

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
	pre_h @
	$F0 and 
	rcc_cfgr_r @ $F0 bic or rcc_cfgr_r ! ;

: SetPPRE1 ( -- )
	pre_p1 @
	$700 and 
	rcc_cfgr_r @ $700 bic or rcc_cfgr_r ! ;

: SetPPRE2 ( -- )
	pre_p2 @
	$3800 and 
	rcc_cfgr_r @ $3800 bic or rcc_cfgr_r ! ;

: SetADCPRE	( -- )
	pre_adc @
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
	flash_acr dup @ 23 bic 18 or swap !
	SetSysClockToPll
	SetHPRE
	SetPPRE1
	SetPPRE2
	SetADCPRE
	0= if
		." PLL is not system clock! Exiting." cr exit
	then
	ChangeUsart1BaudRate ;

\ compiletoram 

