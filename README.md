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

### Software installation

work in progress
