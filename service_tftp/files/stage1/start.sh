#!/bin/sh

set -e

echo "#####################################################"
echo "##                                                 ##"
echo "## Dante Cluster :: TFTP virtualization            ##"
echo "## Author: Tomasz Jaworski, 2019                   ##"
echo "## Maintainer: Sebastian Bąkała, 2019              ##"
echo "##                                                 ##"
echo "#####################################################"

if [[ ! $TFTP_DIR == "/tftpboot" ]]; then
    echo "Copying tftp files to new location..."
    mkdir -p $TFTP_DIR
    cp -r /tftpboot $TFTP_DIR
    rm -r /tftpboot
fi

echo -e "tftp parameters:\n$@ --secure $TFTP_DIR"

#if [[ -z "$@" ]]; then
#    echo "Empty parameter list provided for tftp daemon. Exiting..."
#    exit 1
#fi

echo Running TFTP server...
exec in.tftpd "$@" --secure $TFTP_DIR 1>/logs/out.log 2>/logs/err.log
