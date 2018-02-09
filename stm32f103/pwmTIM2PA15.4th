afio_base 4 + $4000000 over $7000000 swap bic! swap bis!
afio_base 4 + $300 swap bis!
gpioa_base 15 11 config-gpio-pin
100000 getsysclock swap / dup 1- 2 set-tim-2to4-timebase
10 / 1- 1 2 set-tim-oc
360 1 2 set-tim-oc

