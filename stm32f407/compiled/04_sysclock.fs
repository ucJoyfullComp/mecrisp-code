8000000 constant ExternalClockFreq
32768 constant External32KOsc

\  *--------------------------------------------------------
\  *  System Clock source                    | PLL (HSE)
\  *  SYSCLK(Hz)                             | 168000000
\  *  HCLK(Hz)                               | 168000000
\  *  AHB Prescaler                          | 1
\  *  APB1 Prescaler                         | 4
\  *  APB2 Prescaler                         | 2
\  *  HSE Frequency(Hz)                      | 8000000
\  *  PLL_M                                  | 8
\  *  PLL_N                                  | 336
\  *  PLL_P                                  | 2
\  *  PLL_Q                                  | 7
\  *  PLLI2S_N                               | 258
\  *  PLLI2S_R                               | 3
\  *--------------------------------------------------------
\  *  I2S input clock(Hz)                    | 86000000
\  *                                         |
\  *  To achieve the following I2S config:   |
\  *   - Master clock output (MCKO): ON      |
\  *   - Frame wide                : 16bit   |
\  *   - Audio sampling freq (KHz) : 48      |
\  *   - Error %                   : 0.0186  |
\  *   - Prescaler Odd factor (ODD): 0       |
\  *   - Linear prescaler (DIV)    : 2       |
\  *--------------------------------------------------------
\  *  Main regulator output voltage          | Scale1 mode
\  *  Flash Latency(WS)                      | 5
\  *  Prefetch Buffer                        | OFF
\  *  Instruction cache                      | ON
\  *  Data cache                             | ON
\  *--------------------------------------------------------
\  *  Require 48MHz for USB OTG FS,          | Enabled
\  *  SDIO and RNG clock                     |
\  *--------------------------------------------------------

8 variable PLL_M
336 variable PLL_N
2 variable PLL_P
7 variable PLL_Q
258 variable PLLI2S_N
3 variable PLLI2S_R

115200 variable BaudRateUART2

8000000 variable SysClockVar

: PllOn?
    rcc_cr_r @ 24 rshift 3 and 3 =
;

: HseOn?
    rcc_cr_r @ 16 rshift 3 and 3 =
;

: GetPllSrc
    rcc_pllcfgr_r @ 22 rshift 1 and
;

: GetSysClock ( -- n )
    rcc_cfgr_r @ 2 rshift 3 and
    dup 0= if
	    drop 16000000 exit
    then
    dup 1 = if
	    drop ExternalClockFreq exit
    then
    3 = if
	    -1 exit
    then
    \ SysClock is from the PLL
    rcc_pllcfgr_r @ $400000 and 0=
    if
	    16000000
    else
	    ExternalClockFreq
    then
    rcc_pllcfgr_r @ dup 6 rshift
    $1FF and swap $3F and */
    rcc_pllcfgr_r @ 16 rshift
    3 and 1+ 2* /
    dup SysClockVar !
;

: GetApb1Clock ( -- n )
    GetSysClock
    rcc_cfgr_r @ 10 rshift 7 and
    dup 4 and 0= if
	    drop
    else
	    3 and 2 swap lshift /
    then
;

: GetApb2Clock ( -- n )
    GetSysClock
    rcc_cfgr_r @ 13 rshift 7 and
    dup 4 and 0= if
	    drop
    else
	    3 and 2 swap lshift /
    then
;

: GetApb1TimClock ( -- n )
    GetSysClock
    rcc_cfgr_r @ 10 rshift 7 and
    dup 4 and 0= if
	    drop
    else
	    3 and 2 swap lshift /
	    2*
    then
;

: GetApb2TimClock ( -- n )
    GetSysClock
    rcc_cfgr_r @ 13 rshift 7 and
    dup 4 and 0= if
	    drop
    else
	    3 and 2 swap lshift /
	    2*
    then
;

: GetAhbClock ( -- n )
    GetSysClock
    rcc_cfgr_r @ 13 rshift 7 and
    dup 4 and 0= not if
	    3 and 2 swap lshift /
    else
	    drop
    then
;

