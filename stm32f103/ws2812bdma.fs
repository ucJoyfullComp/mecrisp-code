NVIC_BASE NVIC_ISER0 + constant NVIC_EN0_R
$20000000 constant NVIC_EN0_INT29          \ TIM3 interrupt 29
$40000000 constant NVIC_EN0_INT30          \ TIM4 interrupt 30

tim4_base tim_cr1 + constant tim4_cr1_r
tim4_base tim_ccr1 + constant tim4_ccr1_r
tim4_base tim_sr + constant tim4_sr_r

: init-pb6 gpiob_base gpio_crl + dup @ $f000000 bic $b000000 or swap ! ;
: init-tim4
  $4 rcc_apb1enr_r bis!
  100 0 do i drop loop
  0 tim4_cr1_r h!
  0 tim4_base tim_cr2 + h!
  0 tim4_base tim_smcr + h!
  1 tim4_base tim_dier + h!
  $1e5f tim4_sr_r hbic!
  $60 tim4_base tim_ccmr1 + h!
  1 tim4_base tim_ccer + h!
  89 tim4_base tim_arr + h! 
  0 tim4_ccr1_r h! 
;

1 constant WS2811
2 constant WS2812
3 constant WS2812B
WS2812B variable WS-TYPE
0 variable NLEDS

: populate-zero-ws2811 36 tim4_ccr1_r h! ; inline

: populate-one-ws2811 86 tim4_ccr1_r h! ; inline

: populate-zero-ws2812 25 tim4_ccr1_r h! ; inline

: populate-one-ws2812 50 tim4_ccr1_r h! ; inline

: populate-zero-ws2812b 29 tim4_ccr1_r h! ; inline

: populate-one-ws2812b 58 tim4_ccr1_r h! ; inline

: RGBleds ( size nleds -- )
  create 3 , , dup , 3 * allot 
  does>
;

: .columns ; inline

: .size 2 cells + ; inline

: .len 1 cells + ; inline

: .arr 3 cells + swap 3 * + @ $FFFFFF and ;

16 3 RGBleds LEDScolors
0 variable LED-CCR
$800000 variable LED-CCR-MASK
0 variable LED-CCR-CNT

\ TIM4 Update interrupt routine
: load-ws2811
  1 tim4_base tim_sr + bic!
  LED-CCR-MASK @ 0= if
    $800000 LED-CCR-MASK !
    1 LED-CCR-CNT +!
    LED-CCR-CNT @ LEDScolors .len @ = if \ last LED
      $40 gpiob_base gpio_odr + hbic! 
      gpiob_base gpio_crl + dup @ $f000000 bic $3000000 or swap !
      1 tim4_base tim_cr1 + bic!
      0 LED-CCR-CNT !
      exit
    else
      LED-CCR-CNT @ LEDScolors .arr
      dup LED-CCR !
    then
  else
    LED-CCR @
  then
  LED-CCR-MASK @ and if
    populate-one-ws2811
  else
    populate-zero-ws2811
  then
  LED-CCR-MASK dup @ 1 rshift swap !
;

: load-ws2812
  1 tim4_base tim_sr + bic!
  LED-CCR-MASK @ 0= if
    $800000 LED-CCR-MASK !
    1 LED-CCR-CNT +!
    LED-CCR-CNT @ LEDScolors .len @ = if \ last LED
      $40 gpiob_base gpio_odr + hbic! 
      gpiob_base gpio_crl + dup @ $f000000 bic $3000000 or swap !
      1 tim4_base tim_cr1 + bic!
      0 LED-CCR-CNT !
      exit
    else
      LED-CCR-CNT @ LEDScolors .arr
      dup LED-CCR !
    then
  else
    LED-CCR @
  then
  LED-CCR-MASK @ and if
    populate-one-ws2812
  else
    populate-zero-ws2812
  then
  LED-CCR-MASK dup @ 1 rshift swap !
;

: load-ws2812b
  1 tim4_base tim_sr + bic!
  LED-CCR-MASK @ 0= if
    $800000 LED-CCR-MASK !
    1 LED-CCR-CNT +!
    LED-CCR-CNT @ LEDScolors .len @ = if \ last LED
      $40 gpiob_base gpio_odr + hbic! 
      gpiob_base gpio_crl + dup @ $f000000 bic $3000000 or swap !
      1 tim4_base tim_cr1 + bic!
      0 LED-CCR-CNT !
      exit
    else
      LED-CCR-CNT @ LEDScolors .arr
      dup LED-CCR !
    then
  else
    LED-CCR @
  then
  LED-CCR-MASK @ and if
    populate-one-ws2812b
  else
    populate-zero-ws2812b
  then
  LED-CCR-MASK dup @ 1 rshift swap !
;
\ End of interrupt routines

: set-number-leds ( n addr -- )
  swap
  dup 1 < if
    drop 0
    NLEDS !
    0 swap .len !
    exit
  then
  dup NLEDS !
  swap .len !
;

: set-led-color ( r g b n addr -- )
  swap
  1 cells 3 * + >r
  rot r@ c!
  r@ 2 + c!
  r> 1 + c!
;

: get-led-color ( n addr -- r g b )
  swap
  1 cells 3 * + >r
  r@ c@
  r@ 1 + c@
  r> 2 + c@
;

: init-leds
  init-pb6
  WS-TYPE @ WS2811 = if ['] load-ws2811 then
  WS-TYPE @ WS2812 = if ['] load-ws2812 then
  WS-TYPE @ WS2812B = if ['] load-ws2812b then
  irq-tim4 !
  init-tim4
  \ enable timer TIM4 interrupt in NVIC
  \ Enable TIM4 Interrupt in global Interrupt Controller
  NVIC_EN0_INT30 NVIC_EN0_R ! 
;

: update-leds ( -- )
  
; 

init-leds
0 tim4_ccr1_r + h!
1 tim4_cr1_r + bis!

