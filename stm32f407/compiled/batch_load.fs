compiletoflash

include 00_xtrafunc.fs
include 01_sine.fs
include 02_utils.fs
include 03_regmap.fs
include 04_sysclock.fs
include 05_delay.fs

: init
    setsysclock
    init-delay
;

compiletoram

1s-Pulse

init


