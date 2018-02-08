\ Program Name: blinky-1.fs  for Mecrisp-Stellaris by Matthias Koch
\ This program blinks a green led, it's the simplest of 'blinkies' 
\ Hardware: STM32F0 Discovery board
\ Author:  t.porter <terry@tjporter.com.au>


%01  18 lshift $48000800 bis!		

: half-second-delay 400000 0 do loop ;

: green-led.on   %1  9 lshift $48000818 bis! ;	

: green-led.off  %1 9 lshift $48000828 bis! ; 
 
: blink		
do
green-led.on
half-second-delay
green-led.off
half-second-delay
loop
;	


blink
