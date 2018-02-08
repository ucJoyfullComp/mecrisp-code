: test01 ( n -- )
		dup colors c@ swap dup colors 1+ c@ swap dup colors 2 + c@ swap
		9 * spi-data + convert-grb-9oct
;

: test02 ( -- )
		-1 colors 0 do
			i test01
		loop


