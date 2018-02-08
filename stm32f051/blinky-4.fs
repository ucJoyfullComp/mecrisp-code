\ Program Name: blinky-4.fs  for Mecrisp-Stellaris by Matthias Koch
\ This program blinks a green led using cooperate multitasking
\ Hardware: STM32F0 Discovery board
\ Requires: The e4thcom Serial Terminal to preload files via the #require command. 
\ E4thcom, Copyright (C) 2013-2017 Manfred Mahlow and licensed under the GP. https://wiki.forth-ev.de/doku.php/en:projects:e4thcom#e4thcom-061
\ Author:  t.porter <terry@tjporter.com.au>


compiletoram

#require blinky-3-register-memory-map.fs
#require multitask.fs

%01  18 lshift GPIOC_MODER bis!		

: half-second-delay 25000 0 do pause loop ;
: green-led.on   %1  9 lshift GPIOC_BSRR bis! pause ;	
: green-led.off  %1 9 lshift GPIOC_BRR bis! pause ; 
 

: blink		
green-led.on
half-second-delay
green-led.off
half-second-delay
;	


task: blinktask
: blinky& ( -- )
  blinktask activate
    begin  blink again
;

multitask
blinky& 

