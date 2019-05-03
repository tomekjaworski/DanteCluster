#!/bin/bash

readolny TFTP_LOG_VOL="tftp_log_vol"
readolny TFTP_FILES_VOL="tftp_files"

docker volume ls | grep "$TFTP_LOG_VOL"
if [[ $? -ne 0 ]]; then
    echo -e "Creating "$TFTP_LOG_VOL" volume."
    docker volume create "$TFTP_LOG_VOL"
    [[ $? -ne 0 ]] && echo -e "[ERROR] Cannot create new docker volume \'$TFTP_LOG_VOL\'.\nExiting..." && exit 1
fi
docker run -d \
    --name tftp_server \
    --mount source="$TFTP_LOG_VOL",destination=/logs \
    --mount source="$TFTP_FILES_VOL",destination=/tftpboot,readonly \
    -p 69:69/udp \
    tftp

