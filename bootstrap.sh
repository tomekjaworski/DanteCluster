#!/bin/bash

#    Copyright (C) 2013,2014  Markus Rosenstihl
#
#    This program is free software: you can redistribute it and/or modify
#    it under the terms of the GNU General Public License as published by
#    the Free Software Foundation, either version 3 of the License, or
#    (at your option) any later version.
#
#    This program is distributed in the hope that it will be useful,
#    but WITHOUT ANY WARRANTY; without even the implied warranty of
#    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
#    GNU General Public License for more details.
#
#    You should have received a copy of the GNU General Public License
#    along with this program.  If not, see <http://www.gnu.org/licenses/>.
# exit if any command fails
set -e

YELLOW='\033[0;33m'	# Yellow
NOCOLOR='\033[0m'	# No color

function msg () # 1:string
{
    echo -e "${YELLOW}$1${NOCOLOR}"
}


# load our library
. diskless-lib

IMAGEDIR=False
SKIP_DEBOOTSTRAP=False

# -d <dir> - Install distro in <dir>; Directory should be absolute (e.g. /srv/node)
# -b - Omit debootstraping stage 

if [ ! `whoami` = "root" ]; then
	echo "This script should be run only as root"
	exit 1
fi


while getopts ":d:b" opt; do
	case  $opt in
		d)
		IMAGEDIR=$OPTARG 
		echo "Installing to $IMAGEDIR..."

		if [ -d $IMAGEDIR ]; then
		 echo "Directory exists: ${IMAGEDIR}"

		 echo "Press CTRL+C to break or ENTER to continue..."
		 read
		 #echo "aborting ..."
		 #exit 1
		fi
	esac
	case  $opt in
		b)
		echo "Omiting Debootstrap stage..."
		SKIP_DEBOOTSTRAP=True
	esac

done

#exit 1
if [ ! -f ~/.ssh/id_rsa.pub ]; then
	echo "Could not fine file ~/.ssh/id_rsa.pub"
	echo "Use ssh-keygen and create public key for nodes"
	exit 1
fi

#umount_chroot_image $IMAGEDIR

#exit 1
# include_packages should be one package per column
# replace "\n" with "," then remove the last "," to feed it to debootstraps --include
#INCLUDE_PACKAGES=$(awk 1 ORS="," config/include_packages | sed -e 's/,$//' -e 's/\ //')

