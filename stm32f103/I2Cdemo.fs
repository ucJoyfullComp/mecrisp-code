\ PB6:SCL, PB7:SDA - I2C1 / PB8, PB9 alternate
\ PB10:SCL, PB11:SDA - I2C2

I2C1_BASE I2C_CR1 + constant I2C1_CR1
I2C1_BASE I2C_DR + constant I2C1_DR
I2C1_BASE I2C_SR1 + constant I2C1_SR1
I2C1_BASE I2C_SR2 + constant I2C1_SR2

: i2c1.setup.pins
    %1001 RCC_APB2ENR_R bis!
    100 0 do loop
    
    $2 AFIO_BASE AFIO_MAPR + bic!
    $EE 6 4 * lshift GPIOB_BASE GPIO_CRL +
    $FF 6 4 * lshift over bic! bis!
;

: i2c1.setup.port
    1 21 lshift RCC_APB1ENR_R bis!
    100 0 do loop
    1 21 lshift RCC_APB1RSTR_R bis!
    100 0 do loop
    1 21 lshift RCC_APB1RSTR_R bic!
    100 0 do loop

    $0001 I2C1_BASE I2C_CR1 + bis!
    $0024 I2C1_BASE I2C_CR2 + !
    66 2* I2C1_BASE I2C_OAR1 + !
    0 I2C1_BASE I2C_OAR2 + !
    GetPclk1  200000 / I2C1_BASE I2C_CCR + !
    GetPclk1 1000000 / 1+ I2C1_BASE I2C_TRISE + !
;


: i2c1.setup
    i2c1.setup.pins
    i2c1.setup.port
;

: i2c1@? ( -- status-u )
    I2C1_SR1 @ I2C1_SR2 @ 16 lshift or
;

: i2c1! ( c -- )
    $FF and I2C1_DR !
;    
    
: i2c1@ ( -- c )
    I2C1_DR c@
;    
    
: i2c1[[ ( -- )
    $100 I2C1_CR1 bis!
    begin
	i2c1@? 1 and 1 =
    until
;

: i2c1[ ( -- )
    $100 I2C1_CR1 bis!
;

: i2c1] ( -- )
    $100 I2C1_CR1 bic!
;

: i2c1]] ( -- )
    $200 I2C1_CR1 bis!
;

: i2c1.emit ( i2c-addr char -- )
    depth 2 < if
	cr ." error: not enough parameters." cr .s exit
    then
    I2C1_CR1 @ 1 and 0= if
	\ interface i2c1 is not on - possibly not setup! exit.
	2drop exit
    then
    i2c1@? $20000 tuck and = if
	\ i2c1 bus is Busy! exit.
	2drop exit
    then
    $100 I2C1_CR1 bis!
    begin
	I2C1_SR1 @
	dup $0F00 and 0= not if
	    cr ." Error! flags=" hex. ." during START send"
	    drop exit
	then
	1 and 1 =
    until
    swap $FF and 2* I2C1_DR c!
    begin
	I2C1_SR1 @
	dup $0F00 and 0= not if
	    cr ." Error! flags=" hex. ." during address send"
	    drop exit
	then
	2 and 2 =
    until
    I2C1_SR2 @ drop
    begin
	I2C1_SR1 @
	dup $0F00 and 0= not if
	    cr ." Error! flags=" hex. ." send byte in addr="
	    drop exit
	then
	$80 and $80 =
    until
    I2C1_DR c!
    begin
	I2C1_SR1 @
	dup $0F00 and 0= not if
	    cr ." Error! flags=" hex. ." during Last byte sent" exit
	    exit
	then
	$84 and $84 =
    until
    $200 I2C1_CR1 bis!
;

: i2c1.getchar ( i2c-addr -- c )
    depth 1 < if
	cr ." error: not enough parameters." cr .s exit
    then
    I2C1_CR1 @ 1 and 0= if
	\ interface i2c1 is not on - possibly not setup! exit.
	drop exit
    then
    i2c1@? $20000 tuck and = if
	\ i2c1 bus is Busy! exit.
	drop exit
    then
    $100 I2C1_CR1 bis!
    begin
	I2C1_SR1 @
	dup $0F00 and 0= not if
	    cr ." Error! flags=" hex. ." during START send"
	    drop exit
	then
	1 and 1 =
    until
    2* $FF and 1 or I2C1_DR c!
    begin
	I2C1_SR1 @
	dup $0F00 and 0= not if
	    cr ." Error! flags=" hex. ." during address send"
	    drop exit
	then
	2 and 2 =
    until
    I2C1_SR2 @ drop
    $400 I2C1_CR1 bic!
    $200 I2C1_CR1 bis!
    begin
	I2C1_SR1 @
	dup $0F00 and 0= not if
	    cr ." Error! flags=" hex. ." send byte in addr="
	    drop exit
	then
	$40 and $40 =
    until
    I2C1_DR c@
