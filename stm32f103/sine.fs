\ phase in microradians

: deg2rad ( deg -- rad )
    7853982 450000000 */
;

: phase2circle ( x -- x%2pi )
    10 62831853 */mod drop
;

: pi/2-phase ( x -- y )
    1570796 swap -
;

: sqrt ( x -- sqrt(x) )
    
;

: phase2pi/4 ( x -- n x1 ) ( n - the part of the circle in octants )
    dup 0= if 
        1 swap
    else
        1000 785398163 */mod swap dup 0= if
            drop 785398
        then
    then
;

: sin(0<=x<pi/4) ( x -- sinx )
    dup dup >r
    r@ 1000000 */ r@ 6000000 */ dup >r
    - r> r@ 4000000 */ r@ 5000000 */ dup >r
    + r> r@ 6000000 */ r> 7000000 */ -
;

: sin ( x -- sinx ) ( in micro radian )
    phase2pi/4
    swap case
        0 of sin(0<=x<pi/4) endof
        1 of ." 2nd octant" cr endof
        2 of ." 3rd octant" cr endof
        3 of ." 4th octant" cr endof
        4 of ." 5th octant" cr endof
        5 of ." 6th octant" cr endof
        6 of ." 7th octant" cr endof
        7 of ." 8th octant" cr endof
    endcase
;


