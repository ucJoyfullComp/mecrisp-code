8000000 constant ExternalClockFreq
32768 constant External32KOsc

\ ------------  Uzi Cohen  --------------------
\ UNDER NO CIRCUMSTANCES TURN OFF THE HSI !!!
\ ------------  Uzi Cohen  --------------------

\  *--------------------------------------------------------
\  *  System Clock source                    | PLL (HSE)
\  *  SYSCLK(Hz)                             | 48000000
\  *  HCLK(Hz)                               | 48000000
\  *  AHB Prescaler                          | 1
\  *  APB1 Prescaler                         | 1
\  *  APB2 Prescaler                         | 1
\  *  HSE Frequency(Hz)                      | 8000000
\  *  PLL_M                                  | 1
\  *  PLL_N                                  | 6
\  *--------------------------------------------------------
\  *  I2S input clock(Hz)                    | 48000000
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
\  *  Flash Latency(WS)                      | 1
\  *  Prefetch Buffer                        | ON
\  *--------------------------------------------------------

1 variable PLL_M
6 variable PLL_N

115200 variable BaudRateUART1

8000000 variable SysClockVar

: PllOn?
    rcc_cr @ 24 rshift 3 and 3 =
;

: HseOn?
    rcc_cr @ 16 rshift 3 and 3 =
;

: GetPllSrc
    rcc_cfgr @ 15 rshift 3 and
;

: GetSysClock ( -- n )
    rcc_cfgr @ 2 rshift 3 and 
    dup 0= if
	    drop 8000000 exit
    then
    dup 1 = if
	    drop ExternalClockFreq exit
    then
    3 = if
	     48000000 exit
    then
    \ SysClock is from the PLL
    rcc_cfgr @ 15 rshift 3 and
    case
      \ HSI/2
      0 of 
          4000000 SysClockVar ! 
        endof
      \ HSI/PllPrediv
      1 of 
          8000000 rcc_cfgr2 h@ $000F and 1+ / SysClockVar ! 
        endof
      \ HSE/PllPrediv
      2 of 
          ExternalClockFreq rcc_cfgr2 h@ $000F and 1+ / SysClockVar ! 
        endof
      \ HSI48/PllPrediv
      3 of 
          48000000 rcc_cfgr2 h@ $000F and 1+ / SysClockVar ! 
        endof
    endcase
    rcc_cfgr @ 18 rshift 15 and
    dup 15 < if 2 + else 1+ then 
    SysClockVar @ * SysClockVar ! 
    SysClockVar @
;

: GetApbClock ( -- n )
    GetSysClock
    rcc_cfgr @ 8 rshift 7 and
    dup 4 and 0= not if
	    3 and 2 swap lshift /
    else
	    drop
    then
;

: GetAhbClock ( -- n )
    GetSysClock
    rcc_cfgr @ 4 rshift 15 and
    dup 8 and 0= not if
      7 and
      dup 4 < if
  	    3 and 2 swap lshift /
      else
        3 and 64 swap lshift /
      then
    else
	    drop
    then
;

: GetPllSrcFreq
    GetPllSrc
    case
      \ HSI/2
      0 of 4000000 endof
      \ HSI/PllPrediv
      1 of 8000000 rcc_cfgr2 h@ $000F and 1+ / endof
      \ HSE/PllPrediv
      2 of ExternalClockFreq rcc_cfgr2 h@ $000F and 1+ / endof
      \ HSI48/PllPrediv
      3 of 48000000 rcc_cfgr2 h@ $000F and 1+ / endof
    endcase
;

: GetPllClock
    PllOn? if
	    HseOn? if
        GetPllSrcFreq
	    else
	      8000000
	    then
    	\ compute Pll clock
      \ PLL multiplicatin factor
      rcc_cfgr @ 18 rshift 15 and
      dup 15 < if 2 + else 1+ then *
    	\ the stack now has the Pll Clock frequency
      rcc_cfgr @ 4 rshift 15 and
      dup 8 and 0= not if
        7 and
        dup 4 < if
    	    3 and 2 swap lshift /
        else
          3 and 64 swap lshift /
        then
      else
	      drop
      then
    	\ HClk frequency
    else
    	0
    then
;

: SetMainPll ( -- )
    \ turn on HSE, HSI - and compensate to middle of comp scale,
    \ turn off PLL 
    $010900F8 not rcc_cr @ and $00090080 or rcc_cr ! 
    \ wait for HSE to be available 
    0 
    begin 
    	1+ dup $500 = not 
    	$00020000 rcc_cr @ and 0= 
    	and not 
    until 
    drop 
    \ if HSE is available run the sysclock initialization 
    rcc_cr @ $00020000 and 0= if exit then 
    \ enable CSS - if HSE stops the system automatically switches to HSI
    $10000000 rcc_apb1rstr @ or rcc_apb1rstr ! 
    $EFFFFFFF rcc_apb1rstr @ and rcc_apb1rstr ! 
    $10000000 rcc_apb1enr @ or rcc_apb1enr ! 
    $010C pwr_cr ! 
    \ Configure the main PLL 
    rcc_cfgr2 @ 15 bic PLL_M @ 1- 15 and or rcc_cfgr2 !
    rcc_cfgr @ $003D8000 bic 
    PLL_N @ 2 - 15 and 18 lshift or
    %10 15 lshift or 
    rcc_cfgr !
    \ Enable the main PLL 
    $01000000 rcc_cr @ or rcc_cr ! 
    \ Wait till the main PLL is ready 
    begin rcc_cr @ $02000000 and 0= not until
;

