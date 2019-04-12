#/bin/bash

cd /srv
python dhcp_builder.py

cp dhcpd.conf /etc/dhcp/dhcpd.conf

#

#service isc-dhcp-server start
dhcpd -4 -f -d --no-pid

/bin/bash

