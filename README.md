# Dante Automated Acceptance and Unit Testing Cluster


OS version for the Controller: **Debian 9.8 (stretch)**
 * Netinstal ISO: [https://cdimage.debian.org/debian-cd/current/amd64/iso-cd/debian-9.8.0-amd64-netinst.iso](https://cdimage.debian.org/debian-cd/current/amd64/iso-cd/debian-9.8.0-amd64-netinst.iso).
 
Hardware should have two network interfaces with the following assumption:
 * Interface **enp0s3** is connected to the main network (outer layer).
 * Interface **enp0s8** is connected to the node machines (inner layer).

Bare in mind that those two names are used in configuration files and if differ - should be normalized before the installation begins.

### Debian Installation Process

The following steps are prepared based on Virtual Box simulation for netinst ISO file (above). But there is no reason why a real hardware installation would behave any differently. As a result you will have a cleanly installed OS.

1. Boot the netinstaller from downloaded image.
1. Select *Install*.
1. Language: *English*.
1. Country, territory or area: *other* -> *Europe* -> *Poland*.
1. Default locale settings: *United States*.
1. Keymap: *Polish*.
1. Primary network interface: *enp0s3*.
1. Hostname: *cnc*  (as this machine will work as an command and control station).
1. Domain name: can be ommited (none).
1. Root password: you should give some :)
1. Normal user: create one if you wish.
1. Partition disks: you can do any, but I've basically have chosen *Guided - use entire disk*.

Now wait for the base system packages to be installed...

1. Select an installation mirror (or don't).
1. Participate in package popularity contest (or don't).
1. Software selection: *Uncheck all suggestions*. All necessary packages can be installed later via `apt install`.
1. Install the GRUB boot loader on a hard disk: select *Yes* and */dev/sda*.
1. Finish!

### Software installation (WORK IN PROGRESS)

All commands and scripts assume that you are logged in as `root`, including remote session.

#### Install base Linux tools

Install man-pages `man` reader and `ifconfig`.

```
apt install --no-install-recommends --yes man net-tools 
```

Install and run OpenSSH Server. Remote terminal is more user friendly, at least for me :)

```
apt install --no-install-recommends --yes openssh-server net-tools
```

Now check your external IP for the *enp0s3* interface with `ifconfig enp0s3` command to see something like this:
```
enp0s3: flags=4163<UP,BROADCAST,RUNNING,MULTICAST>  mtu 1500
        inet 192.168.1.108  netmask 255.255.255.0  broadcast 192.168.1.255
        inet6 fe80::a00:27ff:fe82:2160  prefixlen 64  scopeid 0x20<link>
        ether 08:00:27:82:21:60  txqueuelen 1000  (Ethernet)
        RX packets 3172  bytes 4532082 (4.3 MiB)
        RX errors 0  dropped 0  overruns 0  frame 0
        TX packets 1364  bytes 106407 (103.9 KiB)
        TX errors 0  dropped 0 overruns 0  carrier 0  collisions 0
```

If so, connect a SSH client (eg. putty) to the IP address shown (here it's `192.168.1.108`) and now you can work remotely.
In addition you can now install helper tools like:
 * `mc` - Midnight Commander
 * `htop` - a `top` alternative
 * `tmux` - for multiple text terminals in a single window
 * `vim` - because you love it :)

```
apt install --no-install-recommends --yes mc htop tmux vim
```

Remote session doesn't allow you to connect as `root` (and that's good).
You need to create your private user, log into it and switch to root by issuing simple:
```
su
```

#### Install this repository

Firstly you have to install `git` tool and *https* support (SSL):
```
apt install --no-install-recommends --yes git 
apt install --no-install-recommends --yes ca-certificates openssl
```

and clone this [repository](https://github.com/tomekjaworski/DanteCluster) into `/srv/dc` directory.

```
cd /srv
git clone https://github.com/tomekjaworski/DanteCluster dc
```

# WORK IN PROGRESS

#### Install Docker

For this task you can follow the Official Docker Docs [here](https://docs.docker.com/install/linux/docker-ce/debian/) 
or run [01_install_docker.sh](https://github.com/tomekjaworski/DanteCluster/blob/feature/docs/01_install_docker.sh) script based on it.

```
./01_install_docker.sh
```