: GetPllClock ( -- u )
    PllOn? if
	    HseOn? if
	      GetPllSrc 0= if
		      \ HSI is the clock source - nominal 16MHz
		      16000000
	      else
		      ExternalClockFreq
	      then
	    else
	      16000000
	    then
    	\ compute Pll clock
	    \ PLLN factor
	    rcc_pllcfgr_r @ 6 rshift $1FF and
	    \ sanity checks on PLLN
	    dup 2 < if 2drop 0 exit then
	    \ PLLM factor
	    rcc_pllcfgr_r @ $3F and
	    \ sanity check on PLLM
    	dup 2 < if drop 2drop 0 exit then
    	*/
    	\ the stack now has the Pll Clock frequency
    	\ PLLP factor
    	rcc_pllcfgr_r @ 16 rshift 3 and 1+ 2*
    	\ HClk frequency
    	/
    else
    	0
    then
;

: SetMainPll ( -- )
    \ turn on HSE, HSI, turn off PLL
    $010900F9 not rcc_cr_r @ and $00090071 or rcc_cr_r !
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
    PLL_M @
    PLL_N @ 6 lshift or
    PLL_P @ 1 rshift 1- 16 lshift or
    $400000 or
    PLL_Q @ 24 lshift or
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
    gpioa_base gpio_ospeedr + dup @ $00030000 bic $00030000 or swap !
    gpioa_base gpio_afrh + dup @ $0000000F bic swap !
    \ set the MCO1 to output the PLLClock/4
    rcc_cfgr_r dup @ $07600000 bic $06600000 or swap !
;

: SetMCO1Input ( n -- ) \ n: 0 - HSI, 1 - LSE, 2 - HSE, 3 - PLL
    rcc_cfgr_r dup
    @ $00600000 not and
    rot 3 and
    21 lshift or
    swap !
;

: SetMCO1Divider ( n -- ) \ n: 2 to 5, anything else - no division
    dup 2 < over 6 swap < or if
      drop 0
    else
      2 +
    then
    24 lshift
    rcc_cfgr_r @ $07000000 bic or
    rcc_cfgr_r !
;

: ReleaseMCO1 ( -- )
    \ set the MCO1 to output to HSI
    rcc_cfgr_r dup @ $07600000 bic swap !
    \ Reset function of pin C9 to default Input
    gpioa_base gpio_moder + dup @ $00030000 bic swap !
    gpioa_base gpio_otyper + dup @ $0100 bic swap !
    gpioa_base gpio_pupdr + dup @ $00030000 bic swap !
    gpioa_base gpio_ospeedr + dup @ $00030000 bic swap !
    gpioa_base gpio_afrh + dup @ $0000000F bic swap !
;

: TurnOnMCO2
    \ configure MCO2 function to pin C9
    gpioc_base gpio_moder + dup @ $000C0000 bic $00080000 or swap !
    gpioc_base gpio_otyper + dup @ $0200 bic swap !
    gpioc_base gpio_pupdr + dup @ $000C0000 bic swap !
    gpioc_base gpio_ospeedr + dup @ $000C0000 bic $000C0000 or swap !
    gpioc_base gpio_afrh + dup @ $000000F0 bic swap !
    \ set the MCO2 to output the SystemClock/4
    rcc_cfgr_r dup @ $F8000000 bic $30000000 or swap !
;

: SetMCO2Input ( n -- ) \ n: 0-System Clock (SYSCLK), 1-PLLI2S, 2-HSE, 3-PLL
    rcc_cfgr_r dup
    @ $C0000000 not and
    rot 3 and
    30 lshift or
    swap !
;

: SetMCO2Divider ( n -- ) \ n: 2 to 5, anything else - no division
    dup 2 < over 6 swap < or if
      drop 0
    else
      2 +
    then
    27 lshift
    rcc_cfgr_r @ $38000000 bic or
    rcc_cfgr_r !
;

