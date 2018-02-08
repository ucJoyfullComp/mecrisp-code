compiletoram

0 0 2variable test_store
    
: test_freq ( -- ) 
\ use port C10 to toggle as fast as possible to check cpu freq 
    %01 10 2* lshift $00300000 gpioc_moder @|+!
    %0 10 lshift $0400 gpioc_otyper @|+!
    %11 10 2* lshift $00300000 gpioc_ospeedr @|+!
    %00 10 2* lshift $00300000 gpioc_pupdr @|+!
    $0400 gpioc_odr test_store 2!
    10000 0 do \ loop 10K times 
    	test_store 2@ hxor!
    loop
;

: maketest ( -- ) 
    ." Test default clock" cr 
    1000 0 do 85 emit loop 
    test_freq 
    SetSysClock 
    ." Test pll clock" cr 
    test_freq 
    cr 
    1000 0 do 85 emit loop
;

