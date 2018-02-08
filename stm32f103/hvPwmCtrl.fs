\ push the ADC Clock period in picoseconds on the stack
\ : ADCClk->T ( -- N[picoseconds] )

\ convert a time period in picoseconds to ADC clock half period counts
\ : T->ADCClk/2 ( T[picoseconds] -- N[number of half period units] )

\ set pin type gpioport[pin] to state
\ : config-gpio-pin ( gpioport pin state -- )

\ : setSmp ( addr smp u -- )

\ set sampling time for n channel for ADCi with adci_base address
\ : setInSmp ( adci_base smp n -- )

\ enable i gpio pins as analog inputs.
\ : initADCIns ( n1 ... ni i -- )

\ return a flag of the End Of Conversion status if ADC# i
\ : testEOC ( adcI_base -- flag )

\ clear all the status flags of ADC# i
\ : clearSR ( adcI_base -- )

\ wait for End Of Conversion of ADC# i for 500 tests
\ : waitForEOC ( adcI_base -- )

\ test EOC for ADC1 
\ : testEOC1 adc1_base testEOC ;

\ clear all status flags for ADC1
\ : clearSR1 adc1_base clearSR ;

\ wait for EOC of ADC1 for 500 tests
\ : waitForEOC1 adc1_base waitForEOC ;

\ test EOC for ADC2
\ : testEOC2 adc2_base testEOC ;

\ clear all status flags for ADC2
\ : clearSR2 adc2_base clearSR ;

\ wait for EOC of ADC2 for 500 tests
\ : waitForEOC2 adc2_base waitForEOC ;

\ 1 constant ADON

\ set prescaler for all ADCs to maximum clock rate <14MHz
\ : setPrescalerToMaximum

\ turn ADC1 on and wait atleast 1uSec
\ : turnADC1-On ( -- )

\ : initADC1 ( -- )

\ do one A/D conversion on channel 1 and place result on stack
\ : convertADC1-single ( -- u )

\ 3315000 variable Vdda

\ display the number on stack in #.###### format
\ : .1R6 

\ do one A/D conversion and translate to voltage and then display in .1R6 format
\ : convIn1 convertADC1-single Vdda @ 4095 */ .1R6 ;

\ do a manual callibration of the Vdda value and translate into Vdda variable
\ : calibrate-vdda

\ translate the sequence number ( in multi channel conversion ) to the relative
\ slot in the sqr register
\ : Seq-to-sqr

\ translate the sequence number ( in multi channel conversion ) to the relevant
\ sqr register
\ : Seq-to-Reg

\ set the analog channel (channel: 0..17) to convert in a multiconversion 
\ sequence to sequence# n (0..15) in ADCi (adci_base address)
\ : setInCh ( adci_base channel n -- )

\ set the number of conversions in a multi conversion sequence in ADC1
\ : setNumOfChans ( n -- )

\ set the number of channel to be converted first
\ : setInCh1 ( adci_base channel -- )

\ do a calibration in ADC1
\ : calibrate-adc1

\ return true if min <= n <= max
: between ( n min max -- flag )
	>r over 1+ < swap r> 1+ < and
;

\ return true if n between 0 and 15
: valid-pin? ( n -- flag )
	0 15 between
;

\ return base address of timer registers according to timer number
: tim-base-addr-2to4 ( n -- a-addr flag )
	case
	2 of tim2_base true endof
	3 of tim3_base true endof
	4 of tim4_base true endof
	>r 0 false r>
	endcase
;

\ set timebase of timer n to ticks
: set-tim-2to4-timebase ( ticks n -- )
	dup 2 4 between if 
		1 over 2 - lshift rcc_apb1enr_r bis!
	else
		2drop exit
	then
	20 0 do i drop loop
	tim-base-addr-2to4 if >r else drop exit then
	0 r@ tim_cr1 + h!
	0 r@ tim_cr2 + h!
	0 r@ tim_smcr + h!
	0 r@ tim_dier + h!
	$1e5f r@ tim_sr + hbic!
	r@ tim_arr + h! 
	1 r> tim_cr1 + hbis!
;

