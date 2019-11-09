#!/bin/bash
#

echo "#####################################################"
echo "##                                                 ##"
echo "## Dante Cluster :: TFTP virtualization            ##"
echo "## Author: Tomasz Jaworski, 2019                   ##"
echo "##                                                 ##"
echo "#####################################################"

set -e

service rsyslog start
service rsyslog status

#
echo Running TFTP server...
/usr/sbin/in.tftpd --listen --user tftp --address 0.0.0.0:69 --secure /srv/tftp -vv

echo Running tail on /var/log/syslog...
tail -f /var/log/syslog


