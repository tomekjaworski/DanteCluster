#/bin/bash
#

echo "#####################################################"
echo "##                                                 ##"
echo "## Dante Cluster :: DHCP/PXE virtualization        ##"
echo "## Author: Tomasz Jaworski, 2019                   ##"
echo "##                                                 ##"
echo "#####################################################"

set -e


cd /srv
python dhcp_builder.py

cp dhcpd.conf /etc/dhcp/dhcpd.conf

echo Running DHCP server...
dhcpd -4 -f -d --no-pid

#/bin/bash

