: initIns 
	\ set A0, A1, A2, A3, A4, A5, A6, A7 to analog input
	$FFFFFFFF gpioa_base gpio_crl + bic!
; 

: testEOC1 adc1_base adc_sr + @ 2 and 2 = ; inline

: clearSR1 $1F adc1_base adc_sr + ! ; inline

: testEOC2 adc2_base adc_sr + @ 2 and 2 = ; inline

: clearSR2 $1F adc2_base adc_sr + ! ; inline

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

