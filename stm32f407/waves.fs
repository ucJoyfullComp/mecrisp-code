: sawtooth ( n -- )
  dup 0 > if  
    begin
      $1000 0 do  i          $FFF i - dacs dup 0 do loop loop
    key? until
  else
    begin
      $1000 0 do  i          $FFF i - dacs loop
    key? until
  then
  drop
;

: square ( n -- )
  dup 0 > if ( n -- n )
    0
    begin
      dup not tuck
      $FFF and swap $FFF and dacs
      over 0 do loop
    key? until
  else
    0
    begin
      dup not tuck
      $FFF and swap $FFF and dacs
    key? until
  then
  2drop
;

: steps ( n -- )
  dup 0 > if  
    begin
      $1000 0 do  i          $FFF i - dacs dup 0 do loop $40 +loop
    key? until
  else
    begin
      $1000 0 do  i          $FFF i - dacs $40 +loop
    key? until
  then
  drop
;
