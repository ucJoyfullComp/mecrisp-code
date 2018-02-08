\ listrangehex - list a range of memory content in hexadecimal quad-octet
: .[]$ ( a n -- )
	cr
	0 do
		dup 
		i cells 
		dup hex. 
		+ @ hex.
		cr
	loop
	drop
;

: @|+! ( n mask addr -- )
	dup >r ( n mask addr -- n mask addr R: addr )
	@ swap ( n mask addr R: addr -- n [addr] mask R: addr )
	not and ( n [addr] mask R: addr -- n masked-val R: addr )
	or r> ! ( n masked-val R: addr -- )
;

