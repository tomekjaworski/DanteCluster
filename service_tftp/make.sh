#!/bin/bash

ln  ../nfsroot/boot/vmlinuz-4.9.0-8-amd64 vmlinuz-4.9.0-8-amd64
ln  ../nfsroot/boot/initrd.img-4.9.0-8-amd64 initrd.img-4.9.0-8-amd64

docker build --tag=dante_tftp .

unlink vmlinuz-4.9.0-8-amd64
unlink initrd.img-4.9.0-8-amd64
