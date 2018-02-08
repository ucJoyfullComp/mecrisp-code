#!/bin/bash

pushd /home/uzi/Dropbox/Hw/Embedded/mecrisp/stm32f103

MYDEV=$1
if [ -z $MYDEV ] ; then
	echo "I am empty"
	MYDEV="/dev/ttyUSB0"
fi

#picocom --send-cmd "ascii-xfr -s -l 100" --receive-cmd "ascii-xfr -r" --imap lfcrlf,bsdel -b 115200 -f n -p n -d 8 $MYDEV 
picocom --send-cmd "ascii-xfr -s -l 110" --receive-cmd "ascii-xfr -r -n" --imap lfcrlf,crcrlf --omap delbs,crlf -b 9600 -f n -p n -d 8 $MYDEV 

popd