: ReleaseMCO2 ( -- )
    \ set the MCO2 to output the SystemClock/5
    rcc_cfgr_r dup @ $F8000000 bic swap !
    \ Reset function of pin C9 to default Input
    gpioc_base gpio_moder + dup @ $000C0000 bic swap !
    gpioc_base gpio_otyper + dup @ $0200 bic swap !
    gpioc_base gpio_pupdr + dup @ $000C0000 bic swap !
    gpioc_base gpio_ospeedr + dup @ $000C0000 bic swap !
    gpioc_base gpio_afrh + dup @ $000000F0 bic swap !
;

: SetI2SPll ( -- )
    \ PLLI2S clock used as I2S clock source
    $00800000 not rcc_cfgr_r @ and rcc_cfgr_r !
    \ Configure PLLI2S
    PLLI2S_N @ 6 lshift PLLI2S_R @ 28 lshift or rcc_plli2scfgr_r !
    \ Enable PLLI2S
    $04000000 rcc_cr_r @ or rcc_cr_r !
    \ Wait till PLLI2S is ready
    begin $08000000 rcc_cr_r @ and 0= not until
;

: GetMaxAHBClock ( -- u )
	PWR_CR_R @ $4000 and 0= if
		144000000
	else
		168000000
	then
;

: MapRatio2AHBCfg ( u -- cfgr )
  dup 1 <= not if
  	dup 256 <= if
  		dup 128 <= if
	  		dup 64 <= if
					dup 32 <= if
						dup 16 <= if
							dup 8 <= if
								dup 4 <= if
									2 <= if
										$80
									else
										$90
									then
								else
									drop $A0
								then
							else
								drop $B0
							then
						else
							drop $C0
						then
					else
						drop $D0
					then
	  		else
	  			drop $E0
	  		then
  		else
  			drop $F0
  		then
  	else
			drop $F0
		then
	else
		drop 0
  then
;

: MapRatio2APB2Cfg ( u -- cfgr )
  dup 1 <= not if
		dup 16 <= if
			dup 8 <= if
				dup 4 <= if
					2 <= if
						$8000
					else
						$A000
					then
				else
					drop $C000
				then
			else
				drop $E000
			then
		else
			drop $E000
		then
  else
  	drop 0
  then
;

: MapRatio2APB1Cfg ( u -- cfgr )
  dup 1 <= not if
		dup 16 <= if
			dup 8 <= if
				dup 4 <= if
					2 <= if
						$1000
					else
						$1400
					then
				else
					drop $1800
				then
			else
				drop $1C00
			then
		else
			drop $1C00
		then
  else
  	drop 0
  then
;

: SetSysClockToPll ( -- )
  \ HCLK = SYSCLK/1 PCLK2 = HCLK/2 PCLK1 = HCLK/4
  $FCF0 not rcc_cfgr_r @ and
  GetPLLClock GetMaxAHBClock /mod swap
  0= not if 1+ then
  MapRatio2AHBCfg
  or
  GetPLLClock 84000000 /mod swap
  0= not if 1+ then
	MapRatio2APB2Cfg
  or
  GetPLLClock 42000000 /mod swap
  0= not if 1+ then
	MapRatio2APB1Cfg
  or
  rcc_cfgr_r !
  \ Select the main PLL as system clock source
  $3 not rcc_cfgr_r @ and $2 or rcc_cfgr_r !
  \ Wait till the main PLL is used as system clock source
  \ if test fails after 500 tests exit.
  0
  begin
  	1+ dup 500 >
  	rcc_cfgr_r @ $0000000c and $8 =
  	or
  until
  drop
;

: SetFlashWs ( nWs -- )
    \ Configure Flash prefetch, Instruction cache,
    \ Data cache and wait state
    dup -1 > if dup 7 > if drop 7 then else drop 0 then
    $00000200 $00000400 or or
    dint
    flash_acr_r !
    eint
;

