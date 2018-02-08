: spi2.init
  $4000 rcc_apb1enr_r bis!
  $4000 rcc_apb1rstr_r 2dup bis! 
  20 0 do loop 
  bic!
  $b4b00000 $fff00000 gpiob_base gpio_crh + dup >r bic! r> bis!
  $002c spi2_base spi_cr1 + h!
  $0000 spi2_base spi_cr2 + h!
;

: spi2.en $0040 spi2_base spi_cr1 + hbis! ;
: spi2.dis $0040 spi2_base spi_cr1 + hbic! ;

: spi2.st spi2_base spi_sr + h@ ;

: spi2.rd ( -- hn ) spi2_base spi_dr + h@ ;
: spi2.wr ( hn -- ) spi2_base spi_dr + h! ;

: test.txrx ( n -- )
  spi2.st 
  dup 1 and 1 = if spi2.rd drop then
\  dup 
  2 and 0= if
    begin spi2.st 2 and 2 = until
  then
\  $40 and $40 = if
\    spi2.st drop
\  then
  spi2.wr
;

: spi2.test
  spi2.en
  spi2.st 2 and 0= if
    begin spi2.st 2 and 2 = until
  then
  256 0 do
    i test.txrx
  loop
  spi2.dis
;

spi2.init
spi2.test

