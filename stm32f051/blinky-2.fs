\ Program Name: blinky-2.fs  for Mecrisp-Stellaris by Matthias Koch
\ This program blinks a green led, but this time uses CMSIS-SVD compliant register names rather than raw memmory addressing
\ Hardware: STM32F0 Discovery board
\ Author:  t.porter <terry@tjporter.com.au>


$48000800 constant GPIOC		\ The led is connected to GPIO Port C, bit 9 
GPIOC $0 + constant GPIOC_MODER
GPIOC $18 + constant GPIOC_BSRR
GPIOC $28 + constant GPIOC_BRR

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
