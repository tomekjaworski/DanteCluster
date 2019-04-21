# Dante Automated Acceptance and Unit Testing Cluster

OS version for the Controller: **Debian 9.8 (stretch)**
 * Netinstal ISO: [https://cdimage.debian.org/debian-cd/current/amd64/iso-cd/debian-9.8.0-amd64-netinst.iso](https://cdimage.debian.org/debian-cd/current/amd64/iso-cd/debian-9.8.0-amd64-netinst.iso).
 
Hardware:
 * At least two network controllers attacged (in my case both are Ethernets).

### Debian Installation Process

The following steps are prepared based on Virtual Box simulation for netinst ISO file (above) and tested on real hardware.
As a result you will have a cleanly installed OS.

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

All commands and scripts assume that you are logged in as `root`, including remote session.

### Install base Linux tools

Install man-pages `man` reader and `ifconfig`.

```
apt install --no-install-recommends --yes man
```

Install and run OpenSSH Server. Remote terminal is more user friendly, at least for me :)

```
apt install --no-install-recommends --yes openssh-server net-tools
```

Install additional helper tools like:
 * `mc` - Midnight Commander
 * `htop` - a `top` alternative
 * `tmux` - for multiple text terminals in a single window
 * `vim` - because you love it :)

```
apt install --no-install-recommends --yes mc htop tmux vim
```

### Network setup

At this point we have a machine that has a default network setting.
To see it, run `ifconfig -a` command. Output should be something like this:
```
enp0s3: flags=4163<UP,BROADCAST,RUNNING,MULTICAST>  mtu 1500
        inet6 fe80::a00:27ff:fee9:7f29  prefixlen 64  scopeid 0x20<link>
        ether 08:00:27:e9:7f:29  txqueuelen 1000  (Ethernet)
        RX packets 0  bytes 0 (0.0 B)
        RX errors 0  dropped 0  overruns 0  frame 0
        TX packets 8  bytes 648 (648.0 B)
        TX errors 0  dropped 0 overruns 0  carrier 0  collisions 0

lo: flags=73<UP,LOOPBACK,RUNNING>  mtu 65536
        inet 127.0.0.1  netmask 255.0.0.0
        inet6 ::1  prefixlen 128  scopeid 0x10<host>
        loop  txqueuelen 1  (Local Loopback)
        RX packets 0  bytes 0 (0.0 B)
        RX errors 0  dropped 0  overruns 0  frame 0
        TX packets 0  bytes 0 (0.0 B)
        TX errors 0  dropped 0 overruns 0  carrier 0  collisions 0

enp0s8: flags=4163<UP,BROADCAST,RUNNING,MULTICAST>  mtu 1500
        inet6 fe80::a00:27ff:fe82:2160  prefixlen 64  scopeid 0x20<link>
        ether 08:00:27:82:21:60  txqueuelen 1000  (Ethernet)
        RX packets 173  bytes 16801 (16.4 KiB)
        RX errors 0  dropped 0  overruns 0  frame 0
        TX packets 145  bytes 29013 (28.3 KiB)
        TX errors 0  dropped 0 overruns 0  carrier 0  collisions 0
```

The `lo` device is the loopback and should be left alone. However both `enp`s are what we want to play with. Those names are of our two network cards.
And because they will change from configuration to configuration I've decided to normalize them by changing their names.
Bare in mind that new names will be used further in this tutorial along with any config file that is available here.

For this we will use `systemd` functionality. Since `systemd` is here and it's not going anywhere soon we should try and use it after all.
Debian installer creates `/etc/network/interfaces` file by default. Rename it to `interfaces.old` and replace it with systemd-based approach. 

To change name of a network interface we need to know its MAC address (see `ifconfig -a` output).
Let us assume that `08:00:27:e9:7f:29` identifies an interface that is connected to the internal (inner) network with cluster node machines,
while `08:00:27:82:21:60` is connected to the outer world network (LAN, WAN or whatever - the Internets perhaps?).

If so, create two files: **/etc/systemd/network/10-inner.link** and **/etc/systemd/network/10-outer.link** with the following content:
```
[Match]
MACAddress=xx:xx:xx:xx:xx:xx

[Link]
Name=__name__goes__here__
```
however replace the `xx:xx:xx:xx:xx:xx` and `__name__goes_here__` parts with MAC address correlated with proper `inner`/`outer` name.

Now we should create ISO Layer-3 description for both interfaces with the following files (see `/confs/etc-systemd-network/` in this repo).
Those files will start outer network interface in DHCP mode and inner as `10.10.0.1`.

* **Outer** network file `/etc/systemd/network/outer.network`:
```
[Match]
Name=outer

[Network]
DHCP=v4
```

* **Inner** network file `/etc/systemd/network/inner.network`:

```
[Match]
Name=inner

[Network]
Address=10.10.0.1/16
#Gateway=192.168.0.104
#DNS=8.8.8.8
```

Now update your `initrd` configuration by running `update-initramfs -u` and reboot your machine. New settings will be used during the next boot.
After that check your new settings (`ifconfig -a`) and you should see something like this:

```
inner: flags=4163<UP,BROADCAST,RUNNING,MULTICAST>  mtu 1500
        inet 10.10.0.1  netmask 255.255.0.0  broadcast 10.10.255.255
        inet6 fe80::a00:27ff:fee9:7f29  prefixlen 64  scopeid 0x20<link>
        ether 08:00:27:e9:7f:29  txqueuelen 1000  (Ethernet)
        RX packets 0  bytes 0 (0.0 B)
        RX errors 0  dropped 0  overruns 0  frame 0
        TX packets 8  bytes 648 (648.0 B)
        TX errors 0  dropped 0 overruns 0  carrier 0  collisions 0

outer: flags=4163<UP,BROADCAST,RUNNING,MULTICAST>  mtu 1500
        inet 192.168.1.108  netmask 255.255.255.0  broadcast 192.168.1.255
        inet6 fe80::a00:27ff:fe82:2160  prefixlen 64  scopeid 0x20<link>
        ether 08:00:27:82:21:60  txqueuelen 1000  (Ethernet)
        RX packets 318  bytes 28663 (27.9 KiB)
        RX errors 0  dropped 0  overruns 0  frame 0
        TX packets 264  bytes 44977 (43.9 KiB)
        TX errors 0  dropped 0 overruns 0  carrier 0  collisions 0
```

If so, connect a SSH client (eg. putty) to the IP address shown (here it's `192.168.1.108`) and now you can work remotely.

Remote session doesn't allow you to connect as `root` (and that's good).
You need to create your private user, log into it and switch to root by issuing simple:
```
su
```

### Install this repository

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

### Install Docker

For this task you can follow the Official Docker Docs [here](https://docs.docker.com/install/linux/docker-ce/debian/) 
or run [01_install_docker.sh](https://github.com/tomekjaworski/DanteCluster/blob/feature/docs/01_install_docker.sh) script based on it.

```
./01_install_docker.sh
```

### Test contenerized DHCP and TFTP
At this point network and Docker is up and running. We can start TFTP and DHCP services for the inner network. No machine will boot up since there is no kernel provided yet (no NFS filesystem is set) but at least we will know it this basic setup is ok.

To do this tart both services via `./make.sh` and `./runme.sh` from both `/srv/dc/service_dhcp` and `/srv/dc/service_tftp`. The runme scripts can be configured to run in foreground. If so, use `tmux` as message sink or change those scripts to run `docker` detached.

While any cluster machine is powered on you should see DHCPLEASE being requested and followed by `pxelinux.0` retrieval.
Of course this machine should be configured to bootup via PXE. If so you should see somethig more or less like this:

![menu.c32](https://wiki.syslinux.org/wiki/images/2/20/Simplemenu.png)

From [Syslinux Wiki](https://wiki.syslinux.org/wiki/index.php?title=File:Simplemenu.png).


### NFS Installation
In order to provide `/` directory (and its content) to the nodes the `NFS` server is used. To install and initialize it, use the following commands:
```
apt install nfs-kernel-server
```
Now, to initialize the client tracker (`nfsdcltrack`) type:
```
mkdir /var/lib/nfs/nfsdcltrack
nfsdcltrack init
```
After that the file `/var/lib/nfs/nfsdcltrack/main.sqlite` should exist.

Now add the following line to `/etc/exports` and restart the NFS server with `systemctl restart nfs-server.service`:
```
/srv/dc/nfsroot 10.10.0.1/255.255.0.0(ro,sync,no_root_squash,fsid=0)
```
It will open a remote mount point named `/srv/dc/nfsroot` available at `10.10.0.1` (this machine) for the whole subset /16.

You can now go and try to test mount this resource by typing:
```
mkdir /test_mount
moun -t nfs 10.10.0.1:/srv/dc/nfsroot /test_mount
```
You should be now able to see your `/srv/dc/nfsroot` content under `/test_mount`.


### Custom Debian distro preparation

This step is based on `debootstrap` Debian package and modified scripts, provided by *Markus Rosenstihl* for his own high-performance cluster.
More information you can find on [his Bitbucket webpage](https://bitbucket.org/mrosenstihl/debian-diskless-cluster/wiki/Home) and Wiki.

Before we can start, the `debootstrap` package should be installed:
```
apt install --no-install-recommends --yes debootstrap
```

Now run main script:
```
./bootstrap.sh -d /srv/dc/nfsroot
```
This will take couple of minutes - some coffee is suggested :) After successful instalation you will have a Debian distro in `/srv/dc/nfsroot`.


WORK IN PROGRESS