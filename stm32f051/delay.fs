\ Delay with Systick-Timer

: init-delay ( -- )
    \ Start free running Systick Timer without Interrupts
    GetSysClock SysClockVar !

    \ Disable SysTick during setup
    0 STK_CSR !

    \ Maximum reload value for 24 bit timer
    $00FFFFFF STK_RVR !

    \ Any write to current clears it
    0 STK_CVR !

    \ Enable SysTick with SysClock clock
    %101 STK_CSR ! \ Use %101 instead for core clock
;

: delay-ticks ( ticks -- ) \  Tick = 2/SysClock (s)
  STK_CVR @ \ Get the starting time
  swap -              \ Subtract ticks to wait

  dup 0< if  \ If difference is negative...
           $00FFFFFF and \ Convert to 24-bit subtraction to calculate value after rollover
           begin $800000 STK_CVR bit@ until \ Wait for next rollover
         then

  begin
    dup
    STK_CVR @ \ Get current time
    ( finish finish current )
    u>= \ Systick counts backwards
  until
  drop
;

: ms ( ms -- ) 
    0 
    SysClockVar @ 1000 / -rot 
    ?do 
      dup delay-ticks 
    loop 
    drop 
;

: us ( us -- ) 
    dup 999 > if
      1000 /mod ms
    then
    SysClockVar @ 1000000 / * delay-ticks 
;


