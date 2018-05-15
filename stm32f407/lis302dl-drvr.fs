\ LIS302DL driver
\  Registers
$0F constant LIS302DL_WHO_AM_I
$20 constant LIS302DL_CTRL_REG1
$21 constant LIS302DL_CTRL_REG2
$22 constant LIS302DL_CTRL_REG3
$23 constant LIS302DL_HP_FILTER_RESET_REG
$27 constant LIS302DL_STATUS_REG
$29 constant LIS302DL_OUT_X
$2B constant LIS302DL_OUT_Y
$2D constant LIS302DL_OUT_Z
$30 constant LIS302DL_FF_WU_CFG1_REG
$31 constant LIS302DL_FF_WU_SRC1_REG
$32 constant LIS302DL_FF_WU_THS1_REG
$33 constant LIS302DL_FF_WU_DURATION1_REG
$34 constant LIS302DL_FF_WU_CFG2_REG
$35 constant LIS302DL_FF_WU_SRC2_REG
$36 constant LIS302DL_FF_WU_THS2_REG
$37 constant LIS302DL_FF_WU_DURATION2_REG
$38 constant LIS302DL_CLICK_CFG_REG
$39 constant LIS302DL_CLICK_SRC_REG
$3B constant LIS302DL_CLICK_THSY_X_REG
$3C constant LIS302DL_CLICK_THSZ_REG
$3D constant LIS302DL_CLICK_TIMELIMIT_REG
$3E constant LIS302DL_CLICK_LATENCY_REG
$3F constant LIS302DL_CLICK_WINDOW_REG

GPIOE_BASE constant LIS302_CS_PORT
3 constant LIS302_CS_PIN

SPI1_CR1_R constant SPI1_CR1
SPI1_CR2_R constant SPI1_CR2
SPI1_SR_R constant SPI1_SR
SPI1_DR_R constant SPI1_DR

: between ( val low high -- flag )
	>r over <= swap r> <= and
;

0 variable set-port-pin-mode-addr
0 variable set-port-pin-mode-pin
16 buffer: spi-buf

: set-port-pin-mode ( mode type speed pupd out af pin portaddr -- )
	depth 8 < if
		." error. not enough parameters. 8 are needed" cr
		exit
	then
	set-port-pin-mode-addr !
	set-port-pin-mode-pin !
	dup 0 15 between if
		set-port-pin-mode-pin @ 8 < if
			15 set-port-pin-mode-pin @ 4 * lshift
			set-port-pin-mode-addr @ GPIO_AFRL + bic!
			set-port-pin-mode-pin @ 4 * lshift
			set-port-pin-mode-addr @ GPIO_AFRL + bis!
		else
			15 set-port-pin-mode-pin @ 4 * lshift
			set-port-pin-mode-addr @ GPIO_AFRH + bic!
			set-port-pin-mode-pin @ 4 * lshift
			set-port-pin-mode-addr @ GPIO_AFRH + bis!
		then
	else
		." error. Alternate function out of range." cr
		2drop 2drop 2drop exit
	then
	dup 0 1 between if
		0= if
			1 set-port-pin-mode-pin @ lshift
			set-port-pin-mode-addr @ GPIO_ODR + bic!
		else
			1 set-port-pin-mode-pin @ lshift
			set-port-pin-mode-addr @ GPIO_ODR + bis!
		then
	else
		." error. Output state not 0 or 1." cr
		drop 2drop 2drop exit
	then
	dup 0 2 between if
		3 set-port-pin-mode-pin @ 2* lshift
		set-port-pin-mode-addr @ GPIO_PUPDR + bic!
		set-port-pin-mode-pin @ 2* lshift
		set-port-pin-mode-addr @ GPIO_PUPDR + bis!
	else
		." error. Pull Up, Pull down config out of range." cr
		2drop 2drop exit
	then
	dup 0 3 between if
		3 set-port-pin-mode-pin @ 2* lshift
		set-port-pin-mode-addr @ GPIO_OSPEEDR + bic!
		set-port-pin-mode-pin @ 2* lshift
		set-port-pin-mode-addr @ GPIO_OSPEEDR + bis!
	else
		." error. Speed config out of range." cr
		drop 2drop exit
	then
	dup 0 1 between if
		1 set-port-pin-mode-pin @ lshift
		set-port-pin-mode-addr @ GPIO_OTYPER + bic!
		set-port-pin-mode-pin @ lshift
		set-port-pin-mode-addr @ GPIO_OTYPER + bis!
	else
		." error. Type of output out of range." cr
		2drop exit
	then
	dup 0 3 between if
		3 set-port-pin-mode-pin @ 2* lshift
		set-port-pin-mode-addr @ GPIO_MODER + bic!
		set-port-pin-mode-pin @ 2* lshift
		set-port-pin-mode-addr @ GPIO_MODER + bis!
	else
		." error. Pin mode out of range." cr
		drop exit
	then
