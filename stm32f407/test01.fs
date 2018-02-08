GPIOC_BASE GPIO_ODR + constant GPIOC_ODR

: test01
    init-delay
    %01 7 2* lshift $0000C000 
    gpioc_base gpio_moder +	@|+!
    %0 7 lshift $0080 
    gpioc_base gpio_otyper + @|+!
    %11 7 2* lshift $0000C000 
    gpioc_base gpio_ospeeder + @|+!
    %00 7 2* lshift $0000C000 
    gpioc_base gpio_pupdr + @|+!
    begin
      $0080 GPIOC_ODR hxor!
      300 us
      key?
    until
;

