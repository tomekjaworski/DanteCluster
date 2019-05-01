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

echo "$@"

echo Running TFTP server...
exec in.tftpd "$@" 1>/logs/out.log 2>/logs/err.log
