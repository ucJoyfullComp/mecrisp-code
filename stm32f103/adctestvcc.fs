: testVddTemp
    $800000 adc1_base adc_cr2 + bis!
    1 ms
    adc1_base 4 16 setInSmp
    adc1_base 4 17 setInSmp
    adc1_base adc_sqr3 + dup @ $1f bic 17 or swap !
    1990000 4095
    convertADC1-single
    */ .
    adc1_base adc_sqr3 + dup @ $1f bic 16 or swap !
    convertADC1-single Vdda @ 4095 */
    1322500 - 4300 / .
    $800000 adc1_base adc_cr2 + bic!
;

: setInCh ( adci_base channel n -- )
  dup 0 > if 
    dup case
      dup 7 < ?of
      endof
      dup 13 < ?of 
        6 -
      endof
      dup 17 < ?of 
        12 -
      endof
    endcase
    1- 5 * $1F over lshift -rot lshift swap rot
    dup case
      dup 7 < ?of
	adc_sqr3
      endof
      dup 13 < ?of 
	adc_sqr2
      endof
      dup 17 < ?of 
	adc_sqr1
      endof
    endcase
    + tuck bic! bis!
  then
;

: setInCh1 ( adci_base channel -- )
  dup 0 swap > if exit then
  dup 17 > if exit then
  swap adc_sqr3 + dup @ $1F bic rot or swap
  !
;

: testIn1
    adc1_base adc_sqr3 + dup @ $1f bic 1 or swap !
    convIn1
;

