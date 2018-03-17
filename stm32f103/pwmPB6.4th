\ assuming TIM4 is set to generate timebase of 48KHz

\ set port B6 to TIM4 capture compare push-pull
gpiob_base 6 11 config-gpio-pin
\ set TIM4 output compare to PWM1 and compare value of 100
100 1 set-tim4-oc

