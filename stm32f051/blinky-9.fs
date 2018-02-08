\ Program Name: blinky-9.fs  for Mecrisp-Stellaris by Matthias Koch
\ This program blinks a blue led at a much faster rate than blinky-8.fs, it's the simplest of 'blinkies'
\ Hardware: STM32F0 Discovery board
\ Author:  t.porter <terry@tjporter.com.au>


%01  16 lshift $48000800 bis!

: half-second-delay 40000 0 do loop ;

: blue-led.on   %1  8 lshift $48000818 bis! ;

: blue-led.off  %1 8 lshift $48000828 bis! ;

: blink
do
blue-led.on
half-second-delay
blue-led.off
half-second-delay
loop
;


blink
