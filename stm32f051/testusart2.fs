115200 variable BaudRateUART2

84 constant circ-buf-size
circ-buf-size buffer: u2rx-buf
u2rx-buf variable u2rx-s-addr
u2rx-buf variable u2rx-e-addr
0 variable u2rx-buf-state
circ-buf-size buffer: u2tx-buf
u2tx-buf variable u2tx-s-addr
u2tx-buf variable u2tx-e-addr
0 variable u2tx-buf-state

: u2rx.get ( -- c | -1 )
    dint
    u2rx-buf-state @ 0 > if
      u2rx-s-addr @ dup c@ swap
      1+ dup u2rx-buf circ-buf-size + = if
        drop u2rx-buf
      then
      u2rx-s-addr !
      -1 u2rx-buf-state +!
      u2rx-buf-state @ 0= if
        u2rx-buf dup u2rx-s-addr !
        u2rx-e-addr !
      then
    else
      -1
    then
    eint
;

: u2rx.put ( c -- 0 | -1 )
    dint
    u2rx-buf-state @ circ-buf-size < if
      u2rx-e-addr @ swap over c!
      1+ dup u2rx-buf circ-buf-size + = if
        drop u2rx-buf
      then
      u2rx-e-addr !
      1 u2rx-buf-state +!
      0
    else
      -1
    then
    eint
;

: u2tx.get ( -- c | -1 )
    dint
    u2tx-buf-state @ 0 > if
      u2tx-s-addr @ dup c@ swap
      1+ dup u2tx-buf circ-buf-size + = if
        drop u2tx-buf
      then
      u2tx-s-addr !
      -1 u2tx-buf-state +!
      u2tx-buf-state @ 0= if
        u2tx-buf dup u2tx-s-addr !
        u2tx-e-addr !
      then
    else
      -1
    then
    eint
;

: u2tx.put ( c -- 0 | -1 )
    dint
    u2tx-buf-state @ circ-buf-size < if
      u2tx-e-addr @ swap over c!
      1+ dup u2tx-buf circ-buf-size + = if
        drop u2tx-buf
      then
      u2tx-e-addr !
      1 u2tx-buf-state +!
      0
    else
      -1
    then
    eint
;

: usart2.isr
    $8 USART2_ISR bit@ if \ ORE Over Run Error interrupt
      USART2_RDR h@ u2rx.put drop
      $8 USART2_ICR !
    then
    $4 USART2_ISR bit@ if \ NF Noise detected Error interrupt
      USART2_RDR h@ drop
      $4 USART2_ICR !
    then
    $2 USART2_ISR bit@ if \ FE Eraming Error interrupt
      $2 USART2_ICR !
    then  
    $1 USART2_ISR bit@ if \ PE Earity Error interrupt
      $1 USART2_ICR !
    then  
    $20 USART2_ISR bit@ if \ RXNE interrupt
      USART2_RDR h@ u2rx.put drop
    then
    $40 USART2_ISR bit@ if \ TC interrupt
      u2tx.get dup -1 = if
        drop
        $C0 USART2_CR1 bic!
        $40 USART2_ICR !
      else
        USART2_TDR !
      then
    then
    $80 USART2_ISR bit@ if \ TXE interrupt
      u2tx.get dup -1 = if
        drop
        $80 USART2_CR1 bic!
      else
        USART2_TDR !
      then
    then
;

: SetupUSART2 ( baudrate -- )
    \ calculate the value of BRR assume oversampling by 16
    GetApbClock swap /

    \ enable clock for the USART2 in RCC
    $00020000 RCC_APB1ENR bis!
    \ reset USART2 by sw from RCC
    $00020000 RCC_APB1RSTR bis!
    30 0 do i drop loop
    $00020000 RCC_APB1RSTR bic!
    30 0 do i drop loop
    \ assign I/O ports A2 - tx, A3 - rx
    %1010 2 2* lshift $000000F0 GPIOA_MODER @|+!
    %10 2 lshift $000C GPIOA_OTYPER @|+!
    %0000 2 2* lshift $000000F0 GPIOA_OSPEEDR @|+!
    %0000 2 2* lshift $000000F0 GPIOA_PUPDR @|+!
    %11 2 2* lshift $000C GPIOA_ODR @|+!
    $11 2 4 * lshift $0000FF00 GPIOA_AFRL @|+!
    \ init USART2 registers 8N1 no hw flow control
    USART2_BRR h!
    $0000002C USART2_CR1 !
    $00000000 USART2_CR2 !
    $00000001 USART2_CR3 !
    $8 USART2_RQR bis!
    $00121B2F USART2_ICR bis!
    \ install ISR and enable interrupt for USART2
    ['] usart2.isr irq-usart2 !
    $40 $FF NVIC_IPR7 tuck bic! bis!
    $10000000 NVIC_ISER bis!
    \ re-enable the usart2
    $00000001 USART2_CR1 bis!
;

\ enable transmit in interrupt mode using the u2tx buffer
: usart2.enTxInt ( -- )
    u2tx-buf-state @ 0= not if
      $000000C0 USART2_CR1 bis!
    then
;

compiletoram

\ code for debug purpose
: rx.inspect
    cr ." u2rx-buf :"
    u2rx-buf circ-buf-size dump
    cr ." u2rx-s-addr : $" u2rx-s-addr @ hex.
    cr ." u2rx-e-addr : $" u2rx-e-addr @ hex.
    cr ." u2rx-buf-state : " u2rx-buf-state @ .
    cr
;

: tx.inspect
    cr ." u2tx-buf :"
    u2tx-buf circ-buf-size dump
    cr ." u2tx-s-addr : $" u2tx-s-addr @ hex.
    cr ." u2tx-e-addr : $" u2tx-e-addr @ hex.
    cr ." u2tx-buf-state : " u2tx-buf-state @ .
    cr
;


\ some code to debug for general USART fixup on change of sysclock

: FixUSART2 ( baudrate -- )
    \ fix USART1 setup for new clock
    \ stop USART1
    USART2_CR1 @ swap
    $0001 USART2_CR1
    dint
    bic!
    eint
    \ calculate the value of BRR register
    GetApbClock ( baudrate -- baudrate ApbClock )
    2dup
    $8000 USART2_CR1 bit@
    if
    	\ oversample by 8
    	over 8 * /mod 2*
    	>r over 2/ + swap / r>
    	16 * +
    else
    	\ oversample by 16
    	over /mod -rot
    	2* < -1 * + 
    then
    \ we are now left with the value of USART_BRR on stack
    \ store into the register.
    USART2_BRR !
    \ re-enable the USART1
    USART2_RDR h@ drop
    $100A5F USART2_ICR !
    $0000000D USART2_CR1
    dint
    bis!
    eint
;

: UpdateUART2 ( -- )
    BaudRateUART2 @ dup case
    	2400 of FixUSART2 endof
    	4800 of FixUSART2 endof
    	7200 of FixUSART2 endof
    	9600 of FixUSART2 endof
    	14400 of FixUSART2 endof
    	19200 of FixUSART2 endof
    	38400 of FixUSART2 endof
    	56000 of FixUSART2 endof
    	57600 of FixUSART2 endof
    	115200 of FixUSART2 endof
    	128000 of FixUSART2 endof
    	230400 of FixUSART2 endof
    	256000 of FixUSART2 endof
    	460800 of FixUSART2 endof
    	921600 of FixUSART2 endof
    	cr . ." is not a standard BaudRate. not updating!"
    endcase
;
   

