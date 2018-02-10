compiletoflash

\ some general words missing in mecrisp-stellaris
include 00_xtrafunc.fs
\ general STM32F103 Base addresses and register mappings for single function blocks
include 01_regdefs.fs
\ STM32F103 register offsets for multiple instanced functions
include 02_regdefsofs.fs
\ driver for controlling the system clocks and PLL
include 03_sysclkpll.fs
\ driver and words for setting the SYSTICK block of the Cortex-M3 (1 mili-second)
include 04_systick.fs
\ generate frequency from TIM4 used also to drive the ADC driver sampling
include 05_genfreqtim4.fs
\ driver for ADC1 and ADC2 in multi-channel mode using the DMA and TIM4 as clock driver
include 06_adcmcdrvr.fs

compiletoram


