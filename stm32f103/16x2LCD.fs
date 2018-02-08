\ PA0-7 - D0-7, PB0 - Enable, PB1 - RS, PB10 - R/Wnot
GPIOA_BASE GPIO_ODR + constant A-ODR
GPIOA_BASE GPIO_BSRR + constant A-BSR
GPIOA_BASE GPIO_BRR + constant A-BRR
GPIOB_BASE GPIO_ODR + constant B-ODR
GPIOB_BASE GPIO_BSRR + constant B-BSR
GPIOB_BASE GPIO_BRR + constant B-BRR
TIM4_BASE TIM_CCR1 + constant TIM4_CCR1

2 variable rows
16 variable cols

: setup-Vo
    $00000004 RCC_APB1ENR_R bis!
    $64 TIM4_BASE TIM_CCMR1 + h!
    $1 TIM4_BASE TIM_CCER + h!
    0 TIM4_BASE TIM_CNT + h!
    GetSysClock 10000 / 1- TIM4_BASE TIM_ARR + h!
    TIM4_BASE TIM_ARR + h@ 2/ TIM4_BASE TIM_CCR1 + h!
    $1 TIM4_BASE TIM_CR1 + h!
    $B 6 4 * lshift GPIOB_BASE GPIO_CRL + $F 6 4 * lshift over bic! bis!
;

: setup-8bit
    %11111111 A-BRR !
    $11111111 GPIOA_BASE GPIO_CRL + !
    %10000000011 B-BRR h!
    $33 GPIOB_BASE GPIO_CRL + tuck @ $ff bic or swap !
    $300 GPIOB_BASE GPIO_CRH + tuck @ $f00 bic or swap !
    setup-Vo
;

: Flash-E
    %1 B-BSR !
    %1 B-BRR !
;

: Cmd
    %10 B-BRR !
;

: Data
    %10 B-BSR !
;

: RD
    $400 B-BSR !
;

: WR
    $400 B-BRR !
;

: Write-Cmd ( cmd -- )
    $FF and Cmd A-ODR @ $FF bic or A-ODR !
    Flash-E
;

: Write-Data ( data-c -- )
    $FF and Data A-ODR @ $FF bic or A-ODR !
    Flash-E
;

: Read-Status ( -- c )
    RD Cmd
    %1 B-BSR !
    GPIOA_BASE GPIO_IDR + h@ $FF and
    %1 B-BRR !
    WR
;

: setup-LCD
    setup-8bit
    50 ms
    $30 Write-Cmd
    5 ms
    $30 Write-Cmd
    200 us
    $30 Write-Cmd
    50 us
    $38 Write-Cmd
    50 us
    $08 $04 or Write-Cmd
    50 us
    $01 Write-Cmd
    50 us
    $04 Write-Cmd
    50 us
;

: Write-Char ( c -- )
    Write-Data
    begin
	Read-Status $80 and 0=
    until
;

: testCont
    100 0 do
	i
	TIM4_BASE TIM_ARR + h@
	100 */
	TIM4_BASE TIM_CCR1 + h!
	1000 ms
    10 +loop
;

: test01
    GPIOA_BASE GPIO_CRL + dup @ $f bic 1 or swap !
    GPIOA_BASE dup GPIO_BRR + swap GPIO_BSRR +
    10000 0 do
	2dup
	1 swap h! 1 ms
	1 swap h! 1 ms
    loop
;

: test02
    58 48 do i Write-Char 10 ms  loop
;

