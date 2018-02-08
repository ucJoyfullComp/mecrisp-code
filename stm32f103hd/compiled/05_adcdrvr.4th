compiletoflash

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


: initADC1 ( -- )
  setPrescalerToMaximum
  \ enable clock to ADC1 and reset it   
  $0200 rcc_apb2enr_r bis!
  100 0 do loop \ delay for device enable to take effect (not sure needed)
  \ reset ADC 1 in RCC unit
  $0200 rcc_apb2rstr_r bis!
  100 0 do loop \ wait a while for reset to take effect
  $0200 rcc_apb2rstr_r bic!
  \ init ADCi
  $00000000 adc1_base adc_cr1 + !
  $00000000 adc1_base adc_cr2 + !
  $00FFFFFF adc1_base adc_smpr1 + !
  $3FFFFFFF adc1_base adc_smpr2 + !
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
  $1 adc1_base adc_sqr3 + bis!
  $003FFFFF adc1_base adc_jsqr + bic!
  \ calibrate ADC1
  $1F adc1_base adc_sr + bic!
  $4 adc1_base adc_cr2 + >r r@ bis!
  begin
    $4 r@ bit@
  until
  r> drop
  \ set to single conversion mode
  \ set default conversion channels and sampling time per channel
  \ set data alignment
  turnADC1-On
; 

\ do one A/D conversion on channel 1 and place result on stack
: convertADC1-single ( -- u )
  turnADC1-On
  1 adc1_base adc_cr2 + bis!
  waitForEOC1
  adc1_base adc_dr + h@
  $1F adc1_base adc_sr + hbic!
;  

3315000 variable Vdda

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

\ do one A/D conversion and translate to voltage and then display in .1R6 format
: convIn1 convertADC1-single Vdda @ 4095 */ .1R6 ;

32 buffer: mynumber

\ do a manual callibration of the Vdda value and translate into Vdda variable
: calibrate-vdda
  depth >r
  ." enter the value of Vdda in uV ( 1V=1000000uV ) "
  mynumber 32 accept
  mynumber swap evaluate
  depth r> = not if Vdda ! then
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

\ do a calibration in ADC1
: calibrate-adc1
    $800000 adc1_base adc_cr2 + bis!
    1 ms
    adc1_base 4 16 setInSmp
    adc1_base 4 17 setInSmp
    17 adc1_base adc_sqr3 + $1F over bic! bis!
    1204000 4095
    convertADC1-single
    */ dup Vdda ! 
    ." Computed Vdda: " . cr
;

\ initialize the system for USART1: 115200,8N1
\ system clock 72MHz
\ initialize ADC1: 1 channel conversion, with conversion time of 1.666uSec
: init
  115200 BaudRate !
  8000000 9 init04
  initADC1
  calibrate-adc1
  1 1 initADCIns
  adc1_base 1 0 setInCh
  adc1_base 1 1 setInSmp
  1 setNumOfChans
;

compiletoram

init
