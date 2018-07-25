GPIOB_BASE GPIO_CRL + constant PB-CRL-R
GPIOB_BASE GPIO_CRH + constant PB-CRH-R
GPIOB_BASE GPIO_IDR + constant PB-IDR-R
GPIOB_BASE GPIO_ODR + constant PB-ODR-R
GPIOB_BASE GPIO_BSRR + constant PB-BSRR-R

: SETUP-PB10_11
	$9 RCC_APB2ENR_R HBIS!
	$00004400 PB-CRH-R ! 
	$0C000000 PB-BSRR-R !
;

0 variable oldstate

: RPI1031-pos ( -- )
    SETUP-PB10_11
	begin
        PB-IDR-R h@ 10 rshift 
        dup oldstate h@ xor 
        0= not if 
            ." S1: " dup 1 and . ." S2: " dup 1 rshift 1 and . cr
            oldstate h!
        else
            drop
        then
		key?
    until 
    .s
; 

: test01 ( -- )
    SETUP-PB10_11
	begin
        PB-IDR-R h@ 10 rshift
        dup oldstate h@ xor
        0= not if
            dup hex . decimal cr
            oldstate h!
        else
            drop
        then
        30 ms
		key?
    until 
; 


