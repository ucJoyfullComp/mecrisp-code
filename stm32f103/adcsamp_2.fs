\ compiletoflash

\ ADC1_BASE ADC_SR + constant ADC1_SR_R
\ ADC1_BASE ADC_CR1 + constant ADC1_CR1_R
\ ADC1_BASE ADC_CR2 + constant ADC1_CR2_R
\ ADC1_BASE ADC_SMPR1 + constant ADC1_SMPR1_R
\ ADC1_BASE ADC_SMPR2 + constant ADC1_SMPR2_R
\ ADC1_BASE ADC_JOFR1 + constant ADC1_JOFR1_R
\ ADC1_BASE ADC_JOFR2 + constant ADC1_JOFR2_R
\ ADC1_BASE ADC_JOFR3 + constant ADC1_JOFR3_R
\ ADC1_BASE ADC_JOFR4 + constant ADC1_JOFR4_R
\ ADC1_BASE ADC_HTR + constant ADC1_HTR_R
\ ADC1_BASE ADC_LTR + constant ADC1_LTR_R
\ ADC1_BASE ADC_SQR1 + constant ADC1_SQR1_R
\ ADC1_BASE ADC_SQR2 + constant ADC1_SQR2_R
\ ADC1_BASE ADC_SQR3 + constant ADC1_SQR3_R
\ ADC1_BASE ADC_JSQR + constant ADC1_JSQR_R
\ ADC1_BASE ADC_JDR1 + constant ADC1_JDR1_R
\ ADC1_BASE ADC_JDR2 + constant ADC1_JDR2_R
\ ADC1_BASE ADC_JDR3 + constant ADC1_JDR3_R
\ ADC1_BASE ADC_JDR4 + constant ADC1_JDR4_R
\ ADC1_BASE ADC_DR + constant ADC1_DR_R
\ ADC2_BASE ADC_SR + constant ADC2_SR_R
\ ADC2_BASE ADC_CR1 + constant ADC2_CR1_R
\ ADC2_BASE ADC_CR2 + constant ADC2_CR2_R
\ ADC2_BASE ADC_SMPR1 + constant ADC2_SMPR1_R
\ ADC2_BASE ADC_SMPR2 + constant ADC2_SMPR2_R
\ ADC2_BASE ADC_JOFR1 + constant ADC2_JOFR1_R
\ ADC2_BASE ADC_JOFR2 + constant ADC2_JOFR2_R
\ ADC2_BASE ADC_JOFR3 + constant ADC2_JOFR3_R
\ ADC2_BASE ADC_JOFR4 + constant ADC2_JOFR4_R
\ ADC2_BASE ADC_HTR + constant ADC2_HTR_R
\ ADC2_BASE ADC_LTR + constant ADC2_LTR_R
\ ADC2_BASE ADC_SQR1 + constant ADC2_SQR1_R
\ ADC2_BASE ADC_SQR2 + constant ADC2_SQR2_R
\ ADC2_BASE ADC_SQR3 + constant ADC2_SQR3_R
\ ADC2_BASE ADC_JSQR + constant ADC2_JSQR_R
\ ADC2_BASE ADC_JDR1 + constant ADC2_JDR1_R
\ ADC2_BASE ADC_JDR2 + constant ADC2_JDR2_R
\ ADC2_BASE ADC_JDR3 + constant ADC2_JDR3_R
\ ADC2_BASE ADC_JDR4 + constant ADC2_JDR4_R
\ ADC2_BASE ADC_DR + constant ADC2_DR_R
\ ADC3_BASE ADC_SR + constant ADC3_SR_R
\ ADC3_BASE ADC_CR1 + constant ADC3_CR1_R
\ ADC3_BASE ADC_CR2 + constant ADC3_CR2_R
\ ADC3_BASE ADC_SMPR1 + constant ADC3_SMPR1_R
\ ADC3_BASE ADC_SMPR2 + constant ADC3_SMPR2_R
\ ADC3_BASE ADC_JOFR1 + constant ADC3_JOFR1_R
\ ADC3_BASE ADC_JOFR2 + constant ADC3_JOFR2_R
\ ADC3_BASE ADC_JOFR3 + constant ADC3_JOFR3_R
\ ADC3_BASE ADC_JOFR4 + constant ADC3_JOFR4_R
\ ADC3_BASE ADC_HTR + constant ADC3_HTR_R
\ ADC3_BASE ADC_LTR + constant ADC3_LTR_R
\ ADC3_BASE ADC_SQR1 + constant ADC3_SQR1_R
\ ADC3_BASE ADC_SQR2 + constant ADC3_SQR2_R
\ ADC3_BASE ADC_SQR3 + constant ADC3_SQR3_R
\ ADC3_BASE ADC_JSQR + constant ADC3_JSQR_R
\ ADC3_BASE ADC_JDR1 + constant ADC3_JDR1_R
\ ADC3_BASE ADC_JDR2 + constant ADC3_JDR2_R
\ ADC3_BASE ADC_JDR3 + constant ADC3_JDR3_R
\ ADC3_BASE ADC_JDR4 + constant ADC3_JDR4_R
\ ADC3_BASE ADC_DR + constant ADC3_DR_R

