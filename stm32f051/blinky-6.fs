\ Program Name: blinky-6.fs  for Mecrisp-Stellaris by Matthias Koch
\ This program blinks a green led using the Systick Interrupt and cooperative multitasking.
\ Hardware: STM32F0 Discovery board
\ Requires: The e4thcom Serial Terminal to preload files via the #require command. 
\ E4thcom, Copyright (C) 2013-2017 Manfred Mahlow and licensed under the GP. https://wiki.forth-ev.de/doku.php/en:projects:e4thcom#e4thcom-061
\ Author:  t.porter <terry@tjporter.com.au>

#require blinky-5-register-memory-map.fs
#require multitask.fs

%01  18 lshift GPIOC_MODER bis!

task: blinkytask
: blinky& ( -- )
  blinkytask background
    begin
	key? if boot-task wake then
	GPIOC_ODR @
	1 9 lshift xor
	GPIOC_ODR !
	stop
    again
;

: tick ( -- ) blinkytask wake ;

 ' tick irq-systick !
 8000000  STK_RVR !  \ Set the Systick interval
        7 STK_CSR !  \ Enable the systick interrupt

multitask
blinky&
