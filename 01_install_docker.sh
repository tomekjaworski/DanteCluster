#!/bin/bash

set -o pipefail
source ./.helpers/functions.sh

#
# Steps taken from Docker Docs:
# https://docs.docker.com/install/linux/docker-ce/debian/
#

clear

msg "\n[INFO] Updating the apt package index"
apt update 2>> $LOGFILE
[[ $? -ne 0 ]] && err "[ERROR] Failed to apt update.\nCheck $LOGFILE file." && exit 1

msg "\n[INFO] Install packages to allow apt to use a repository over HTTPS"
apt install \
    --yes \
    --no-install-recommends \
    --no-install-suggests \
    apt-transport-https \
    curl \
    gnupg2 \
    software-properties-common \
    2>> $LOGFILE
[[ $? -ne 0 ]] && err "[ERROR] Failed while installing packets.\nCheck $LOGFILE file." && exit 2

msg "\n[INFO] Add Docker’s official GPG key"
curl -fsSL https://download.docker.com/linux/debian/gpg | apt-key add - 2>> $LOGFILE
[[ $? -ne 0 ]] && err "[ERROR] Failed while downloading docker gpg key.\nCheck $LOGFILE file." && exit 3

msg "\n[INFO] Verify key with fingerprint 0EBFCD88 and press any key"
apt-key fingerprint 0EBFCD88 2>> $LOGFILE
[[ $? -ne 0 ]] && err "[ERROR] Failed during fingerprint verification.\nCheck $LOGFILE file." && exit 4

msg "\n[INFO] Set up the stable repository"
add-apt-repository "deb [arch=amd64] https://download.docker.com/linux/debian $(lsb_release -cs) stable" 2>> $LOGFILE
[[ $? -ne 0 ]] && err "[ERROR] Failed to add-apt-repository.\nCheck $LOGFILE file." && exit 5

msg "\n[INFO] Install docker CE"
apt update 2>> $LOGFILE
[[ $? -ne 0 ]] && err "[ERROR] Failed to apt update.\nCheck $LOGFILE file." && exit 1

apt install --yes docker-ce docker-ce-cli containerd.io 2>> $LOGFILE
[[ $? -ne 0 ]] && err "[ERROR] Failed to install docker CE.\nCheck $LOGFILE file." && exit 6

rm $LOGFILE
msg "[OK] DONE."
exit 0
