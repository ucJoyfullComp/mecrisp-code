: exti2.isr
    1 EXTI_PR bit@ if 
	1 GPIOB_ODR xor!
	1 EXTI_PR !
    then
;

: init.exti2
    $1 RCC_APB2ENR bis!
    \ output Pin PC0
    $00000001 $00000003 GPIOB_MODER tuck bic! bis!
    $0001 GPIOB_OTYPER bic!
    $00000003 GPIOB_OSPEEDR bis!
    $00000003 GPIOB_PUPDR bic!
    $0001 GPIOB_ODR bic!
    \ Input Pin PA3 - EXTI source
    $00000000 $00000003 GPIOA_MODER tuck bic! bis!
    $00000002 $00000003 GPIOA_PUPDR tuck bic! bis!
    \ set EXTI pin0 source
    0 $000F SYSCFG_EXTICR1 tuck bic! hbis!
    1 EXTI_IMR bis!
    1 EXTI_RTSR bis!
    1 EXTI_FTSR bis!
    \ enable interrupt in NVIC
    1 6 lshift NVIC_IPER bis!
    6 4 mod 4 * >r $40 r@ lshift $FF r> lshift
    NVIC_IPR1 tuck bic! bis!
;

