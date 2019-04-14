#!/bin/bash
#

set -e

YELLOW='\033[0;33m'	# Yellow
NOCOLOR='\033[0m'	# No color

function msg () # 1:string
{
    echo -e "${YELLOW}$1${NOCOLOR}"
}

#
# Steps taken from Docker Docs:
# https://docs.docker.com/install/linux/docker-ce/debian/
#


clear

msg "Update the apt package index"
apt update

msg "Install packages to allow apt to use a repository over HTTPS"
apt install --yes apt-transport-https curl gnupg2 software-properties-common

msg "Add Docker’s official GPG key"
curl -fsSL https://download.docker.com/linux/debian/gpg | apt-key add -

msg "Verify key with fingerprint 0EBFCD88 and press any key"
apt-key fingerprint 0EBFCD88

echo "Press any key..."
read

# ---------

msg "Set up the stable repository"
msg "lsb_release = $(lsb_release -cs)"

add-apt-repository "deb [arch=amd64] https://download.docker.com/linux/debian $(lsb_release -cs) stable"

# ----------

msg "nstall docker CE"
apt-get update
apt-get install --yes docker-ce docker-ce-cli containerd.io

msg "DONE"
exit 0


