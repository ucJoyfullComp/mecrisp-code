\ compiletoflash

\ Calculate Bitlog and Bitexp - close relatives to logarithm and exponent to base 2.

: bitlog ( u -- u )

 \ Invented by Tom Lehman at Invivo Research, Inc., ca. 1990
 \ Gives an integer analog of the log function
 \ For large x, B(x) = 8*(log(base 2)(x) - 1)

  dup 8 u<= if 1 lshift
            else
              ( u )
              30 over clz - 3 lshift
              ( u s1 )
              swap
              ( s1 u )
              28 over clz - rshift 7 and
              ( s1 s2 ) +
            then

  1-foldable ;


: bitexp ( u -- u )

  \ Returns an integer value equivalent to
  \ the exponential. For numbers > 16,
  \ bitexp(x) approx = 2^(x/8 + 1)

  \ B(E(x)) = x for 16 <= x <= 247.

  dup 247 u>  \ Overflow ?
  if drop $F0000000
  else

    dup 16 u<= if 1 rshift
               else
                 dup ( u u )
                 7 and 8 or ( u b )
                 swap ( b u )
                 3 rshift 2 - lshift
               then

  then

  1-foldable ;


\ Emulate c, which is not available in hardware on some chips.

0 variable c,collection

: c, ( c -- )
  c,collection @ ?dup if $FF and swap 8 lshift or h,
                         0 c,collection !
                      else $100 or c,collection ! then ;

: calign ( -- )
  c,collection @ if 0 c, then ;


( Roman numerals taken from Leo Brodies "Thinking Forth" )

create romans
  (      ones ) char I c,  char V c,
  (      tens ) char X c,  char L c,
  (  hundreds ) char C c,  char D c,
  ( thousands ) char M c,
  calign

  align   \ In this chip 16 bit writes are emulated internally, too,
          \ as its Flash controller has 32-bits-at-once write access only.
          \ Aligning on 4 ensures that this is actually written.

0 variable column# ( current_offset )
: ones      0 column# ! ;
: tens      2 column# ! ;
: hundreds  4 column# ! ;
: thousands 6 column# ! ;

: column ( -- address-of-column ) romans column# @ + ;

: .symbol ( offset -- ) column + c@ emit ;
: oner  0 .symbol ;
: fiver 1 .symbol ;
: tener 2 .symbol ;

: oners ( #-of-oners )
  ?dup if 0 do oner loop then ;

: almost ( quotient-of-5 -- )
  oner if tener else fiver then ;

: romandigit ( digit -- )
  5 /mod over 4 = if almost drop else if fiver then oners then ;

: roman ( number -- )
  1000 /mod thousands romandigit
   100 /mod  hundreds romandigit
    10 /mod      tens romandigit
                 ones romandigit ;


\ From Gerry Jackson on comp.lang.forth

: sm/rem ( d n -- r q ) m/mod inline 3-foldable ;

: fm/mod ( d n -- r q )
   dup >r 2dup xor 0< >r sm/rem
   over r> and if 1- swap r> + swap exit then r> drop
   3-foldable
;

\ #######   RANDOM   ##########################################

\ setseed   sets the random number seed
\ random    returns a random 32-bit number
\
\ based on "Xorshift RNGs" by George Marsaglia
\ http://www.jstatsoft.org/v08/i14/paper

$7a92764b variable seed

: setseed   ( u -- )
    dup 0= or       \ map 0 to -1
    seed !
;

: random    ( -- u )
    seed @
    dup 13 lshift xor
    dup 17 rshift xor
    dup 5  lshift xor
    dup seed !
    57947 *
;

: randrange  ( u0 -- u1 ) \ u1 is a random number less than u0
    random um* nip
;

compiletoflash

\ Sine and Cosine with Cordic algorithm

: numbertable <builds does> swap 2 lshift + @ ;

hex
numbertable e^ka

C90FDAA2 ,
76B19C15 ,
3EB6EBF2 ,
1FD5BA9A ,
0FFAADDB ,
07FF556E ,
03FFEAAB ,
01FFFD55 ,

00FFFFAA ,
007FFFF5 ,
003FFFFE ,
001FFFFF ,
000FFFFF ,
0007FFFF ,
0003FFFF ,
0001FFFF ,

0000FFFF ,
00007FFF ,
00003FFF ,
00001FFF ,
00000FFF ,
000007FF ,
000003FF ,
000001FF ,

000000FF ,
0000007F ,
0000003F ,
0000001F ,
0000000F ,
00000007 ,
00000003 ,
00000001 ,

decimal

: 2rshift 0 ?do d2/ loop ;

: cordic ( f-angle -- f-error f-sine f-cosine )
         ( Angle between -Pi/2 and +Pi/2 ! )
  0 0 $9B74EDA8 0
  32 0 do
    2rot dup 0<
    if
      i e^ka 0 d+ 2rot 2rot
            2over i 2rshift 2rot 2rot
      2swap 2over i 2rshift
      d- 2rot 2rot d+
    else
      i e^ka 0 d- 2rot 2rot
            2over i 2rshift 2rot 2rot
      2swap 2over i 2rshift
      d+ 2rot 2rot 2swap d-
    then
  loop
2-foldable ;

: sine   ( f-angle -- f-sine )   cordic 2drop 2nip   2-foldable ;
: cosine ( f-angle -- f-cosine ) cordic 2nip  2nip   2-foldable ;

3,141592653589793  2constant pi
pi 2,0 f/ 2constant pi/2
pi 4,0 f/ cosine f. \ Displays cos(Pi/4)

: widecosine ( f-angle -- f-cosine )
  dabs
  pi/2 ud/mod drop 3 and ( Quadrant f-angle )

  case
    0 of                 cosine         endof
    1 of dnegate pi/2 d+ cosine dnegate endof
    2 of                 cosine dnegate endof
    3 of dnegate pi/2 d+ cosine         endof
  endcase

  2-foldable ;

: widesine ( f-angle -- f-sine )
  dup >r \ Save sign
  dabs
  pi/2 ud/mod drop 3 and ( Quadrant f-angle )

  case
    0 of                 sine          endof
    1 of dnegate pi/2 d+ sine          endof
    2 of                 sine  dnegate endof
    3 of dnegate pi/2 d+ sine  dnegate endof
  endcase

  r> 0< if dnegate then
  2-foldable ;


\ Fast integer square root. Algorithm from the book "Hacker's Delight".

: isqrt ( u -- u^1/2 )
  [
  $2040 h, \   movs r0, #0x40
  $0600 h, \   lsls r0, #24
  $2100 h, \   movs r1, #0
  $000A h, \ 1:movs r2, r1
  $4302 h, \   orrs r2, r0
  $0849 h, \   lsrs r1, #1
  $4296 h, \   cmp r6, r2
  $D301 h, \   blo 2f
  $1AB6 h, \   subs r6, r2
  $4301 h, \   orrs r1, r0
  $0880 h, \ 2:lsrs r0, #2
  $D1F6 h, \   bne 1b
  $000E h, \   movs r6, r1
  ]
  1-foldable
;

: sqr ( n -- n^2 ) dup * 1-foldable ;

\ Small examples for all targets

: invert  not  inline 1-foldable ;
: octal 8 base ! ;
: star 42 emit ;

\ compiletoram

