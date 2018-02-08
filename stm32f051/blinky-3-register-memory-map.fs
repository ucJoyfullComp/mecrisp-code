\ Program Name: blinky-3-register-memory-map.fs 
\ Required by: blinky-3.fs
\ Hardware: STM32F0 Discovery board
\ Author:  t.porter <terry@tjporter.com.au>


$48000800 constant GPIOC		 
GPIOC $0 + constant GPIOC_MODER
GPIOC $18 + constant GPIOC_BSRR
GPIOC $28 + constant GPIOC_BRR


