: spi1.init
  $1000 rcc_apb2enr_r bis!
  $1000 rcc_apb2rstr_r 2dup bis! 
  20 0 do loop 
  bic!
  $b4b00000 $fff00000 gpioa_base gpio_crl + dup >r bic! r> bis!
  $002c spi1_base spi_cr1 + h!
  $0000 spi1_base spi_cr2 + h!
;

: spi1.en $0040 spi1_base spi_cr1 + hbis! ;
: spi1.dis $0040 spi1_base spi_cr1 + hbic! ;

: spi1.st spi1_base spi_sr + h@ ;

: spi1.rd ( -- hn ) spi1_base spi_dr + h@ ;
: spi1.wr ( hn -- ) spi1_base spi_dr + h! ;

: test.txrx ( n -- )
  spi1.st 
  dup 1 and 1 = if spi1.rd drop then
\  dup 
  2 and 0= if
    begin spi1.st 2 and 2 = until
  then
\  $40 and $40 = if
\    spi1.st drop
\  then
  spi1.wr
;

: spi1.test
  spi1.en
  spi1.st 2 and 0= if
    begin spi1.st 2 and 2 = until
  then
  256 0 do
    i test.txrx
  loop
  spi1.dis
;

spi1.init
spi1.test