: TurnOnMCO
    \ configure MCO function to pin A8 
    gpioa_moder + dup @ $00030000 bic $00020000 or swap ! 
    gpioa_otyper + dup @ $0100 bic swap ! 
    gpioa_ospeedr + dup @ $00030000 bic $00030000 or swap ! 
    gpioa_pupdr + dup @ $00030000 bic swap ! 
    gpioa_afrh + dup @ $0000000F bic swap ! 
    \ set the MCO to output the SysClock/4
    rcc_cfgr dup @ 
    $7F000000 bic 
    $20000000 or
    $04000000 or
    swap !
;

: SetMCOInput ( n -- ) \ n: 0 - disable, 1 - HSI14, 2 - LSI, 3 - LSE
  \ n: 4 - SysClock, 5 - HSI, 6 - HSE, 7 - PLL, 8 - HSI48 
    rcc_cfgr @ $0F000000 bic 
    swap $F and 
    24 lshift or 
    rcc_cfgr !
;

: SetMCODivider ( n -- ) \ n: 0 to 7, anything else - no division
  \ the dividers are exponents of 2 - the division is by 2^n 
  \ that is 1,2,4,8,16,32,64,128
    dup 0 < over 8 swap < or if
      drop 0
    then
    28 lshift 
    rcc_cfgr @ $70000000 bic or
    rcc_cfgr !
;

: ReleaseMCO ( -- )
    \ set the MCO to output to HSI
    rcc_cfgr dup @ $7F000000 bic swap !
    \ Reset function of pin C9 to default Input
    gpioa_moder dup @ $00030000 bic swap ! 
    gpioa_otyper dup @ $0100 bic swap ! 
    gpioa_pupdr dup @ $00030000 bic swap ! 
    gpioa_ospeedr dup @ $00030000 bic swap ! 
    gpioa_afrh dup @ $0000000F bic swap ! 
;    

: SetSysClockToPll 
    \ HCLK = SYSCLK/1 PCLK = HCLK/1
    rcc_cfgr @ $7F0 bic rcc_cfgr ! 
    \ Select the main PLL as system clock source 
    rcc_cfgr @ $3 bic $2 or rcc_cfgr ! 
    \ Wait till the main PLL is used as system clock source
    \ if test fails after 500 tests exit.
    0
    begin
    	1+ dup 500 >
    	rcc_cfgr @ $0000000c and $8 =
    	or
    until
    drop
;

: SetFlashWs ( nWs -- )
    \ Configure Flash prefetch and wait state 
    7 and
    dup 0= if
      drop 0
    else
      $10 or
    then
    flash_acr @ $17 bic
    or flash_acr
    dint ! eint
;

: SetSysClock ( -- ) 
    \ set the clock source of the USART1 to HSI. we will not lose the link
    $3 rcc_cfgr3 bis!
    50 0 do i drop loop
    SetMainPll
    \ TurnOnMCO1 
    \ 3 SetMCO1Input \ MCO1 = PLLclock/5 
    \ TurnOnMCO2 
    \ 0 SetMCO2Input \ MCO2 = SYSCLK/5
    \ calculate the requested CPU clock
    GetPllClock
    case
	  dup 168000000 > ?of 7 endof
	  dup 144000000 > ?of 6 endof
	  dup 120000000 > ?of 5 endof
 	  dup 96000000 > ?of 4 endof
	  dup 72000000 > ?of 3 endof
	  dup 48000000 > ?of 2 endof
	  dup 24000000 > ?of 1 endof
	  dup 0 > ?of 0 endof
    endcase
    SetFlashWs
    PllOn? if
	    SetSysClockToPll
    else
	    cr ." pll not on. exiting"
    then
    GetSysClock SysClockVar !
;

: SetSysClockToHSI
    \ set the clock source of the USART1 to HSI. we will not lose the link
    $3 rcc_cfgr3 bis!
    50 0 do i drop loop
    \ Set clock to HSI ( guess callibrated )
    \ Turn HSI on and verify working
    $1 rcc_cr bis!
    500 0 do
      $2 rcc_cr bit@ if
        leave
      then
    loop
    $2 rcc_cr bit@ 0= if
      ." unable to turn HSI on. Staying in current clock."
      exit
    then
    \ fixup the HSI frequency, set HSI trim to $10
    rcc_cr @ $F8 bic $78 or rcc_cr !
    \ set system clock to HSI and verify the switch.
    $3 rcc_cfgr bic!
    500 0 do
      $C rcc_cfgr bit@ 0= if
        leave
      then
    loop
    $C rcc_cfgr bit@ if
      exit
    then
    \ set the flash WS accordingly, set the bus clocks prescalers to 1
    SetFlashWs
    rcc_cfgr @ $07F0 bic rcc_cfgr !
    GetSysClock SysClockVar !
;

: SetSysClockToHSE
    \ set the clock source of the USART1 to HSI. we will not lose the link
    $3 rcc_cfgr3 bis!
    50 0 do i drop loop
    \ Set clock to HSE
    \ Turn HSE on and verify working
    $10000 rcc_cr bis!
    500 0 do
      $20000 rcc_cr bit@ if
        leave
      then
    loop
    $20000 rcc_cr bit@ 0= if
      ." unable to turn HSE on. Staying in current clock."
      exit
    then
    \ set system clock to HSE and verify the switch.
    rcc_cfgr dup @ $3 bic $1 or swap !
    500 0 do
      $C rcc_cfgr c@ and $4 = if
        leave
      then
    loop
    $C rcc_cfgr c@ and $4 = not if
      exit
    then
    \ set the flash WS accordingly, set the bus clocks prescalers to 1
    SetFlashWs
    rcc_cfgr dup @ $07F0 bic swap !
    GetSysClock SysClockVar !
;


