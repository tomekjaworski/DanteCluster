#!/bin/bash

readonly TFTP_LOG_VOL="tftp_log_vol"
readonly TFTP_FILES_VOL="tftp_files"

function stop_and_remove_container() {
	if [[ ! -z "$(docker ps | grep $1)" ]]; then
		echo "Removing old $1 container..."
		docker stop $1
		docker rm $1
	fi
}

stop_and_remove_container "tftp_server"

docker run -d \
    --name tftp_server \
    --network=host \
    --mount source="$TFTP_LOG_VOL",destination=/logs \
    --mount source="$TFTP_FILES_VOL",destination=/tftpboot/kern,readonly \
    tftp

sleep 1
docker ps -a --format "table {{.ID}}\t{{.Image}}\t{{.Status}}\t{{.Names}}\t{{.Ports}}\t{{.Mounts}}" | grep "tftp_server"
