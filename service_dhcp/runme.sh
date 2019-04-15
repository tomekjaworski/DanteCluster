#!/bin/bash
#

docker build --tag dante_dhcp .
docker run --rm  -it --network=host --name dante_dhcp dante_dhcp
