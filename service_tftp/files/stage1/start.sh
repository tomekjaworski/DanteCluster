#!/bin/sh
#

set -e

echo "#####################################################"
echo "##                                                 ##"
echo "## Dante Cluster :: TFTP virtualization            ##"
echo "## Author: Tomasz Jaworski, 2019                   ##"
echo "## Maintainer: Sebastian Bąkała, 2019              ##"
echo "##                                                 ##"
echo "#####################################################"

if [[ ! TFTP_DIR == "/tftpboot" ]]; then
    echo "Copying tftp files to new location..."
    mv /tftpboot $TFTP_DIR
    [[ $? -ne 0 ]] && echo -e "[ERROR] cannot move tftp directory to new location.\nExiting..." && exit 1
fi

echo "$@"

echo Running TFTP server...
exec in.tftpd "$@" 1>/logs/out.log 2>/logs/err.log
