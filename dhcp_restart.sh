#!/bin/bash


ifconfig enp0s8 10.10.0.1 netmask 255.255.0.0
service isc-dhcp-server restart
service tftpd-hpa restart
service nfs-kernel-server restart





