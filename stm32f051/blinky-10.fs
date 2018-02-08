\ Program Name: blinky-10.fs  for Mecrisp-Stellaris by Matthias Koch
\ This program blinks a green led using the Systick Interrupt and cooperative multitasking.
\ it also blinks the blue led at a different rate in another task, in the third task it
\ outputs 'emit' to the terminal
\ Hardware: STM32F0 Discovery board
\ Requires: The e4thcom Serial Terminal to preload files via the #require command.
\ E4thcom, Copyright (C) 2013-2017 Manfred Mahlow and licensed under the GP. https://wiki.forth-ev.de/doku.php/en:projects:e4thcom#e4thcom-061
\ Author:  t.porter <terry@tjporter.com.au>

#require blinky-5-register-memory-map.fs
#require multitask.fs

%11  18 lshift GPIOC_MODER bic!
%01  18 lshift GPIOC_MODER bis!
%11  16 lshift GPIOC_MODER bic!
%01  16 lshift GPIOC_MODER bis!

task: blinkygreentask
: blinkygreen& ( -- )
  blinkygreentask background
    begin
	key? if boot-task wake then
	GPIOC_ODR h@
	1 9 lshift xor
	GPIOC_ODR h!
	stop
    again
;

task: blinkybluetask
: blinkyblue& ( -- )
  blinkybluetask background
    begin
	key? if boot-task wake then
	GPIOC_ODR h@
	1 8 lshift xor
	GPIOC_ODR h!
	stop
    again
;

task: emittertask
: emitter& ( -- )
  emittertask background
    begin
	key? if boot-task wake then
	cr ." emit "
	stop
    again
;

0 variable tick-count

: tick ( -- )
tick-count @ case
dup 100 mod 0= ?of blinkygreentask wake endof
dup 150 mod 0= ?of blinkybluetask wake endof
dup 250 mod 0= ?of emittertask wake endof
endcase
tick-count @ 1+
dup 3000 < if
tick-count !
else
3000 - tick-count !
then
;

 ' tick irq-systick !
 80000    STK_RVR !  \ Set the Systick interval to 10mSec
        7 STK_CSR !  \ Enable the systick interrupt

multitask
blinkygreen&
blinkyblue&
emitter&

