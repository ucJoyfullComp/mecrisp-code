DMA_BASE DMA_ISR + constant DMA1_ISR
DMA_BASE DMA_IFCR + constant DMA1_IFCR
DMA_BASE DMA_CCR1 + constant DMA1_CCR1
DMA_BASE DMA_CNDTR1 + constant DMA1_CNDTR1
DMA_BASE DMA_CPAR1 + constant DMA1_CPAR1 
DMA_BASE DMA_CMAR1 + constant DMA1_CMAR1 

2 constant hcell
: hcells 2* ;
: hcell+ 2 + ;

10 constant maxadcchs
maxadcchs hcells 2* buffer: adcdbuf
adcdbuf maxadcchs hcells + variable adchbuf

0 variable adcNchannels
0 variable adcLFlag
0 variable adcHFlag

: dmach1-adc-isr
    adcNchannels @ 0= if
	1 ADC2_BASE ADC_CR2 + bic!
	1 ADC1_BASE ADC_CR2 + bic!
	0 dup adcLFlag ! adcHFlag !
    else
	4 DMA1_ISR bit@ if
	    $80000000 adcNchannels @ or adcLFlag !
	    4 DMA1_IFCR bis!
	then
	2 DMA1_ISR bit@ if
	    $80000000 adcNchannels @ or adcHFlag !
	    2 DMA1_IFCR bis!
	then	
    then
    15 DMA1_IFCR bis!
;