: config-gpio-pin ( gpioport pin state -- )
  $F and 
  swap 
  dup 15 > if 
      exit 
  then 
  dup 8 < if 
      gpio_crl 
  else 
      8 - gpio_crh 
  then 
  rot + >r ( S: gpioport state npin gpiocfgreg -- state npin R: -- gpiocfgr-addr )
  4 * $F over ( state npin -- state shftN mask shftN )
  lshift r@ bic! ( state shftN mask shftN -- state shftN R: gpiocfgr-addr )
  lshift r> bis! ( state shftN R: gpiocfgr-addr -- )
;

: initADCIns ( n1 ... ni i -- )
	dup 1 < if exit then
	0 do 
	  case
	    8 < ?of gpioa_base swap 0 config-gpio-bin endof
	    10 < ?of gpiob_base swap 0 config-gpio-bin endof
	    gpioc_base swap 0 config-gpio-bin
	  endcase
	loop
; 

: testEOC ( adcI_base -- flag )
  2 swap adc_sr + bit@ ; inline

: clearSR ( adcI_base -- )
  $1F swap adc_sr + bic! ; inline

: waitForEOC ( adcI_base -- )
  begin
    dup testEOC not
  until
  drop
;

: testEOC1 adc1_base testEOC ;

: clearSR1 adc1_base clearSR ;

: waitForEOC1 adc1_base waitForEOC ;

: testEOC2 adc2_base testEOC ;

: clearSR2 adc2_base clearSR ;

: waitForEOC2 adc2_base waitForEOC ;

1 constant ADON

: setPrescalerToMaximum
  \ set prescaler for all ADCs to maximum clock rate <14MHz
  GetPClk2 14000000 /mod swap 
  0= not if 1+ then
  dup 0= if 1 then
  2* 1- 14 lshift 
  $C000 rcc_cfgr_r bic! 
  rcc_cfgr_r bis!
;

: initADCi ( n1 ... ni inChans adcI -- )
  	\ enable clock to ADCi and reset it   
  	case
	  1 of 
	      adc1_base 
	      $0200 rcc_apb2enr_r bis!
	      100 0 do loop \ delay for device enable to take effect (not sure needed)
	      \ reset ADC 1 in RCC unit
	      $0200 rcc_apb2rstr_r bis!
	      100 0 do loop \ wait a while for reset to take effect
	      $0200 rcc_apb2rstr_r bic!
	    endof
	  2 of 
	      $0400 rcc_apb2enr_r bis!
	      100 0 do loop \ delay for device enable to take effect (not sure needed)
	      \ reset ADC 2 in RCC unit
	      $0400 rcc_apb2rstr_r bis!
	      100 0 do loop \ wait a while for reset to take effect
	      $0400 rcc_apb2rstr_r bic!
	      adc2_base
	    endof
	  3 of 
	      $8000 rcc_apb2enr_r bis!
	      100 0 do loop \ delay for device enable to take effect (not sure needed)
	      \ reset ADC 3 in RCC unit
	      $8000 rcc_apb2rstr_r bis!
	      100 0 do loop \ wait a while for reset to take effect
	      $8000 rcc_apb2rstr_r bic!
	      adc3_base 
	    endof
	  dup 0 > if
	    0 do
	      drop
	    loop
	  then
	  exit
	endcase
	\ init ADCi
	\ calibrate ADC1
	\ set to single conversion mode
	\ set default conversion channels and sampling time per channel
	\ set data alignment
	
	ADON adc1_base adc_cr2 + bis! \ turn ADC1 on from powerdown
	
; 

: readVref
	initADC1
;	

\ compiletoram
