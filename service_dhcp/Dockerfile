#
# Docker-based DHCP service for Dante Automated Unit and Acceptance Testing Cluster
# Author: Tomasz Jaworski, 2019
# 

# Use Debian 9.8 Stretch and Python 3.6.8
#FROM debian:9.8-slim
FROM python:3.6.8-slim-stretch

LABEL maintainer="Tomasz Jaworski <tjaworski@iis.p.lodz.pl>"

# Install needed packages
RUN apt update
RUN apt upgrade --yes
RUN apt install --yes isc-dhcp-server tftp-hpa
RUN apt install --yes net-tools


# Setup basic networking
EXPOSE 67/udp 67/tcp

# Prepare python env
RUN pip3 install jinja2
ADD dhcp_builder.py /srv/dhcp_builder.py

# ------------------------------

RUN touch /var/lib/dhcp/dhcpd.leases
ADD etc_default_isc-dhcp-server /etc/default/isc-dhcp-server

COPY dhcpd.conf.j2template /srv/dhcpd.conf.j2template
COPY machines.json /srv/machines.json


COPY internal_start.sh /srv/internal_start.sh


CMD ["/bin/bash","/srv/internal_start.sh"]

