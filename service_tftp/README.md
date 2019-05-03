# DOCKER TFTP SERVER

* based on alpine:3.9
* environment variables:
    * TFTP_DIR=/tftpboot
* default tftp options:
    * --ipv4
    * --foreground
    * --verbose
    * --listen
    * --user tftp
    * --secure ${TFTP_DIR}
* logs are stored in **/logs** directory:
    * **/logs/out.log** - contains stdout (fd1)
    * **/logs/err.log** - contains stderr (fd2)

### Build:
`docker build -t tftp -f Dockefile .`
### Use docker volume - its more secure than bind mount
### Simple run:
`docker run -d --name tftp_server --mount source=tftp_log_vol,destination=/logs -p 69:69/udp tftp`
### To provide config files you can use:
```
docker run -d \
    --name tftp_server \
    --mount source=tftp_log_vol,destination=/logs \
    --mount source=tftp_files,destination=/tftpboot,readonly \
    -p 69:69/udp \
    tftp
```
**Remember that if you change TFTP_DIR variable you had to change destination directory in mountpoint, eg.**
```
docker run -d \
    --name tftp_server \
    --mount source=tftp_log_vol,destination=/logs \
    --mount source=tftp_files,destination=/tftpboot,readonly \
    --env TFTP_DIR=/var/tftpboot \
    -p 69:69/udp \
    tftp
```
