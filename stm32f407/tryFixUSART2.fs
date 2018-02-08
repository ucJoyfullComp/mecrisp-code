: FixUSART2 ( baudrate -- )
    \ fix USART2 setup for new clock
    \ stop USART2
    $000C usart2_base usart_cr1 + dup h@ 
    rot not and $2000 or swap h!
    \ calculate the value of BRR register
    GetApb1Clock swap ( baudrate -- Apb1Clock baudrate )
    $8000 usart2_base usart_cr1 + hbit@ if
	\ oversample by 8
	tuck 8 * /mod 2* >r over 2/ + swap / r> +
    else
	\ oversample by 16
	tuck /mod -rot 2* < -1 * + 
    then
    \ we are now left with the value of USART_BRR on stack
    \ store into the register.
    usart2_base usart_brr + h!
    \ re-enable the USART2
    $2000 usart2_base usart_cr1 + dup h@ rot or swap h!
    200 0 do i drop loop
    $000C usart2_base usart_cr1 + dup h@ rot or swap h! 
    usart2_base usart_sr + h@ 8 and 
    if
	usart2_base usart_dr + h@ drop 
    then
;

