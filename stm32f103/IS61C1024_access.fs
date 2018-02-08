( C15                  -- WE_ )
( A11                  -- OE_ )
( A12                  -- CE1_ )
( B0:1,3:15,C13:14     -- A0:16 )
( A0:7                 -- D0:7 )

: setup-sram ( -- )
( setting the pullup or pulldown conditions )
   $1800 gpioa_base gpio_odr + h!
   $0000 gpiob_base gpio_odr + h!
   $8000 gpioc_base gpio_odr + h!
( setting the pins for input or output default )
   2 0 do
    gpiob_base i 3 config-gpio-pin
   loop
   13 0 do
    gpiob_base i 3 + 3 config-gpio-pin
   loop
   2 0 do
    gpioc_base i 13 + 3 config-gpio-pin
   loop
   8 0 do
    gpioa_base i 4 config-gpio-pin
   loop
   gpioa_base 11 3 config-gpio-pin
   gpioa_base 12 3 config-gpio-pin
   gpioc_base 15 3 config-gpio-pin
;

: set-sram-address ( addr -- )
   $1ffff and dup
   >r dup $18000 and 2 rshift
   $6000 gpioc_base gpio_odr + dup
   >r hbic! r> hbis!
   $7fff and dup 3 and swap
   $7ffc and 1 lshift or
   gpiob_base gpio_odr +
   >r r@ h@ 4 and or r> r> drop h!
;

: write-sram ( addr 8b-data -- )
   $1800 gpioa_base gpio_odr + hbis!
   $8000 gpioc_base gpio_odr + hbis!
   swap set-sram-address
   $ff and 
   gpioa_base gpio_odr + h@ 
   $ff00 and or gpioa_base gpio_odr + h!
   $33333333 gpioa_base gpio_crl + !
   $1000 gpioa_base gpio_odr + hbic!
   $8000 gpioc_base gpio_odr + hbic!
   $8000 gpioc_base gpio_odr + hbis!
   $1000 gpioa_base gpio_odr + hbis!
   $44444444 gpioa_base gpio_crl + !
;

: read-sram ( addr -- 8b-data )
   $1800 gpioa_base gpio_odr + hbis!
   $8000 gpioc_base gpio_odr + hbis!
   set-sram-address
   $1800 gpioa_base gpio_odr + hbic!
   gpioa_base gpio_idr + h@ $ff and
   $1800 gpioa_base gpio_odr + hbis!
   $44444444 gpioa_base gpio_crl + !
;

: read-sram-block ( addr c-addr u -- 8b-data )
   $1800 gpioa_base gpio_odr + hbis!
   $8000 gpioc_base gpio_odr + hbis!
   0 do
    2dup swap
    i + set-sram-address
    $1800 gpioa_base gpio_odr + hbic!
    gpioa_base gpio_idr + h@ $ff and 
    swap i + c!
    $1800 gpioa_base gpio_odr + hbis!
   loop
   $44444444 gpioa_base gpio_crl + !
   2drop
;

setup-sram
0 set-sram-address