: FixUSART2 ( baudrate -- )
    \ fix USART2 setup for new clock
    \ stop USART2
    $2000 usart2_base usart_cr1 +
    $000C usart2_base usart_cr1 +
    dint
    hbic!
    3 0 do i drop loop
    hbic!
    eint
    \ calculate the value of BRR register
    GetApb1Clock ( baudrate -- baudrate Apb1Clock )
    $8000 usart2_base usart_cr1 + hbit@
    if
    	\ oversample by 8
    	over 8 * /mod 2*
    	>r over 2/ + swap / r>
    	16 * +
    else
    	\ oversample by 16
    	over /mod -rot
    	2* < -1 * +
    then
    \ we are now left with the value of USART_BRR on stack
    \ store into the register.
    usart2_base usart_brr + h!
    \ re-enable the USART2
    $2000 usart2_base usart_cr1 + hbis!
    20 0 do i drop loop
    $000C usart2_base usart_cr1 + hbis!
    usart2_base usart_sr + h@ 8 and
    if
    	usart2_base usart_dr + h@ drop
    then
;

: UpdateUART2 ( -- )
    BaudRateUART2 @ dup case
    	2400 of FixUSART2 endof
    	4800 of FixUSART2 endof
    	7200 of FixUSART2 endof
    	9600 of FixUSART2 endof
    	14400 of FixUSART2 endof
    	19200 of FixUSART2 endof
    	38400 of FixUSART2 endof
    	56000 of FixUSART2 endof
    	57600 of FixUSART2 endof
    	115200 of FixUSART2 endof
    	128000 of FixUSART2 endof
    	230400 of FixUSART2 endof
    	256000 of FixUSART2 endof
    	460800 of FixUSART2 endof
    	921600 of FixUSART2 endof
    	cr . ." is not a standard BaudRate. not updating!"
    endcase
;

: SetSysClock ( -- )
    SetMainPll
    \ TurnOnMCO1
    \ 3 SetMCO1Input \ MCO1 = PLLclock/5
    \ TurnOnMCO2
    \ 0 SetMCO2Input \ MCO2 = SYSCLK/5
    \ calculate the requested CPU clock
    GetPllClock case
	  dup 150000000 > ?of 5 endof
  	dup 120000000 > ?of 4 endof
	  dup 90000000 > ?of 3 endof
	  dup 60000000 > ?of 2 endof
	  dup 30000000 > ?of 1 endof
	  dup 0 > ?of 0 endof
    endcase
    SetFlashWs
    PllOn? if
	    SetSysClockToPll
	    UpdateUART2
    else
	    cr ." pll not on. exiting"
    then
    GetSysClock SysClockVar !
;

: SetSysClockToHSI
    \ Set clock to HSI ( guess callibrated )
    \ Turn HSI on and verify working
    $1 rcc_cr_r bis!
    500 0 do
      $2 rcc_cr_r bit@ if
        leave
      then
    loop
    $2 rcc_cr_r bit@ 0= if
      ." unable to turn HSI on. Staying in current clock."
      exit
    then
    \ fixup the HSI frequency, set HSI trim to $10
    rcc_cr_r @ $F8 bic $78 or rcc_cr_r !
    \ set system clock to HSI and verify the switch.
    $3 rcc_cfgr_r bic!
    500 0 do
      $C rcc_cfgr_r bit@ 0= if
        leave
      then
    loop
    $C rcc_cfgr_r bit@ if
      exit
    then
    \ set the flash WS accordingly, set the bus clocks prescalers to 1
    \ update the USART2
    0 SetFlashWs
    rcc_cfgr_r dup @ $FCF0 bic swap !
    UpdateUART2
    GetSysClock SysClockVar !
;

: SetSysClockToHSE
    \ Set clock to HSE
    \ Turn HSE on and verify working
    $10000 rcc_cr_r bis!
    500 0 do
      $20000 rcc_cr_r bit@ if
        leave
      then
    loop
    $20000 rcc_cr_r bit@ not if
      ." unable to turn HSE on. Staying in current clock."
      exit
    then
    \ set system clock to HSE and verify the switch.
    rcc_cfgr_r dup @ $3 bic $1 or swap !
    500 0 do
      $C rcc_cfgr_r c@ and $4 = if
        leave
      then
    loop
    $C rcc_cfgr_r c@ and $4 = not if
      exit
    then
    \ set the flash WS accordingly, set the bus clocks prescalers to 1
    \ update the USART2
    0 SetFlashWs
    rcc_cfgr_r dup @ $FCF0 bic swap !
    UpdateUART2
    GetSysClock SysClockVar !
;


