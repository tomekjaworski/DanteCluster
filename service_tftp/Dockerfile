#
# Docker-based DHCP service for Dante Automated Unit and Acceptance Testing Cluster
# Author: Tomasz Jaworski, 2019
# 

# Use Debian 9.8 Stretch
FROM debian:9.8-slim

LABEL maintainer="Tomasz Jaworski <tjaworski@iis.p.lodz.pl>"

# Install needed packages
RUN apt update
RUN apt upgrade --yes
RUN apt install --yes tftpd-hpa pxelinux rsyslog
RUN apt install --yes net-tools


# Setup basic networking
EXPOSE 69/udp

# ------------------------------

# Prepare TFTP data directory for PXE
RUN cp /usr/lib/PXELINUX/pxelinux.0 /srv/tftp/
RUN cp /usr/lib/syslinux/modules/bios/ldlinux.c32 /srv/tftp/
RUN cp /usr/lib/syslinux/modules/bios/menu.c32 /srv/tftp
RUN cp /usr/lib/syslinux/modules/bios/libutil.c32 /srv/tftp

#COPY /srv/dc/nfsroot/boot/make.sh /srv/tftp/
COPY vmlinuz-4.9.0-8-amd64 /srv/tftp/vmlinuz-4.9.0-8-amd64
COPY initrd.img-4.9.0-8-amd64 /srv/tftp/initrd.img-4.9.0-8-amd64

RUN mkdir /srv/tftp/pxelinux.cfg
COPY default /srv/tftp/pxelinux.cfg/default

WORKDIR /srv
COPY internal_start.sh .

ENTRYPOINT ["./internal_start.sh"]