: dmaadc-init
    \ update Interrupt vector
    ['] dmach1-adc-isr irq-dma1ch1 !
    \ reset DMA1
    1 RCC_AHBENR_R bis!
    20 0 do i drop loop
    \ init DMA1 Channel 1
    $2AA6 DMA1_CCR1 h!
    10 DMA1_CNDTR1 h!
    ADC1_BASE ADC_DR + DMA1_CPAR1 !
    adcdbuf DMA1_CMAR1 !
    \ enable DMA1 CH1 irq
    1 11 lshift NVIC_ISER0 bis!
    \ enable DMA1 CH1
    15 DMA1_IFCR bis!
    20 0 do loop
    dint
    1 DMA1_CCR1 bis!
    10 0 do loop
    15 DMA1_IFCR bis!
    eint
;    
    
\ push the ADC Clock period in picoseconds on the stack
: ADCClk->T ( -- N[picoseconds] )
  1000000 dup getADCClk */mod 
  swap getADCClk 2* > if
    1+ 
  then 
;

\ convert a time period in picoseconds to ADC clock half period counts
: T->ADCClk/2 ( T[picoseconds] -- N[number of half period units] )
  ADCClk->T 2/ /mod 
  swap 0= not if
    1+
  then
;

\ set pin type gpioport[pin] to state
: config-gpio-pin ( gpioport pin state -- )
  $F and 
  swap 
  dup 15 > if
      2drop drop
      exit 
  then 
  dup 8 < if 
      gpio_crl 
  else 
      8 - gpio_crh 
  then
  >r rot r> + >r 
  4 * $F over 
  lshift r@ bic! 
  lshift r> bis! 
;

: setSmp ( addr smp u -- )
  3 * >r 
  $7 swap over and swap 
  r@ lshift swap
  r> lshift swap
  rot swap over
  bic! bis!
;

\ set sampling time for n channel for ADCi with adci_base address
: setInSmp ( adci_base smp n -- )
  dup case
    dup 10 < ?of 
      rot adc_smpr2 + -rot
      setSmp
    endof
    dup 18 < ?of 
      10 -
      rot adc_smpr1 + -rot
      setSmp
    endof
  endcase
;

\ enable i gpio pins as analog inputs.
: initADCIns ( n1 ... ni i -- )
  dup 1 < if exit then
  0 do 
    case
      dup dup 8 < ?of gpioa_base swap 0 config-gpio-pin endof
      dup 10 < ?of gpiob_base swap 8 - 0 config-gpio-pin endof
      dup 16 < ?of gpioc_base swap 10 - 0 config-gpio-pin endof
      drop
    endcase
  loop
;

\ translate the sequence number ( in multi channel conversion ) to the relative
\ slot in the sqr register
: Seq-to-sqr
    dup 6 < if
    else dup 12 < if 
        6 -
    else dup 16 < if 
        12 -
    then
    then
    then
;

\ translate the sequence number ( in multi channel conversion ) to the relevant
\ sqr register
: Seq-to-Reg
    dup 6 < if
      drop
      adc_sqr3
    else
    dup 12 < if 
      drop
      adc_sqr2
    else
    dup 16 < if 
      drop
      adc_sqr1
    then
    then
    then
;

\ set the analog channel (channel: 0..17) to convert in a multiconversion 
\ sequence to sequence# n (0..15) in ADCi (adci_base address)
: setInCh ( adci_base channel n -- )
  dup 0 < if 
    2drop drop
  else
    dup 15 > if
      2drop drop
    else
      dup Seq-to-sqr
      5 * $1F over lshift >r 
      rot swap lshift swap
      Seq-to-Reg rot
      + r> over
      bic! bis!
    then
  then
;

\ set the number of conversions in a multi conversion sequence in ADC1
: setNumOfChans ( n -- )
  dup 0 > over 17 < and if
    1- 20 lshift 
    adc1_base adc_sqr1 + 
    dup $f 20 lshift swap bic!
    bis!
  else
    drop
  then
;

\ set the number of channel to be converted first
: setInCh1 ( adci_base channel -- )
  dup 0 swap > if exit then
  dup 17 > if exit then
  swap adc_sqr3 + dup @ $1F bic rot or swap
  !
;

\ return a flag of the End Of Conversion status if ADC# i
: testEOC ( adcI_base -- flag )
  2 swap adc_sr + bit@ ; inline

\ clear all the status flags of ADC# i
: clearSR ( adcI_base -- )
  $1F swap adc_sr + bic! ; inline

\ wait for End Of Conversion of ADC# i for 500 tests
: waitForEOC ( adcI_base -- )
  500 0 do
    dup testEOC if drop unloop  exit then
  loop
  drop
;

\ test EOC for ADC1 
: testEOC1 adc1_base testEOC ;

\ clear all status flags for ADC1
: clearSR1 adc1_base clearSR ;

\ wait for EOC of ADC1 for 500 tests
: waitForEOC1 adc1_base waitForEOC ;

\ test EOC for ADC2
: testEOC2 adc2_base testEOC ;

\ clear all status flags for ADC2
: clearSR2 adc2_base clearSR ;

\ wait for EOC of ADC2 for 500 tests
: waitForEOC2 adc2_base waitForEOC ;

1 constant ADON

\ set prescaler for all ADCs to maximum clock rate <14MHz
: setPrescalerToMaximum
  GetPClk2 14000000 /mod swap 
  0= not if 1+ then
  dup 0= if 1 then
  2/ 1- 14 lshift 
  $C000 rcc_cfgr_r bic! 
  rcc_cfgr_r bis!
;

\ turn ADC1 on and wait atleast 1uSec
: turnADC1-On ( -- )
  1 adc1_base adc_cr2 + bit@ not if
    1 adc1_base adc_cr2 + bis!
    getsysclock 1000000 /mod 
    swap 500000 > if 1+ then 
    0 do loop
  then
;

\ turn ADC2 on and wait atleast 1uSec
: turnADC2-On ( -- )
  1 adc2_base adc_cr2 + bit@ not if
    1 adc2_base adc_cr2 + bis!
    getsysclock 1000000 /mod 
    swap 500000 > if 1+ then 
    0 do loop
  then
;

: init-ADC1
  $00060100 adc1_base adc_cr1 + !
  $009A0000 adc1_base adc_cr2 + !
  $00FC0000 adc1_base adc_smpr1 + !
  $00000000 adc1_base adc_smpr2 + !
  $FFF adc1_base 
  2dup adc_jofr1 + hbic!
  2dup adc_jofr2 + hbic!
  2dup adc_jofr3 + hbic!
  2dup adc_jofr4 + hbic!
  2dup adc_htr + hbis!
  adc_ltr + hbic!
  $00FFFFFF adc1_base adc_sqr1 + bic!
  $3FFFFFFF adc1_base 
  2dup adc_sqr2 + bic!
  adc_sqr3 + bic!
  $003FFFFF adc1_base adc_jsqr + bic!
;

: init-ADC2
  $00060100 adc2_base adc_cr1 + !
  $00900000 adc2_base adc_cr2 + !
  $00000000 adc2_base adc_smpr1 + !
  $00000000 adc2_base adc_smpr2 + !
  $FFF adc2_base 
  2dup adc_jofr1 + hbic!
  2dup adc_jofr2 + hbic!
  2dup adc_jofr3 + hbic!
  2dup adc_jofr4 + hbic!
  2dup adc_htr + hbis!
  adc_ltr + hbic!
  $00FFFFFF adc2_base adc_sqr1 + bic!
  $3FFFFFFF adc2_base 
  2dup adc_sqr2 + bic!
  adc_sqr3 + bic!
  $003FFFFF adc2_base adc_jsqr + bic!
;

: calibrate-ADC1
  $1F adc1_base adc_sr + bic!
  $1 adc1_base adc_cr2 + bis!
  100 0 do loop
  $4 adc1_base adc_cr2 + >r r@ bis!
  r> 
  begin
    $4 over bit@
  until
  drop
;

: calibrate-ADC2
  $1F adc2_base adc_sr + bic!
  $1 adc2_base adc_cr2 + bis!
  100 0 do loop
  $4 adc2_base adc_cr2 + >r r@ bis!
  r>
  begin
    $4 over bit@
  until
  drop
;

: config-ADC1
  $100 adc1_base adc_cr2 + bis!
  \ set default conversion channels and sampling time per channel
  $00400000 adc1_base adc_sqr1 + !
  $00000000 adc1_base adc_sqr2 + !
  8 5 lshift
  6 or 5 lshift
  4 or 5 lshift
  2 or 5 lshift
  0 or
  adc1_base adc_sqr3 + !
;

: config-ADC2
  $100 adc2_base adc_cr2 + bis!
  \ set default conversion channels and sampling time per channel
  $00400000 adc2_base adc_sqr1 + !
  $00000000 adc2_base adc_sqr2 + !
  9 5 lshift
  7 or 5 lshift
  5 or 5 lshift
  3 or 5 lshift
  1 or
  adc2_base adc_sqr3 + !
;

: initADCs ( -- )
  dmaadc-init
  setPrescalerToMaximum
  \ enable clock to ADC1 and ADC2 and reset them
  $0600 rcc_apb2enr_r bis!
  100 0 do loop \ delay for device enable to take effect (not sure needed)
  \ reset ADC1 and ADC2 in RCC unit
  $0600 rcc_apb2rstr_r bis!
  100 0 do loop \ wait a while for reset to take effect
  $0600 rcc_apb2rstr_r bic!
  init-ADC1
  init-ADC2
  calibrate-ADC1
  calibrate-ADC2
  config-ADC1
  config-ADC2
  \ Set trigger source for conversion to tim4 oc4 
  GetSysClock 48000 / dup 1-
  4 set-tim-2to4-timebase
  2/ 1- 4 set-tim4-oc
  $40 tim4_base tim_cr2 + h!
  \ variables
  0 adcNchannels !
  \ enable ADC converters
; 

\ display the number on stack in #.###### format
: .1R6 
  1000000 /mod 
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

: enable-adc
  \ enable ADC converters
  turnADC2-On
  turnADC1-On 
;

: disable-adc
  1 adc2_base adc_cr2 + bic!
  1 adc1_base adc_cr2 + bic!
;

\ initialize ADC1,2: 10 channels, dma transfer, conversion time of 1.666uSec
: init
  init
  initADCs
;
