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
`docker run -d --name tftp_server --mount source=tftp_log_vol,destination=/logs,readonly tftp`

