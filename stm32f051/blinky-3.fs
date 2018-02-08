\ Program Name: blinky-3.fs  for Mecrisp-Stellaris by Matthias Koch
\ This program blinks a green led, but this time uses CMSIS-SVD compliant register names rather than raw memmory addressing
\ Hardware: STM32F0 Discovery board
\ Requires: The e4thcom Serial Terminal to preload blinky3-register-memory-map.fs via the #require command. 
\ E4thcom, Copyright (C) 2013-2017 Manfred Mahlow and licensed under the GP. https://wiki.forth-ev.de/doku.php/en:projects:e4thcom#e4thcom-061
\ Author:  t.porter <terry@tjporter.com.au>


#require blinky-3-register-memory-map.fs


%01  18 lshift GPIOC_MODER bis!		

: half-second-delay 400000 0 do loop ;

: green-led.on   %1  9 lshift GPIOC_BSRR bis! ;	

: green-led.off  %1 9 lshift GPIOC_BRR bis! ; 
 
: blink		
do
green-led.on
half-second-delay
green-led.off
half-second-delay
loop
;	


blink
