\ Program Name: blinky-5.fs  for Mecrisp-Stellaris by Matthias Koch
\ This program blinks a green led every second using the Systick Interrupt
\ Hardware: STM32F0 Discovery board
\ Requires: The e4thcom Serial Terminal to preload files via the #require command. 
\ E4thcom, Copyright (C) 2013-2017 Manfred Mahlow and licensed under the GP. https://wiki.forth-ev.de/doku.php/en:projects:e4thcom#e4thcom-061
\ Author:  t.porter <terry@tjporter.com.au>

#require blinky-5-register-memory-map.fs

%01  18 lshift GPIOC_MODER bis!	\ Set GPIOC-9 to output mode

: toggle-led
 GPIOC_ODR @			\ Read the GPIOC output data register
 1 9 lshift xor			\ Xor GPIOC-9 with its last value to toggle it
 GPIOC_ODR !			\ Output the new GPIOC-9 value
;

 ' toggle-led irq-systick !
 8000000  STK_RVR !		\ Configure the Systick interval counter. The clock is 8MHz, so this will interrupt every second
        7 STK_CSR !		\ Enable the Systick interrupt
