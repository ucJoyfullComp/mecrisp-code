GPIOB_BASE GPIO_CRL + constant PB-CRL-R
GPIOB_BASE GPIO_CRH + constant PB-CRH-R
GPIOB_BASE GPIO_IDR + constant PB-IDR-R
GPIOB_BASE GPIO_ODR + constant PB-ODR-R
GPIOB_BASE GPIO_BSRR + constant PB-BSRR-R

: SETUP-PB5
	$9 RCC_APB2ENR_R HBIS!
	$00100000 PB-CRL-R ! 
	$00200000 PB-BSRR-R !
;

SETUP-PB5

: tpb5-ms ( ms -- )
	1024 0 DO 
		I 1 and 0=
        if        
		    $20 PB-ODR-R bic!
        else
    		$20 PB-ODR-R bis!
        then
		dup ms
	LOOP 
	drop
; 

