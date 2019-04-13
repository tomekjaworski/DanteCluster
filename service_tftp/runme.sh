#!/bin/bash

docker build --tag dante_tftp .
docker run --rm -it --network=host --name dante_tftp  dante_tftp


