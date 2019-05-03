#!/bin/bash

readonly TFTP_FILES_VOL="tftp_files"
readonly FILEPATH="../nfsroot/boot"

echo "Making docker volume with tftp files"
docker run --rm -v $(pwd)/$FILEPATH:/files --mount source=$TFTP_FILES_VOL,destination=/files alpine:3.9 bin/sh -c "ls -la /files"

echo "Building docker tftp server."
docker build -t tftp -f Dockefile .

