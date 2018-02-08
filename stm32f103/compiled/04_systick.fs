\ compiletoflash 

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
    STK_LOAD ! \ How many ticks between interrupts ?
    STK_CTRL dup @ %10 bic swap ! \ Enable the systick interrupt.
;

( Delay with Systick-Timer )

: systick-1Hz ( -- ) \ Tick every second with 8 MHz clock
    GetHClk 
    STK_CTRL @ %100 and 0= if 
	8 / 
    then
    systick 
;

: delay-init ( -- )
    \ Start free running Systick Timer without Interrupts
    
    \ Disable SysTick during setup
    0 STK_CTRL !
    
    \ Maximum reload value for 24 bit timer
    $00FFFFFF STK_LOAD !

    \ Any write to current clears it
    0 STK_VAL !

    ['] def-stk-isr irq-systick ! 

    \ Enable SysTick with (HClk / 8)MHz clock
    %11 STK_CTRL ! \ Use %111 instead for core clock
;

: delay-ticks ( ticks -- ) \  Tick = 1/1MHz = 1 us
    STK_VAL @ \ Get the starting time
    swap -              \ Subtract ticks to wait
    $00FFFFFF and       \ Handle possible counter roll over
                        \ by converting to 24-bit subtraction
    ( finish )

    begin
	dup
	STK_VAL @ \ Get current time
	( finish finish current )
	u>= \ Systick counts backwards
    until
    drop
;

: us ( us -- ) 
    %11 STK_CTRL bic!
    GetHClk 
    STK_CTRL @ %100 and 0= if 
	8 / 
    then 
    1000000 */
    dup 
    STK_LOAD dup @ >r ! 
    STK_VAL ! 
    0 SysTick-Count ! 
    0 SysTick-Flag ! 
    %11 STK_CTRL bis!
    begin
	SysTick-Flag @
    until
    r> STK_LOAD !
;

: setup-ms ( -- )
    GetHClk 
    STK_CTRL @ %100 and 0= if 
	8 / 
    then
    1000 /
    STK_LOAD !
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

: initSysClock ( ExternalClock pll_m -- )
    pll_m !
    ExternalClockFreq !
    SetSysClock
    dint
    delay-init
    setup-ms
    eint
    10 ms
    cr ." Hello World My Sys Clock Frequency is " GetSysClock . cr
;

: init ( -- )
    115200 BaudRate !
    8000000 9 initSysClock
;

cornerstone Rewind-to-Basis

