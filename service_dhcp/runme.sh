#!/bin/bash
#

docker build --tag dante_pxe .
docker run --rm  -it --network=host --name dante_pxe dante_pxe
