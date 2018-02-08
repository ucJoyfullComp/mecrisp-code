\ the us@72M gives approximately 1 microsecond per loop
\   for a system clock of 72MHz ( default clock )
: us@72M ( us -- )
    0 do i drop loop
;

: spi-buffer ( "name" u -- ) ( n -- addr|size|len )
  <builds dup 1 < if exit then dup , 0 , allot
  does> swap dup 0 < if
	dup -1 = if
	    drop @ exit
	then
	dup -2 = if
	    drop 1 cells + exit
	then
    then
    2 cells + +
;