;

: i2c1.type ( i2caddr c-addr len -- )
    depth 3 < if
	cr ." error: not enough parameters." cr .s exit
    then
    I2C1_CR1 @ 1 and 0= if
	\ interface i2c1 is not on - possibly not setup! exit.
	drop 2drop exit
    then
    i2c1@? $20000 tuck and = if
	\ i2c1 bus is Busy! exit.
	drop 2drop exit
    then
    $100 I2C1_CR1 bis!
    begin
	I2C1_SR1 @
	dup $0F00 and 0= not if
	    cr ." Error! flags=" hex. ." during START send"
	    2drop exit
	then
	1 and 1 =
    until
    rot $FF and 2* I2C1_DR c!
    begin
	I2C1_SR1 @
	dup $0F00 and 0= not if
	    cr ." Error! flags=" hex. ." during address send"
	    2drop exit
	then
	2 and 2 =
    until
    I2C1_SR2 @ drop
    0 do
	begin
	    I2C1_SR1 @
	    dup $0F00 and 0= not if
		cr ." Error! flags=" hex. ." send byte in addr="
		drop hex. exit
	    then
	    $80 and $80 =
	until
	dup c@ I2C1_DR c!
	1+
    loop
    drop
    begin
	I2C1_SR1 @
	dup $0F00 and 0= not if
	    cr ." Error! flags=" hex. ." during Last byte sent"
	    exit
	then
	$84 and $84 =
    until
    $200 I2C1_CR1 bis!    
;

: i2c1.accept ( i2c-addr c-addr size -- c-addr len )
    depth 3 < if
	cr ." error: not enough parameters." cr .s exit
    then
    I2C1_CR1 @ 1 and 0= if
	\ interface i2c1 is not on - possibly not setup! exit.
	drop 2drop exit
    then
    i2c1@? $20000 tuck and = if
	\ i2c1 bus is Busy! exit.
	drop 2drop exit
    then
    $100 I2C1_CR1 bis!
    begin
	I2C1_SR1 @
	dup $0F00 and 0= not if
	    cr ." Error! flags=" hex. ." during START send"
	    drop 2drop exit
	then
	1 and 1 =
    until
    rot 2* $FF and 1 or I2C1_DR c!
    begin
	I2C1_SR1 @
	dup $0F00 and 0= not if
	    cr ." Error! flags=" hex. ." during address send"
	    drop exit
	then
	2 and 2 =
    until
    I2C1_SR2 @ drop
    2dup 1- 0 do
	begin
	    I2C1_SR1 @
	    dup $0F00 and 0= not if
		cr ." Error! flags=" hex. ." send byte in addr="
		drop exit
	    then
	    $40 and $40 =
	until
	I2C1_DR c@ tuck c! 1+
    loop
    $400 I2C1_CR1 bic!
    $200 I2C1_CR1 bis!
    begin
	I2C1_SR1 @
	dup $0F00 and 0= not if
	    cr ." Error! flags=" hex. ." send byte in addr="
	    drop exit
	then
	$40 and $40 =
    until
    I2C1_DR c@ swap c!
;

: mymsg s" Hello World!" ;

\ input message up to 64 chars in length into mymsg buffer
\  - leaves the address and length of the message on stack
: !msg ( c-addr size -- c-addr len )
    begin key? dup if key drop then not until
    tuck 0 do
	key case
	    10 of drop swap drop i exit endof
	    13 of drop swap drop i exit endof
	    over i + c!
	endcase
    loop
    swap
;

: test01 ( -- )
    I2C1_CR1 @ 1 and 0= if
	i2c1.setup
    then
    $27 mymsg i2c1.type
;

: test02 ( -- )
    I2C1_CR1 @ 1 and 0= if
	i2c1.setup
    then
    $27 $FF i2c1.emit
    $FF GPIOA_BASE GPIO_ODR + bis!
    20 us
\    $66666666 GPIOA_BASE GPIO_CRL + !
    $22222222 GPIOA_BASE GPIO_CRL + !
    $55 GPIOA_BASE GPIO_ODR + tuck h@ $FF00 and or swap h!
    20 us
    $27 i2c1.getchar cr hex.
    $AA GPIOA_BASE GPIO_ODR + tuck h@ $FF00 and or swap h!
    20 us
    $27 i2c1.getchar cr hex.
;    

