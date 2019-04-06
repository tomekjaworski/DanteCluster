#!/bin/bash

# Check who am i
if [ "$USER" != "root" ]
then
	echo "Only root can run this script"
	exit 1
fi

apt-get install debootstrap

debootstrap				\
	--verbose			\
	--variant=minbase		\
	--arch=amd64			\
	stretch ./node 			\
	http://ftp.pl.debian.org/debian


