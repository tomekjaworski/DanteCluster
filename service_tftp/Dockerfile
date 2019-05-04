#
# Docker-based DHCP service for Dante Automated Unit and Acceptance Testing Cluster
# Authors:  Tomasz Jaworski, 2019
#           Sebastian Bąkała, 2019
#
FROM alpine:3.9 AS pxe
#ENV VMLINUZ vmlinuz-4.9.0-8-amd64
#ENV INITRD initrd.img-4.9.0-8-amd64
ENV TFTP_DIR /tftpboot
COPY ./files/stage0 ${TFTP_DIR}
RUN apk add --update syslinux \
    && cp /usr/share/syslinux/pxelinux.0 ${TFTP_DIR} \
    && cp /usr/share/syslinux/menu.c32 ${TFTP_DIR} \
    && cp /usr/share/syslinux/ldlinux.c32 ${TFTP_DIR} \
    && cp /usr/share/syslinux/libutil.c32 ${TFTP_DIR} \
    && find ${TFTP_DIR} -type f -exec chmod 0444 {} +
FROM alpine:3.9
#ENV VMLINUZ vmlinuz-4.9.0-8-amd64
#ENV INITRD initrd.img-4.9.0-8-amd64
ENV TFTP_DIR /tftpboot
EXPOSE 69/udp
COPY --from=pxe ${TFTP_DIR} ${TFTP_DIR}  
COPY ./files/stage1/start.sh /bin/start
RUN apk add --no-cache tftp-hpa \
    && adduser -D tftp \
    && mkdir /logs
ENTRYPOINT ["start"]
CMD ["--ipv4", "--foreground", "--verbose", "--listen", "--user", "tftp"]
