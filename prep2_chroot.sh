#!/bin/bash

# Check who am i
if [ "$USER" != "root" ]
then
	echo "Only root can run this script"
	exit 1
fi

# Setup cluster node name
# Hostname will be changed later, according do DHCP lease
echo "dante-node-base" > /etc/hostname
 
# Update distro and install selected kernel
apt update
apt install --no-install-recommends --yes	\
		linux-image-4.9.0-8-amd64			\
		systemd-sysv

echo "==============="

apt install --no-install-recommends --yes	\
		vim									\
		tmux								\
		wget								

echo "==============="

# Cleanup	
apt-get clean

# Set default root:root password (TO BE REMOVED!!)
passwd root <<EOF
root
root
EOF

exit





