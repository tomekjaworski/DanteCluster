#!/bin/bash

readonly TFTP_FILES_VOL="tftp_files"
readonly FILEPATH="../nfsroot/boot"

echo "Making docker volume with tftp files"
docker run --rm -v $(pwd)/$FILEPATH:/files0 --mount source=$TFTP_FILES_VOL,destination=/files1 alpine:3.9 cp -r /files0 /files1 1>/dev/null

echo "Building docker tftp server."
docker build -t tftp -f Dockerfile . 1>/dev/null

echo "Remove dangling docker images."
docker rmi $(docker images -f "dangling=true" -q) 1>/dev/null

echo "Print info about newly created tftp image"
docker image ls | grep -w "^tftp"