\ set pin PB(n) to Alternate function push-pull
: init-pb-n ( n -- ) 
	dup valid-pin? if
		dup 8 < if
			4 * 
			dup $b swap lshift swap $f swap lshift 
			gpiob_base gpio_crl + dup >r @ swap bic or r> !
		else
			8 - 4 *
			dup $b swap lshift swap $f swap lshift 
			gpiob_base gpio_crh + dup >r @ swap bic or r> !
		then
	else ." pin must be between 0..15"
	then
;

\ set TIM4 Output compare oc# to PWM1 mode and value of clocks
: set-tim4-oc ( clocks oc# -- )
	case
	1 of
		$60 tim4_base tim_ccmr1 + dup $ff swap hbic! hbis!
		$1 tim4_base tim_ccer + dup $3 swap hbic! hbis!
		tim4_base tim_ccr1 + h! 
	endof
	2 of 
		$6000 tim4_base tim_ccmr1 + dup $ff00 swap hbic! hbis!
		$10 tim4_base tim_ccer + dup $30 swap hbic! hbis!
		tim4_base tim_ccr2 + h! 
	endof
	3 of 
		$60 tim4_base tim_ccmr2 + dup $ff swap hbic! hbis!
		$100 tim4_base tim_ccer + dup $300 swap hbic! hbis!
		tim4_base tim_ccr3 + h! 
	endof
	4 of 
		$6000 tim4_base tim_ccmr2 + dup $ff00 swap hbic! hbis!
		$1000 tim4_base tim_ccer + dup $3000 swap hbic! hbis!
		tim4_base tim_ccr4 + h! 
	endof
	2drop exit
	endcase 
;

\ generate the closest frequency possible on pin PB(6) symmetrical
: genFreqInPB6 ( freq -- )
	GetSysClock swap / dup 1-
	4 set-tim-2to4-timebase
	6 init-pb-n
	2/ 1- 1 set-tim4-oc
;

\ alias of genFreqInPB6
: gf genFreqInPB6 ;

tim4_base tim_arr + constant tim4_arr_r
tim4_base tim_ccr1 + constant tim4_ccr1_r
tim4_base tim_ccr1 + constant tim4_ccr2_r

\ display the number on stack in #.###### format
: .4R3 
  1000 /mod 
  <# [char] . hold dup s>d dabs #S rot sign #> type
  <# 0 over
  case
    dup base @ < ?of # 
      5 0 do 0 .digit hold loop endof
    dup base @ dup * < ?of #S 
      4 0 do 0 .digit hold loop endof
    dup base @ dup dup * * < ?of #S 
      3 0 do 0 .digit hold loop endof
    dup base @ dup * dup * < ?of #S 
      2 0 do 0 .digit hold loop endof
    dup base @ dup dup * dup * * < ?of #S 
      0 .digit hold endof
    dup base @ dup * dup dup * * < ?of #S endof
  endcase
  #> type space
;

\ generate an exponential sweep from 1KHz <= frequency < 1MHz in pin PB(6)
: testFreq
    10000000 1000 do
	i genFreqInPB6
	10 ms
	i 1100 1000 */ i -
    +loop
    10000 genFreqInPB6
;

\ generate a linear PWM sweep in pin PB(6) [0:10:ARR(TIM4)]
: testPwm
  10000 genFreqInPB6
  GetSysClock 10000 /
  0 do
    i 1 set-tim4-oc
    10 ms
  10 +loop
  GetSysClock 10000 / 2/ 1- 1 set-tim4-oc
;

\ read analog value from pin PA(0) an PA(1)
: readValAin
    0 1 2 initADCIns
    adc1_base 1 1 setInSmp
    1 setNumOfChans
    10 0 do
	adc1_base 0 0 setInCh
	cr convIn1
	adc1_base 1 0 setInCh
	convertADC1-single Vdda @ 468 * 40950 */ .1R6
	100 ms
    loop
;

\ read analog value from pin PA(0) and PA(1)
\  and if 1 is larger than 0 turn PWM off
: compA1toA0andpwm
    0 1 set-tim4-oc
    GetSysClock 60000 / dup 1-
    4 set-tim-2to4-timebase
    6 init-pb-n
    0 1 2 initADCIns
    adc1_base 1 1 setInSmp
    1 setNumOfChans
    begin
	adc1_base 0 0 setInCh
	convertADC1-single 1+
	adc1_base 1 0 setInCh
	convertADC1-single
	< if
	    0 1 set-tim4-oc
	else
	    400 1 set-tim4-oc
	then
	1 ms
    again
;


