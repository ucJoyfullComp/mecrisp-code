\ setup timer TIM2 for 100KHz time-base, 
\  timer output compare channels 1 to 4 are set to PWM1 with 0
\  the io of the Output Channels is set by alternate function
\  to A15=CH1, B3=CH2, B10=CH3, B11=CH4


\ afio_base 4 + $4000000 over $7000000 swap bic! swap bis!
\ afio_base 4 + $300 swap bis!
\ gpioa_base 15 11 config-gpio-pin
\ 100000 getsysclock swap / dup 1- 2 set-tim-2to4-timebase
\ 10 / 1- 1 2 set-tim-oc
\ 360 1 2 set-tim-oc

: setup-tim2&io
    afio_base 4 + $4000000 over $7000000 swap bic! swap bis!
    afio_base 4 + $300 swap bis!
    gpioa_base 15 11 config-gpio-pin
    gpiob_base  3 11 config-gpio-pin
    gpiob_base 10 11 config-gpio-pin
    gpiob_base 11 11 config-gpio-pin
    100000 getsysclock swap / dup 1- 2 set-tim-2to4-timebase
    0 1 2 set-tim-oc
    0 2 2 set-tim-oc
    0 3 2 set-tim-oc
    0 4 2 set-tim-oc
;


