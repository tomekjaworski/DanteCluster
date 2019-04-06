#!/bin/bash
cd node

find . | cpio -H newc -o > ../initramfs.cpio
cd ..
cat initramfs.cpio | gzip -v1 > initramfs.igz
cp initramfs.igz /srv/tftp/kernel
