\ 8 Queens benchmark at 168MHz fcpu
\ STM32F407 Discovery board
\ by Igor de om1zz, 2015
\ no warranties of any kind

0 variable millis

: millisc 1 millis +! ;

: inimillis ( -- ) 
  ['] millisc irq-systick !
  168000 systick
  eint
;

0 variable solutions
0 variable nodes
 
: bits ( n -- mask ) 1 swap lshift 1- ;
: lowBit  ( mask -- bit ) dup negate and ;
: lowBit- ( mask -- bits ) dup 1- and ;
 
: next3 ( dl dr f files -- dl dr f dl' dr' f' )
  not >r
  2 pick r@ and 2* 1+
  2 pick r@ and 2/
  2 pick r> and ;
 
: try ( dl dr f -- )
  dup if
    1 nodes +!
    dup 2over and and
    begin ?dup while
      dup >r lowBit next3 recurse r> lowBit-
    repeat
  else 1 solutions +! then
  drop 2drop ;
 
: queens ( n -- )
  0 solutions ! 0 nodes !
  -1 -1 rot bits try
  solutions @ . ." solutions, " nodes @ . ." nodes" ;
  
: testQ ( N -- ) millis @ swap queens millis @ swap - ."   t= " . ." ms" ;
  
\
\ STM32F407, 168MHz clock, serial 115k2:
\ ==============================================
\ 8 testQ 92 solutions, 1965 nodes  t= 4 ms ok.
\ 10 testQ 724 solutions, 34815 nodes  t= 43 ms ok.
\ 12 testQ 14200 solutions, 841989 nodes  t= 1002 ms ok.
\ 13 testQ 73712 solutions, 4601178 nodes  t= 5457 ms ok.
\ 14 testQ 365596 solutions, 26992957 nodes  t= 31936 ms ok.
\ 16 testQ 14772512 solutions, 1126417791 nodes  t= 1331997 ms ok.

