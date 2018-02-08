: spi1.en $40 spi1_base spi_cr1 + tuck h@ or swap h! ;
: spi1.dis $40 spi1_base spi_cr1 + tuck h@ swap not and swap h! ;

: spi1.rd ( -- hn ) spi1_base spi_dr + h@ ;
: spi1.wr ( hn -- ) spi1_base spi_dr + h! ;

: spi1.st spi1_base spi_sr + h@ ;
: spi1.clear.flags
    spi1.st drop
    spi1.rd drop
    spi1_base spi_cr1 + dup h@ swap h!
    0 spi1_base spi_sr + h!
    spi1.st drop
;

: spi1.init
  $1000 rcc_apb2enr_r bis!
  $1000 rcc_apb2rstr_r 2dup bis! 
  200 0 do loop 
  bic!
  200 0 do loop 
  $b0b00000 $f0f00000 gpioa_base gpio_crl + dup >r bic! r> bis!
  $c024 spi1_base spi_cr1 + h!
;

: spi1.tx.byte ( b -- )
  spi1.st 
  dup 1 and 1 = if spi1.rd drop then
  dup 2 and 0= if
      begin spi1.st 2 and 2 = until
  then
  $7c and 0= not if spi1.clear.flags then
  spi1.wr
;

: spi1.tx.@tx1+ ( addr -- addr+1 )
    spi1.st
    dup 1 and 1 = if spi1.rd drop then
    dup 2 and 0= if
	begin spi1.st 2 and 2 = until
    then
    $7c and 0= not if cr spi1.st hex. spi1.clear.flags then
    dup c@ spi1.wr
    1+
;

: spi1.tx.nbytes ( addr n -- )
    spi1.clear.flags
    0 do spi1.tx.@tx1+ loop
    drop
;

0 variable conv-ws2812-temp

: convert-oct-ws2812 ( c -- u24 )
    0 conv-ws2812-temp !
    8 0 do
	1 7 i - lshift over and
	0= if 4 else 6 then
	7 i - 3 * lshift conv-ws2812-temp bis!
    loop
    drop
    conv-ws2812-temp @
;

: convert-color-ws2812 ( addr-color addr-dst -- addr-dst+9 )
    >r ( addr-color R: addr-dst )
    dup c@ convert-oct-ws2812 ( addr-color u24 R: addr-dst )
    dup 65536 / $ff and r@ c!
    dup 256 / $ff and r@ 1+ c!
    $ff and r@ 2 + c!
    dup 1+ c@ convert-oct-ws2812 ( addr-color u24 R: addr-dst )
    dup 65536 / $ff and r@ 3 + c!
    dup 256 / $ff and r@ 4 + c!
    $ff and r@ 5 + c!
    2 + c@ convert-oct-ws2812 ( u24 R: addr-dst )
    dup 65536 / $ff and r@ 6 + c!
    dup 256 / $ff and r@ 7 + c!
    $ff and r@ 8 + c!
    r> 9 +
;

: convert-grb-9oct ( g r b addr -- )
    >r
    rot convert-oct-ws2812 ( g r b R: addr -- r b g24 R: addr )
    dup dup
    $ff and rot
    8 rshift $ff and swap
    16 rshift $ff and
    r@ c! r@ 1+ c!
    r> dup 3 + >r
    2 + c! ( ... -- r b R: addr+3 )
    swap convert-oct-ws2812
    dup dup
    $ff and rot
    8 rshift $ff and swap
    16 rshift $ff and
    r@ c! r@ 1+ c!
    r> dup 3 + >r
    2 + c! ( ... -- b R: addr+6 )
    convert-oct-ws2812
    dup dup
    $ff and rot
    8 rshift $ff and swap
    16 rshift $ff and
    r@ c! r@ 1+ c!
    r> dup 3 + >r
    2 + c! ( ... -- )
;

: convert-grb-9oct-array-old ( addr1 n1 addr2 n2 -- flag )
    >r over 9 * r> > if drop 2drop -1 exit then
    -rot ( addr1 n1 addr2 -- addr2 addr1 n1 )
    0 do ( addr2 addr1 n1 -- addr2 addr1 )
	dup >r over
	r@ c@ r@ 1+ c@ r> 2 + c@ ( a2 a1 a2 R: a1 -- a2 a1 a2 g r b )
	convert-grb-9oct ( a2 a1 a2 g r b -- a2 a1 )
	swap 9 + swap
    loop
    2drop
    0
;

: convert-grb-9oct-array ( addr1 n1 addr2 n2 -- flag )
    >r over 9 * r> > if drop 2drop -1 exit then
    swap 0 do ( addr1 n1 addr2 -- addr1 addr2 )
	2dup convert-color-ws2812 swap drop
	swap 3 + swap
    loop
    2drop
    0
;

: ws2812.latch
    10 0 do
	0 spi1.tx.byte
    loop
    spi1.st $80 and $80 = if
	begin spi1.st $80 and 0= until
    then
;

