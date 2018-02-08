\ compiletoflash
\ ADC1_BASE ADC_SR + constant ADC1_SR_R
\ ADC1_BASE ADC_CR1 + constant ADC1_CR1_R
\ ADC1_BASE ADC_CR2 + constant ADC1_CR2_R
\ ADC1_BASE ADC_SMPR1 + constant ADC1_SMPR1_R
\ ADC1_BASE ADC_SMPR2 + constant ADC1_SMPR2_R
\ ADC1_BASE ADC_JOFR1 + constant ADC1_JOFR1_R
\ ADC1_BASE ADC_JOFR2 + constant ADC1_JOFR2_R
\ ADC1_BASE ADC_JOFR3 + constant ADC1_JOFR3_R
ADC1_BASE ADC_JOFR4 + constant ADC1_JOFR4_R
ADC1_BASE ADC_HTR + constant ADC1_HTR_R
ADC1_BASE ADC_LTR + constant ADC1_LTR_R
ADC1_BASE ADC_SQR1 + constant ADC1_SQR1_R
ADC1_BASE ADC_SQR2 + constant ADC1_SQR2_R
ADC1_BASE ADC_SQR3 + constant ADC1_SQR3_R
ADC1_BASE ADC_JSQR + constant ADC1_JSQR_R
ADC1_BASE ADC_JDR1 + constant ADC1_JDR1_R
ADC1_BASE ADC_JDR2 + constant ADC1_JDR2_R
ADC1_BASE ADC_JDR3 + constant ADC1_JDR3_R
ADC1_BASE ADC_JDR4 + constant ADC1_JDR4_R
ADC1_BASE ADC_DR + constant ADC1_DR_R
ADC2_BASE ADC_SR + constant ADC2_SR_R
ADC2_BASE ADC_CR1 + constant ADC2_CR1_R
ADC2_BASE ADC_CR2 + constant ADC2_CR2_R
ADC2_BASE ADC_SMPR1 + constant ADC2_SMPR1_R
ADC2_BASE ADC_SMPR2 + constant ADC2_SMPR2_R
ADC2_BASE ADC_JOFR1 + constant ADC2_JOFR1_R
ADC2_BASE ADC_JOFR2 + constant ADC2_JOFR2_R
ADC2_BASE ADC_JOFR3 + constant ADC2_JOFR3_R
ADC2_BASE ADC_JOFR4 + constant ADC2_JOFR4_R
ADC2_BASE ADC_HTR + constant ADC2_HTR_R
ADC2_BASE ADC_LTR + constant ADC2_LTR_R
ADC2_BASE ADC_SQR1 + constant ADC2_SQR1_R
ADC2_BASE ADC_SQR2 + constant ADC2_SQR2_R
ADC2_BASE ADC_SQR3 + constant ADC2_SQR3_R
ADC2_BASE ADC_JSQR + constant ADC2_JSQR_R
ADC2_BASE ADC_JDR1 + constant ADC2_JDR1_R
ADC2_BASE ADC_JDR2 + constant ADC2_JDR2_R
ADC2_BASE ADC_JDR3 + constant ADC2_JDR3_R
ADC2_BASE ADC_JDR4 + constant ADC2_JDR4_R
ADC2_BASE ADC_DR + constant ADC2_DR_R


: initIns 
	\ set A0, A1, A2, A3, A4, A5, A6, A7 to analog input
	$FFFFFFFF gpioa_base gpio_crl + bic!
; 

: testEOC1 adc1_sr_r + @ 2 and 2 = ; inline

: clearSR1 $1F adc1_sr_r + ! ; inline

: testEOC2 adc2_sr_r + @ 2 and 2 = ; inline

: clearSR2 $1F adc2_sr_r + ! ; inline

: initADCClockHSE
	$10000 rcc_cr_r bit@ not if 
		$10000 rcc_cr_r bis!
		begin $20000 rcc_cr_r bit@ until
		$C rcc_cfgr_r @ and 4 = not if 
			rcc_cfgr_r dup @ 3 bic 1 or swap !
			begin $C rcc_cfgr_r @ and 4 = until
		then
	then
;

1 constant ADON



: initADC1 ( insToSample -- )
	initADCClockHSE
	\ enable ADC 1
	$0200 rcc_apb2enr_r bis!
	100 0 do loop \ delay for device enable to take effect (not sure needed)
	\ reset ADC 1 in RCC unit
	$0200 rcc_apb2rstr_r bis!
	100 0 do loop \ wait a while for reset to take effect
	$0200 rcc_apb2rstr_r bic!
	\ init ADC 1
	\ calibrate ADC1
	\ set to single conversion mode
	\ set default conversion channels and sampling time per channel
	\ set data alignment
	
	ADON adc1_base adc_cr2 + bis! \ turn ADC1 on from powerdown
	
; 

: initADC2 ( insToSample -- )
	initADCClockHSE
	\ enable ADC 2
	$0400 rcc_apb2enr_r bis!
	100 0 do loop \ delay for device enable to take effect (not sure needed)
	\ reset ADC 2 in RCC unit
	$0400 rcc_apb2rstr_r bis!
	100 0 do loop \ wait a while for reset to take effect
	$0400 rcc_apb2rstr_r bic!
	\ init ADC 2
	
	ADON adc2_base adc_cr2 + bis! \ turn ADC2 on
; 

: readVref
	InitADC1
;	

\ compiletoram