;

: init-spi1
	1 12 lshift RCC_APB2ENR_R bis!
	1 0 lshift RCC_AHB1ENR_R bis!
	1 4 lshift RCC_AHB1ENR_R bis!
	( mode type speed pupd out af pin portaddr -- )
	2 0 2 2 0 5 5 GPIOA_BASE set-port-pin-mode
	2 0 2 1 0 5 6 GPIOA_BASE set-port-pin-mode
	2 0 2 1 0 5 7 GPIOA_BASE set-port-pin-mode
	1 0 2 0 1 0 3 GPIOE_BASE set-port-pin-mode
	0 0 2 0 0 0 0 GPIOE_BASE set-port-pin-mode
	0 0 2 0 0 0 1 GPIOE_BASE set-port-pin-mode
	\ $031C SPI1_CR1 h!
	$031C SPI1_CR1 h!
	100 0 do loop
	$0040 SPI1_CR1 bis!
;

$1000 constant LIS302DL_FLAG_TIMEOUT
LIS302DL_FLAG_TIMEOUT variable LIS302DLTimeout

\ Read/Write command
$80 constant READWRITE_CMD
\ Multiple byte read/write command
$40 constant MULTIPLEBYTE_CMD
\ Dummy Byte Send by the SPI Master device in order
\  to generate the Clock to the Slave device
$00 constant DUMMY_BYTE

: LIS302DL-SendByte ( B -- B )
	LIS302DL_FLAG_TIMEOUT LIS302DLTimeout !
	begin
		-1 LIS302DLTimeout +!
		LIS302DLTimeout @ 0= if
			drop 0 exit
		then
		2 SPI1_SR hbit@
	until
	SPI1_DR c!
	LIS302DL_FLAG_TIMEOUT LIS302DLTimeout !
	begin
		-1 LIS302DLTimeout +!
		LIS302DLTimeout @ 0= if
			0 exit
		then
		1 SPI1_SR hbit@
	until
	SPI1_DR c@
;

: write-data ( addr n reg# -- )
    1 LIS302_CS_PIN lshift LIS302_CS_PORT GPIO_ODR + hbic!
		over 1 > if
			MULTIPLEBYTE_CMD or
			READWRITE_CMD not and
		else
			READWRITE_CMD MULTIPLEBYTE_CMD or not and
		then
    LIS302DL-SendByte drop
    0 do
			dup c@ LIS302DL-SendByte drop
			1+
    loop
    drop
    1 LIS302_CS_PIN lshift LIS302_CS_PORT GPIO_ODR + hbis!
;

: read-data ( addr n reg# -- )
    1 LIS302_CS_PIN lshift LIS302_CS_PORT GPIO_ODR + hbic!
		over 1 > if
			MULTIPLEBYTE_CMD or READWRITE_CMD or
		else
			READWRITE_CMD or
		then
    LIS302DL-SendByte drop
    0 do
			DUMMY_BYTE LIS302DL-SendByte over c!
			1+
    loop
    drop
    1 LIS302_CS_PIN lshift LIS302_CS_PORT GPIO_ODR + hbis!
;

: wai? ( -- B )
	spi-buf 1 LIS302DL_WHO_AM_I read-data
	spi-buf c@
;

: status? ( -- B )
	spi-buf 1 LIS302DL_STATUS_REG read-data
	spi-buf c@
;

: x.axis? ( -- n )
	spi-buf 2 LIS302DL_OUT_X read-data
	spi-buf c@
;

: y.axis? ( -- B )
	spi-buf 2 LIS302DL_OUT_Y read-data
	spi-buf c@
;

: z.axis? ( -- B )
	spi-buf 2 LIS302DL_OUT_Z read-data
	spi-buf c@
;

: acc? ( -- x y z )
	spi-buf 6 LIS302DL_OUT_X read-data
	spi-buf c@
	spi-buf 2 + c@
	spi-buf 4 + c@
;

: test01
	init-spi1
	wai? hex . decimal
;

0 variable lis-count

: test02
	0 lis-count !
	1 LIS302_CS_PIN lshift LIS302_CS_PORT GPIO_ODR + hbic!
	$20 LIS302DL-SendByte drop
  $47 LIS302DL-SendByte drop
  1 LIS302_CS_PIN lshift LIS302_CS_PORT GPIO_ODR + hbis!
	begin
		5 ms
		status? 8 and 0= if
			1 lis-count +!
		else
			acc? rot . swap . . cr
			0 lis-count !
		then
		lis-count @ 500 >
	until
;

: test03
	1 LIS302_CS_PIN lshift LIS302_CS_PORT GPIO_ODR + hbic!
	$20 LIS302DL-SendByte drop
  $47 LIS302DL-SendByte drop
  1 LIS302_CS_PIN lshift LIS302_CS_PORT GPIO_ODR + hbis!
	1000 0 do
		5 ms
		acc? rot . swap . . cr
	loop
;


