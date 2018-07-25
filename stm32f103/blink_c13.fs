GPIOC_BASE GPIO_CRL + constant PC-CRL-R
GPIOC_BASE GPIO_CRH + constant PC-CRH-R
GPIOC_BASE GPIO_IDR + constant PC-IDR-R
GPIOC_BASE GPIO_ODR + constant PC-ODR-R
GPIOC_BASE GPIO_BSRR + constant PC-BSRR-R

: SETUP-PC13
	$11 RCC_APB2ENR_R HBIS!
	$00100000 PC-CRH-R ! 
	$20000000 PC-BSRR-R !
;

: blink ( ms blinks -- )
    SETUP-PC13
    depth 2 <
    if
        depth 0= if
            500
        then
        10
    then
	2* 0 DO 
		I 1 and 0=
        if        
		    $2000 PC-ODR-R bic!
        else
    		$2000 PC-ODR-R bis!
        then
		dup ms
	LOOP 
	drop
; 

