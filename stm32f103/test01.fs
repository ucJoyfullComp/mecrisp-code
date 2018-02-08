\ 0 variable stateCntr

: .test 1 stateCntr +! stateCntr @ . .s ;
: getArrayVar ( a-addr u -- d-variance )
  2dup getArrayAvg ( a-addr u -- a-addr u d-avg )
.test
  >r drop \ save the average
  tuck 0 -rot
.test
  0 do \ doing the variance
.test
     dup cell+ swap @ 
.test
     r@ 
.test
     - dup *
.test
     rot + swap
.test
  loop
  r> 2drop
.test
  swap 1- tuck /mod 
.test
  -rot 1000000 rot 
.test
  */mod swap
.test
0 stateCntr !
;
