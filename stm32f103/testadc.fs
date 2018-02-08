: test01
	0 1 initADCIns
	adc1_base 1 1 setInSmp
	1 setNumOfChans
	adc1_base 0 0 setInCh
	10 0 do
		cr convIn1
		100 ms
	loop
;

