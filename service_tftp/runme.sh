#!/bin/bash

readonly TFTP_LOG_VOL="tftp_log_vol"
readonly TFTP_FILES_VOL="tftp_files"

docker run -d \
    --name tftp_server \
    --mount source="$TFTP_LOG_VOL",destination=/logs \
    --mount source="$TFTP_FILES_VOL",destination=/tftpboot,readonly \
    tftp
docker ps -a --format "table {{.ID}}\t{{.Image}}\t{{.Status}}\t{{.Names}}\t{{.Ports}}\t{{.Mounts}}" | grep "tftp_server"
