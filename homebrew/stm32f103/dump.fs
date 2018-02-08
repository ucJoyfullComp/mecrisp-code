( this is a color hexdump utility for the Mecrisp Forth )
( environment.  It also provides an essential interface )
( for ANSI color text formatting                        )

: esc         27 emit ;

: red         esc ." [31;1m" ;
: green       esc ." [32;1m" ;
: yellow      esc ." [33;1m" ;
: blue        esc ." [34;1m" ;
: magenta     esc ." [35;1m" ;
: cyan        esc ." [36;1m" ;
: white       esc ." [37;1m" ;
: rst         esc ." [0m" ;

: dump.h      16 mod 0= if cr dup i + hex . decimal ." : " then ;
: nibbles     dup $f and swap $f0 and 4 rshift ;
: bs          8 emit ;

: color       dup 0= if magenta exit then
    dup $20 < if blue exit then
    dup $7e > if cyan exit then
    dup $30 < if yellow exit then
    green ;

( OK, so emitting a backspace is a little bit of a hack )
( here, but it keeps me from having to implement a      )
( space-free '.' word.                                  )

: .nibbles nibbles hex . bs . bs decimal ;

: dump ( addr length -- )
    cr 0 do
	i dump.h
	dup i + c@
	color .nibbles space rst
    loop
    cr drop ;

: dumpraw ( addr length -- )
    cr 0 do
	i 16 mod 0= if cr then
	dup i + c@ .nibbles
    loop
    cr drop ;

