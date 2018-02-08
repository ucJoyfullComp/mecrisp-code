compiletoflash

include utils.fs
include dump.fs
include stm32f0xx.fs
include sysclock.fs

: init
    SetSysClock
;

include delay.fs

: init
    init
    init-delay
;

compiletoram

\ include usysclock.fs
include testusart2.fs


