#!/bin/bash

docker build --tag=dante_dhcp .
readonly DANGLING=$(docker images --format {{.ID}} --filter dangling=true)
[[ -z $DANGLING ]] && echo "No dangling images found." || docker rmi $DANGLING

