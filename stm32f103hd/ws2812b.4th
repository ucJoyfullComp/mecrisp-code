: init-pb10 gpiob_base gpio_crh + dup @ $f00 bic $200 or swap ! ;
0 variable tim2-count
: send-zero ( -- )
	
;

: send-one ( -- )
	
;

: get-lsb ( u -- u )
	2 /mod if send-one else send-zero then
;
