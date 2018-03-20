\ TIM6 - Time Sample generator
\ DMA Channel
\ Analog output example
\ PA4: DAC1 out
\ PA5: DAC2 out

1 29 lshift constant DACEN
1  4 lshift constant TIM6EN
1 21 lshift constant DMA1EN
1 22 lshift constant DMA2EN


64 constant DMABUFSIZE
DMABUFSIZE cells 2* buffer: dac2chbuf1
dac2chbuf1 DMABUFSIZE cells + constant dac2chbuf2

: dacdma-irq
	$400 DMA1_LISR_R bit@ if
		$400 DMA1_LIFCR_R bis!
	then
	$800 DMA1_LISR_R bit@ if
		$800 DMA1_LIFCR_R bis!
	then
;

: test-tim6-irq
	1 TIM6_BASE TIM1_SR + hbic!
	$8000 GPIOE_BASE GPIO_ODR + hxor!
;

: dma1s1-enable
	1 DMA1_S1CR_R bis!
;

: dma1s1-disable
	1 DMA1_S1CR_R bic!
;

: init-dmabufs
	DMABUFSIZE 0 do
		2048 16 lshift 2048 or dac2chbuf1 i cells + !
		2048 16 lshift 2048 or dac2chbuf2 i cells + !
	loop
;

: init-dmabufs-tri
	DMABUFSIZE 0 do
		2048 16 lshift i 4095 DMABUFSIZE 1- */ or
		dac2chbuf1 i cells + !
		2048 16 lshift DMABUFSIZE 1- i - 4095 DMABUFSIZE 1- */ or
		dac2chbuf2 i cells + !
	loop
;

: init-dmabufs-sawp
	DMABUFSIZE 0 do
		2048 16 lshift i 4095 DMABUFSIZE 1- */ or
		dac2chbuf1 i cells + !
		2048 16 lshift 2048 or
		dac2chbuf2 i cells + !
	loop
;

: init-dmabufs-sawn
	DMABUFSIZE 0 do
		2048 16 lshift 2048 or
		dac2chbuf1 i cells + !
		2048 16 lshift DMABUFSIZE 1- i - 4095 DMABUFSIZE 1- */  or
		dac2chbuf2 i cells + !
	loop
;

: init-dacdma
	DMA1EN RCC_AHB1ENR_R bis!
	10 0 do loop
	DMA1EN RCC_AHB1RSTR_R bis!
	10 0 do loop
	DMA1EN RCC_AHB1RSTR_R bic!
	10 0 do loop

	init-dmabufs

	\ configure DMA1 stream 1 (TIM6_UP)
	DMA1_S1CR_R @ $F1000000 and
	7 25 lshift or \ channel 7
	0 23 lshift or \ mburst transfer configuration
	0 21 lshift or \ pburst transfer configuration
	0 19 lshift or \ current target in double buffer mode
	1 18 lshift or \ double buffer mode
	3 16 lshift or \ Very high priority
	0 15 lshift or \ PINCOS
	2 13 lshift or \ 32bit memory data size
	2 11 lshift or \ 32bit peripheral data size
	1 10 lshift or \ increment memory after transfer
	0 9 lshift or \ do not increment peripheral address
	0 8 lshift or \ circular mode
	1 6 lshift or \ memory to periph transfer
	0 5 lshift or \ DMA is flow controller
	DMA1_S1CR_R !
	DMABUFSIZE DMA1_S1NDTR_R h! \ number of transfers in one buffer
	DAC_DHR12RD_R DMA1_S1PAR_R ! \ address of the peripheral
	dac2chbuf1 DMA1_S1M0AR_R ! \ address of first buffer
	dac2chbuf2 DMA1_S1M1AR_R ! \ address of second fuffer
	\ set dma1s1 interrupt handler
	['] dacdma-irq irq-dma1s1 !
	1 12 lshift NVIC_ISER0_R bis!
	\ enable interrupts
	DMA1_S1CR_R @ $1f bic
	%1100 2* or \ interrupt sources: TCIE, HTIE
	DMA1_S1CR_R !
;

: tim6-disable
	1 TIM6_BASE TIM1_CR1 + bic!
;

: tim6-enable
	1 TIM6_BASE TIM1_CR1 + bis!
;

: init-tim6
	TIM6EN RCC_APB1ENR_R bis!
	10 0 do loop
	TIM6EN RCC_APB1RSTR_R bis!
	10 0 do loop
	TIM6EN RCC_APB1RSTR_R bic!
	10 0 do loop

	$80 TIM6_BASE TIM1_CR1 + h!

\	['] test-tim6-irq irq-tim6dac !
\	1 22 lshift NVIC_ISER1_R bis!
	$100 TIM6_BASE TIM1_DIER + h!

	GetApb1Clock
	rcc_cfgr_r @ 12 rshift 1 and 1 = if
		2*
	then
	96000 / 1-
	TIM6_BASE TIM1_ARR + h!
;

: test-tim6
	\ init PE15 for io
	%01 15 2* lshift GPIOE_BASE GPIO_MODER +
	%11 15 2* lshift over bic! bis!
	1 15 lshift GPIOE_BASE GPIO_OTYPER + bic!
	%11 15 2* lshift GPIOE_BASE GPIO_OSPEEDER + bis!
	%11 15 2* lshift GPIOE_BASE GPIO_PUPDR + bic!
	%1 15 lshift GPIOE_BASE GPIO_ODR + bic!
	\ init TIM6
	init-tim6

	tim6-enable
	\ wait
	10000 ms
	tim6-disable
;

: dacs ( Channel1 Channel2 -- )
  \ $FFF and DAC_DHR12R2_R !
  \ $FFF and DAC_DHR12R1_R !
  $FFF and 16 lshift
  swap
  $FFF and
  or
  DAC_DHR12RD_R !
;

: init-dac ( -- )
  DACEN RCC_APB1ENR_R bis! \ Enable clock for DAC
  200 0 do loop

  %11 4 2* lshift
  %11 5 2* lshift or GPIOA_BASE GPIO_MODER + bis! \ Set DAC Pins to analog mode

  $00010001 DAC_CR_R ! \ Enable Channel 1 and 2
  2048 2048 dacs
;

: dac1 ( Channel1 -- ) \ PA4
  $FFF and DAC_DHR12R1_R !
;

: dac2 ( Channel2 -- ) \ PA5
  $FFF and DAC_DHR12R2_R !
;

: triangle ( n -- )
  init-dac

  dup 0 > if
    begin
      $1000 0 do  i          $FFF i - dacs dup 0 do loop loop
      $1000 0 do  $FFF i -   i        dacs dup 0 do loop loop
    key? until
  else
    begin
      $1000 0 do  i          $FFF i - dacs loop
      $1000 0 do  $FFF i -   i        dacs loop
    key? until
  then
  drop
;

: test-dacdma
	\ init PE15 for io
	%01 15 2* lshift GPIOE_BASE GPIO_MODER +
	%11 15 2* lshift over bic! bis!
	1 15 lshift GPIOE_BASE GPIO_OTYPER + bic!
	%11 15 2* lshift GPIOE_BASE GPIO_OSPEEDER + bis!
	%11 15 2* lshift GPIOE_BASE GPIO_PUPDR + bic!
	%1 15 lshift GPIOE_BASE GPIO_ODR + bic!
	\ init TIM6
	init-dac
	init-dacdma
	init-tim6

	init-dmabufs-tri
	dma1s1-enable
	tim6-enable
	\ wait
	5000 ms
	tim6-disable
	dma1s1-disable
;


