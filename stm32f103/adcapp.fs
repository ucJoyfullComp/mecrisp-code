\ compiletoflash

\ define structure for statistic analisys of samples
: key-val create 1+ 2 cells * allot does> ;
: .bins 0 cells + ; inline
: .samples cell+ ; inline
: .key swap 1+ 2 * cells + ; inline
: .val swap 1+ 2 * 1+ cells + ; inline

\ compiletoram
