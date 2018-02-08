compiletoflash 

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
	NVIC_ST_CTRL_R @ %100 0= and if 
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
  $00FFFFFF and       \ Handle possible counter roll over by converting to 24-bit subtraction
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

: allinit ( pll_m ExternalClock -- )
	pll_m !
	ExternalClockFreq !
	SetSysClock
	." Hello World My Sys Clock Frequency is " GetSysClock . cr
	dint
	delay-init
	setup-ms
	eint
;

cornerstone Rewind-to-Basis

compiletoram

: tick  ( -- ) ." Tick" cr ;

: clock ( -- ) 
  ['] tick irq-systick !
  systick-1Hz
  eint
; 

