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

0 variable xn
0 variable an
0 variable bn

: sqrt ( x -- sqrt(x) ( in micro integers )
	dup 0 < if drop -1 exit then
	dup 1001 < if	5000 else
		dup 1000000 > if
			dup 1+ 2/
		else 500000
		then
	then
	dup xn ! ( ... -- x x0 )
	over >r  ( ... -- x x0 )
	tuck dup 1000000 */ - 2/ over 1000000 swap */ dup an ! ( ... -- x0 a0 )
	+ dup bn ! ( ... -- b0 )
	an @ dup 1000000 */ 2/ over 1000000 swap */ - dup xn ! ( ... -- x1 )
	r@ over dup 1000000 */ - 2/ over 1000000 swap */ dup an ! ( . -- x1 a1 )
	+ dup bn ! ( ... -- b1 )
	an @ dup 1000000 */ 2/ over 1000000 swap */ - dup xn ! ( ... -- x2 )
	r> over dup 1000000 */ - 2/ over 1000000 swap */ dup an ! ( . -- x2 a2 )
	+ dup bn ! ( ... -- b2 )
	an @ dup 1000000 */ 2/ over 1000000 swap */ - ( ... -- x3 )
;

: phase2pi/4 ( x -- n x1 ) ( n - the part of the circle in octants )
    dup 0= if
        1 swap
    else
        1000 785398163 */mod swap 1000 /
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
        1 of 785398 swap -
        	sin(0<=x<pi/4)
        	dup 1000000 */
        	1000000 swap -
        	sqrt endof
        2 of sin(0<=x<pi/4)
        	dup 1000000 */
        	1000000 swap -
        	sqrt endof
        3 of 785398 swap - sin(0<=x<pi/4) endof
        4 of sin(0<=x<pi/4) negate endof
        5 of 785398 swap -
        	sin(0<=x<pi/4)
        	dup 1000000 */
        	1000000 swap -
        	sqrt negate endof
        6 of sin(0<=x<pi/4)
        	dup 1000000 */
        	1000000 swap -
        	sqrt negate endof
        7 of 785398 swap - sin(0<=x<pi/4) negate endof
    endcase
;


