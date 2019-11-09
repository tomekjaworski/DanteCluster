#!/bin/bash

docker rm `docker ps -a --format {{.ID}}`
docker image rm `docker image  ls --format {{.ID}}`


