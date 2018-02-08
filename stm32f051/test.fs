include sysclock.fs

: test01
    $3 rcc_cfgr3 bis!
    50 0 do i drop loop
    SetMainPll
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
;

: test02
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
    drop
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
;