if [ ! $IMAGEDIR = "False" ] ; then

	rm -rf $IMAGEDIR/etc/fstab
	rm -rf $IMAGEDIR/etc/mtab

	
	if [ $SKIP_DEBOOTSTRAP == "False" ] ; then
		msg "debootstrap to $IMAGEDIR"

		debootstrap --variant=minbase --components=main,contrib,non-free --arch=amd64 \
	    	--include=systemd,systemd-sysv \
			stretch $IMAGEDIR http://ftp.pl.debian.org/debian  | tee  debootstrap.log

		RETV=$?
		if [ $RETV -ne 0 ]; then
	    	echo "debootstrap status: failed: $RETV"
		    exit 1
		else
	    	echo "debootstrap status: success!"
		fi
	fi

	msg "install /etc/fstab"
	install files/etc_fstab $IMAGEDIR/etc/fstab

	
	# copy apt-cacher proxy configuration 
	msg "install /etc/apt/apt.conf.d/00proxy"
	install files/etc_apt_apt.d.conf.d_00proxy $IMAGEDIR/etc/apt/apt.conf.d/00proxy	

	# install system localization stuff
	msg "chroot: apt install locales"
	chroot $IMAGEDIR apt-get -y install locales

	# install prereq system stuff
	msg "chroot: apt install dialog netbase"
	chroot $IMAGEDIR apt-get -y install dialog netbase


	# set timezone and keyboard
	msg "Timezones and keyboard"
	echo "Europe/Berlin" > $IMAGEDIR/etc/timezone
	install $IMAGEDIR/usr/share/zoneinfo/Europe/Warsaw $IMAGEDIR/etc/localtime
	install files/etc_default_keyboard $IMAGEDIR/etc/default/keyboard

	# generate locales
	msg "Locale generation"
	echo "en_GB.UTF-8 UTF-8" > $IMAGEDIR/etc/locale.gen
	chroot $IMAGEDIR locale-gen
	chroot $IMAGEDIR update-locale LANG="en_GB.UTF-8"

	# allow all users to use /sbin/halt command (via sudo) so that they can reboot the node if needed
	echo "ALL	ALL=NOPASSWD: /sbin/shutdown, /sbin/halt, /sbin/reboot, /sbin/poweroff" >> $IMAGEDIR/etc/sudoers

	# prevent starting of services
	echo "prevent starting of daemons"
	echo -e "#!/bin/sh\necho Not starting daemon\nexit 101" > $IMAGEDIR/usr/sbin/policy-rc.d
	chmod 755 $IMAGEDIR/usr/sbin/policy-rc.d
	
	echo "prevent installation of suggested/recommended packages"
	echo -e "APT::Install-Recommends \"0\";" > $IMAGEDIR/etc/apt/apt.conf
	echo -e "APT::Install-Suggests \"0\";" >> $IMAGEDIR/etc/apt/apt.conf

	# networking	
	echo "auto lo" > $IMAGEDIR/etc/network/interfaces
	echo "iface lo inet loopback" >> $IMAGEDIR/etc/network/interfaces
	echo "iface enp0s3 inet manual" >> $IMAGEDIR/etc/network/interfaces
	
	
	# prepare DNS info for nodes and for APT during this installation
	#cp files/etc_resolv.conf $IMAGEDIR/etc/resolv.conf
	cp /etc/resolv.conf $IMAGEDIR/etc/resolv.conf


	echo "image" > $IMAGEDIR/etc/debian_chroot

	msg "mount chroot image"
	mount_chroot_image $IMAGEDIR
	chroot $IMAGEDIR ln -sv /proc/mounts /etc/mtab
	
	# kernel and initramfs
	chroot $IMAGEDIR apt-get -y install udev linux-image-amd64 firmware-linux initramfs-tools
	chroot $IMAGEDIR apt-get -y install aufs-tools
	sed -i 's/KEYMAP=n/KEYMAP=us/' $IMAGEDIR/etc/initramfs-tools/initramfs.conf
#	sed -i 's/DEVICE=/DEVICE=enp0s3/' $IMAGEDIR/etc/initramfs-tools/initramfs.conf
	echo "aufs" > $IMAGEDIR/etc/initramfs-tools/modules
	echo "overlay" > $IMAGEDIR/etc/initramfs-tools/modules
	cp -v files/initramfs_aufs $IMAGEDIR/etc/initramfs-tools/scripts/aufs
	# handled by aufs script
	mkdir -p $IMAGEDIR/nfsroot $IMAGEDIR/ramdisk
	chroot $IMAGEDIR update-initramfs -u



	# install nis nfs-client and openssh-server
	chroot $IMAGEDIR apt-get -y -q install nfs-common openssh-server
#	chroot $IMAGEDIR apt-get -y -q install nfs-common openssh-server ntp rsyslog

	# password less ssh login for root
	mkdir -p $IMAGEDIR/root/.ssh
	cat ~/.ssh/id_rsa.pub > $IMAGEDIR/root/.ssh/authorized_keys

	# install system stuff
	chroot $IMAGEDIR apt-get --yes --quiet install console-common console-data

	# install system tools
	chroot $IMAGEDIR apt-get --yes install moreutils manpages

	# install common tools
	chroot $IMAGEDIR apt-get --yes --quiet install vim sudo mc bzip2

	# install network tools
	chroot $IMAGEDIR apt-get --yes install net-tools iputils-ping
	

	# preseed NIS
	# configure NIS clients
