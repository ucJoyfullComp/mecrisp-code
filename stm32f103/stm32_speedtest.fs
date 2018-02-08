0 0 2variable f_a

: fdiv.step
  f_a 2@ 601296 1 f/ f_a 2! 
;

: loop2k.float
  2000 0 do fdiv.step loop 
;

: test01.float
  0 1234567 f_a 2!
  gpiob_base gpio_bsrr + dup
  1 5 lshift swap !
  loop2k.float
  1 21 lshift swap !
  f_a 2@ f. cr
  100 ms
;

: runme.float
  3 20 lshift gpiob_base gpio_crl + dup >r @ 15 20 lshift not and or r> !
  begin test01.float again
;

0 variable n_a

: div.step
  n_a @ 100000 100014 */ n_a ! 
;

: loop2k.reg
  2000 0 do div.step loop 
;

: test02
  1234567 n_a !
  gpiob_base gpio_bsrr + dup
  1 5 lshift swap !
  loop2k.reg
  1 21 lshift swap !
  n_a @ . cr
  100 ms
;

: runme.reg
  3 20 lshift gpiob_base gpio_crl + dup >r @ 15 20 lshift not and or r> !
  begin test02 again
;


