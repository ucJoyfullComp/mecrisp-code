compiletoflash
compiletoram

\
\ set DC/DC converter on TIM4 OC1 as PWM output for the inductor FET driver
\ and port A1 as the output value analog sampling input, and port A2 as the
\ reference input 
\ the main application is to increese the voltage on A1 is equal to the
\ reference voltage on A2 and to regulate the voltage on A1 to be equal
\ to that on A2 via the PWM pulse on B6
\
NVIC_BASE NVIC_ISER0 + constant NVIC_EN0_R
$00000800 constant NVIC_EN0_INT11          \ DMA1CH1 interrupt 11
$00001000 constant NVIC_EN0_INT12          \ DMA1CH2 interrupt 12
$20000000 constant NVIC_EN0_INT29          \ TIM3 interrupt 29
$40000000 constant NVIC_EN0_INT30          \ TIM4 interrupt 30

tim4_base tim_arr + constant tim4_arr_r
tim4_base tim_ccr1 + constant tim4_ccr1_r
tim4_base tim_ccr1 + constant tim4_ccr2_r

\ test that min <= n <= max return true|false
: between ( n min max -- flag )
	>r over r> 1+ < >r swap 1+ < r> and
;

\ test than n is in range 0..15 return true|false
: valid-pin? ( n -- flag )
	0 15 between
;