#	echo "nis nis/domain string $(ypdomainname)" | chroot $IMAGEDIR debconf-set-selections
#	chroot $IMAGEDIR apt-get -y -q install nis
#	fgrep -xq "+::::::" $IMAGEDIR/etc/passwd || echo "+::::::" >> $IMAGEDIR/etc/passwd
#	fgrep -xq "+:::" $IMAGEDIR/etc/group || echo "+:::" >> $IMAGEDIR/etc/group
#	fgrep -xq "+::::::::" $IMAGEDIR/etc/shadow || echo "+::::::::" >> $IMAGEDIR/etc/shadow
#	fgrep -xq "ypserver 192.168.0.254" $IMAGEDIR/etc/yp.conf || echo "ypserver 192.168.0.254" >> $IMAGEDIR/etc/yp.conf
#	sed -i 's/compat/nis compat/g' $IMAGEDIR/etc/nsswitch.conf
#	sed -i 's/NISCLIENT=false/NISCLIENT=true/g' $IMAGEDIR/etc/default/nis

	# configure NTP
#	install files/etc_ntp.conf $IMAGEDIR/etc/ntp.conf
	
	# configure rsyslog for remote logging
#	install files/etc_rsyslog.conf $IMAGEDIR/etc/rsyslog.conf


	# cluster managment
#	chroot $IMAGEDIR apt-get -y -q install slurm-llnl munge ganglia-monitor

	# configure munge
#	MUNGE_UID=$(chroot $IMAGEDIR id -u munge)
#	MUNGE_GID=$(chroot $IMAGEDIR id -g munge)
#
#	if [ -f /etc/munge/munge.key ];then
#		echo "MUNGE key found"
#	else
#		echo "Create MUNGE key"
#		create-munge-key
#	fi
#	install --mode=600 --group=$MUNGE_GID --owner=$MUNGE_UID  /etc/munge/munge.key $IMAGEDIR/etc/munge/
	
	# configure slurm
#	[ -f /etc/slurm-llnl/slurm.conf ] && cp -v /etc/slurm-llnl/slurm.conf $IMAGEDIR/etc/slurm-llnl/

	# configure ganglia gmond
#	install files/etc_ganglia_gmond.conf $IMAGEDIR/etc/ganglia/gmond.conf

	# gromacs
#	chroot $IMAGEDIR apt-get -y -q install gromacs-openmpi gromacs-data gromacs-dev

	# development 
#	chroot $IMAGEDIR apt-get -y -q install build-essential make  gfortran libopenmpi-dev
	
	# python packages
#	chroot $IMAGEDIR apt-get -y -q install python-scipy ipython python-zmq
	
	msg "umount chroot image"
	umount_chroot_image $IMAGEDIR


	# copy encrypted root password to chroot
	usermod -R $IMAGEDIR -p $(grep root /etc/shadow|awk -F: '{ print $2 }') root
	pwck -R $IMAGEDIR -s
	grpck -R $IMAGEDIR -s
	echo "" >  $IMAGEDIR/etc/hostname
	
	# prepare PXE
	INITRD=$(ls $IMAGEDIR/boot/initrd.img*amd64|sort -r|head -1) # copy newest
	VMINUZ=$(ls $IMAGEDIR/boot/vmlinuz*amd64|sort -r|head -1) # copy newest
	#cp -v $INITRD /srv/tftp/kernel/$(basename ${INITRD}) 
	#cp -v $VMINUZ /srv/tftp/kernel/$(basename ${VMINUZ}) 
	#cp -v $IMAGEDIR/boot/initrd.img-3.2.0-4-amd64 /srv/tftp/initrd.img-3.2.0-4-amd64 

	# copy pre-generated host-keys to the chroot
	if [ -d files/ssh ]; then
	    install --mode=644 files/ssh/*key.pub $IMAGEDIR/etc/ssh/
	    install --mode=600 files/ssh/*key $IMAGEDIR/etc/ssh/
	fi

	echo "### Done."

else
	echo "Usage: $0 -d <nfsrootdir> -b"
	exit 1
fi


