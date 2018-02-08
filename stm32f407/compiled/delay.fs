\ Delay with Systick-Timer

$E000E010 constant NVIC_ST_CTRL_R
$E000E014 constant NVIC_ST_RELOAD_R
$E000E018 constant NVIC_ST_CURRENT_R

: init-delay ( -- )
    \ Start free running Systick Timer without Interrupts
    GetSysClock SysClockVar !

    \ Disable SysTick during setup
    0 NVIC_ST_CTRL_R !

    \ Maximum reload value for 24 bit timer
    $00FFFFFF NVIC_ST_RELOAD_R !

    \ Any write to current clears it
    0 NVIC_ST_CURRENT_R !

    \ Enable SysTick with SysClock clock
    %101 NVIC_ST_CTRL_R ! \ Use %101 instead for core clock
;

: delay-ticks ( ticks -- ) \  Tick = 2/SysClock (s)
  NVIC_ST_CURRENT_R @ \ Get the starting time
  swap -              \ Subtract ticks to wait

  dup 0< if  \ If difference is negative...
           $00FFFFFF and \ Convert to 24-bit subtraction to calculate value after rollover
           begin $800000 NVIC_ST_CURRENT_R bit@ until \ Wait for next rollover
         then

  begin
    dup
    NVIC_ST_CURRENT_R @ \ Get current time
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

compiletoram

\ Example:

: 1s-Pulse ( -- )
    init-delay \ Start Systick Timer
    1000 ms
;

compiletoflash