: grb-array ( n -- addr )
  <builds dup 3 * swap , allot
  does>
    over -1 = if
	@ swap drop
    else
	cell+ swap 3 * +
    then
;

: .3 ( c1 c2 c3 -- ) rot . swap . . ;
: h.3 ( c1 c2 c3 -- ) base @ >r hex rot . swap . . r> base ! ;
: b.3oct ( addr -- )
    dup c@ 16 lshift swap
    1+ dup c@ 8 lshift swap
    1+ c@
    or or
    base @ >r 2 base ! . cr r> base !
;


30 grb-array colors
900 buffer: spi-data

: colors.setR ( n r -- )
    swap colors 1+ c!
;

: colors.setG ( n g -- )
    swap colors c!
;

: colors.setB ( n b -- )
    swap colors 2 + c!
;

: colors.setRGB ( n r g b -- )
    >r >r swap colors r> over r> over ( n r g b -- r a g a b a )
    2 + c!
    c!
    1+ c!
;

: colors.getRGB ( n -- r g b )
    colors dup >r
    1+ c@
    r@ c@
    r> 2 + c@
;

: colors.getR ( n -- r )
    colors 1+ c@
;

: colors.getG ( n -- g )
    colors c@
;

: colors.getB ( n -- b )
    colors 2 + c@
;

: colors.print ( n -- )
    colors.getRGB h.3 cr
;

: colors.clear ( n -- )
    0 0 0 colors.setRGB
;

: colors.clear.all ( -- )
    -1 colors 0 do
	i 0 0 0 colors.setRGB
    loop
;

: colors.setWlevel ( level n -- )
    colors 2dup c! 2dup 1+ c! 2 + c!
;

: colors.setWlevel.all ( level -- )
    0 colors -1 colors 3 * 0 do
	 2dup i + c!
    loop
    2drop
;

: b.color.3oct ( addr -- )
    dup cr
    b.3oct
    3 + dup b.3oct
    3 + b.3oct
;

0 variable light-level

: test-pattern ( -- )
    0 light-level @ 0 0 colors.setRGB
    1 0 light-level @ 0 colors.setRGB
    2 0 0 light-level @ colors.setRGB
    3 0 0 255 light-level @ - colors.setRGB
    4 0 255 light-level @ - 0 colors.setRGB
    5 255 light-level @ - 0 0 colors.setRGB
;

: ws2812.update-colors
    0 colors -1 colors spi-data -1 colors 9 * convert-grb-9oct-array drop
    spi1.en
    spi-data -1 colors 9 * spi1.tx.nbytes
    ws2812.latch
    spi1.dis
;

: ws2812.test.step
    test-pattern
    -1 colors 6 do
	i light-level @ colors.setWlevel
    loop
    ws2812.update-colors
;

: ws2812.test
    5 light-level !
    5
    begin
	ws2812.test.step
	10 light-level +!
	100 ms
	10 + dup 255 >
    until
    colors.clear.all ws2812.update-colors
;

: list-colors
    -1 colors 0 do
	i colors.getRGB .3 cr
    loop
    cr
;

: test-color ( n -- )
    dup colors c@ swap dup colors 1+ c@ swap dup colors 2 + c@ swap
    9 * spi-data + convert-grb-9oct
;

: test-all-colors.print ( -- )
    -1 colors 0 do
	i test-color
    loop
    -1 colors 0 do
	i 9 * spi-data + b.color.3oct
    loop
;

\ spi1.init
\ ws2812.test

\ test-pattern
\ 0 colors -1 colors spi-data 90 convert-grb-9oct-array .s drop
\ spi1.en
\ spi1.st hex. cr spi-data spi1.tx.@tx1+ spi1.st hex.

: test01
    -1 colors 0 do i 0 0 0 colors.setRGB loop
    1 $ff dup dup colors.setRGB
    0 colors -1 colors spi-data -1 colors 9 *
    convert-grb-9oct-array .
    spi1.en
    spi-data -1 colors 9 * spi1.tx.nbytes
    ws2812.latch
    spi1.dis
;

: test02 ( addr u -- )
    spi1.en
    spi1.tx.nbytes
    ws2812.latch
    spi1.dis
;

: test03 ( addr n -- )
    0 do ( addr n -- addr )
	cr
	3 0 do
	    dup
	    j 9 * i 3 * + + b.3oct
	loop
    loop
    cr
    drop
;

: test04
    0 colors.setWlevel.all
    ws2812.update-colors
    1000 ms
    50 colors.setWlevel.all
    ws2812.update-colors
    1000 ms
    100 colors.setWlevel.all
    ws2812.update-colors
    1000 ms
    150 colors.setWlevel.all
    ws2812.update-colors
    1000 ms
    200 colors.setWlevel.all
    ws2812.update-colors
    1000 ms
    255 colors.setWlevel.all
    ws2812.update-colors
    1000 ms
    0 colors.setWlevel.all
    ws2812.update-colors
;    

spi1.init
colors.clear.all
ws2812.update-colors
