#!/bin/bash

# Check who am i
if [ "$USER" != "root" ]
then
	echo "Only root can run this script"
	exit 1
fi

cp prep2_chroot.sh ./node
chroot ./node /prep2_chroot.sh




