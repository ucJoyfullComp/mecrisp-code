compiletoflash

include utils.fs
include regmap.fs
include sysclock.fs
include delay.fs

: init
    setsysclock
    init-delay
;

compiletoram

1s-Pulse

init


