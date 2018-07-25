\ set the GPIO port at which RX711 is connected to, set the Clock and Data pins
GPIOB_BASE constant RX711_PORT
$400 constant RX711_Clk_Pin
$800 constant RX711_Data_Pin

\ set the GPIO pins for RX711 assume it is connected 
\ at PB10 - Clock, PB11 - Data
: SETUP-PB10_11
	$9 RCC_APB2ENR_R HBIS!
	$00004300 RX711_PORT GPIO_CRH + ! 
	$04000000 RX711_PORT GPIO_BSRR + !
;

\ set the Clock pin to High state
: RX711-Clk-Hi
    RX711_Clk_Pin RX711_PORT GPIO_ODR + hbis!
; inline

\ set the Clock pin to Low state
: RX711-Clk-Lo
    RX711_Clk_Pin RX711_PORT GPIO_ODR + hbic!
; inline

\ Read the Data pin state
: RX711-Data-Rd
    RX711_Data_Pin RX711_PORT GPIO_IDR + hbit@ 1 and
; inline

\ set the state of RX711 to Normal operation
: RX711-Normal ( -- )
    RX711-Clk-Lo
    begin RX711-Data-Rd until
;

\ set the RX711 in Power Down mode.
: RX711-PowerDown ( -- )
    RX711-Clk-Hi
    70 us
;

\ Read a Data bit
: RX711-ReadBit ( -- bit )
    RX711-Clk-Hi
    5 0 do loop
    RX711-Clk-Lo
    RX711-Data-Rd
    2 0 do loop
;

\ send a clock pulse to the RX711 - about 1.8uSec at core clock of 72MHz
: RX711-ClockPulse ( -- )
    RX711-Clk-Hi
    5 0 do loop
    RX711-Clk-Lo
    4 0 do loop
;

\ read 24 bits of data, package them to a sample and push to stack
: RX711-Read-Data ( -- n )
    begin RX711-Data-Rd 0= until
    0
    24 0 do
        2*
        RX711-ReadBit
        or
    loop
;

\ set the next conversion to isample from input A with PGA gain of 128
: RX711-Conv-A128
    RX711-ClockPulse
;

\ set the next conversion to isample from input B with PGA gain of 32
: RX711-Conv-B32
    RX711-ClockPulse
    RX711-ClockPulse
;

\ set the next conversion to isample from input A with PGA gain of 64
: RX711-Conv-A64
    RX711-ClockPulse
    RX711-ClockPulse
    RX711-ClockPulse
;

\ setup gpio, config RX711 to normal mode, read data from A with gain 128, 
\ set next conversion to 
: RX711-Read-A128 ( -- n )
    SETUP-PB10_11
    RX711-Normal
    70 us
    RX711-Read-Data
    RX711-Conv-A128
;

\ setup RX711: PB10 - Clock, PB11 - Data
\ init RX711: set RX711 state to Normal, not at power down
\ continually read the value of input B with gain of 32 and display the read
\ value and the voltage equivalent with AVCC set to 4.009
: test01 ( -- ) 
    SETUP-PB10_11
    RX711-Normal
    70 us
    begin
        RX711-Read-Data
        RX711-Conv-B32
        ." Value: " dup . ." Voltage: " 4009000 8388608 */ .1R6 cr
        key?
    until
    RX711-PowerDown
;

\ test clock rate to RX711
: test02 ( u -- ) \ u is the number of clocks to generate.
    SETUP-PB10_11
    0 do 
        RX711-ClockPulse
    loop
;


