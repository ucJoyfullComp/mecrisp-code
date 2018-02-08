\ Program Name: blinky-5-register-memory-map.fs 
\ Required by: blinky-5.fs and blinky-6.fs
\ Hardware: STM32F0 Discovery board
\ Author:  t.porter <terry@tjporter.com.au>


$48000800 constant GPIOC		 
GPIOC $0 + constant GPIOC_MODER
GPIOC $14 + constant GPIOC_ODR
GPIOC $18 + constant GPIOC_BSRR
GPIOC $28 + constant GPIOC_BRR
$E000E010 CONSTANT STK_CSR	    \ SysTick Control and Status Register
$E000E014 CONSTANT STK_RVR	    \ SysTick Reload Value Register


