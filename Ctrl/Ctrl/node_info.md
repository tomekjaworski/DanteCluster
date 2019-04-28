
##### Table of Contents  

* [HDD temperature](#hddtemp)  
* [CPU Information](#cpuinfo)  

<a name="hddtemp"/>

### HDD temperature
```

apt install hddtemp
```


```
hddtemp /dev/sd?
```

Typical output:
```
WARNING: Drive /dev/sda doesn't seem to have a temperature sensor.
WARNING: This doesn't mean it hasn't got one.
WARNING: If you are sure it has one, please contact me (hddtemp@guzu.net).
WARNING: See --help, --debug and --drivebase options.
/dev/sda: Samsung SSD 850 EVO 500G B              @:  no sensor
/dev/sdb: WDC WD20EFRX-68EUZN0: 24°C
/dev/sdc: WDC WD20EFRX-68EUZN0: 23°C
/dev/sdd: ADATA SP900NS38: 28°C
```

-----------

<a name="cpuinfo"/>

### CPU Information

##### lscpu
```
lscpu
```

Typical output:
```
Architecture:          x86_64
CPU op-mode(s):        32-bit, 64-bit
Byte Order:            Little Endian
CPU(s):                4
On-line CPU(s) list:   0-3
Thread(s) per core:    1
Core(s) per socket:    4
Socket(s):             1
NUMA node(s):          1
Vendor ID:             GenuineIntel
CPU family:            6
Model:                 60
Model name:            Intel(R) Core(TM) i5-4670 CPU @ 3.40GHz
Stepping:              3
CPU MHz:               839.831
CPU max MHz:           3800.0000
CPU min MHz:           800.0000
BogoMIPS:              6784.24
Virtualization:        VT-x
L1d cache:             32K
L1i cache:             32K
L2 cache:              256K
L3 cache:              6144K
NUMA node0 CPU(s):     0-3
Flags:                 fpu vme de pse tsc msr pae mce cx8 apic sep mtrr pge mca cmov pat pse36 clflush dts acpi mmx fxsr sse sse2 ss ht tm pbe syscall nx pdpe1gb rdtscp lm constant_tsc arch_perfmon pebs bts rep_good nopl xtopology nonstop_tsc aperfmperf pni pclmulqdq dtes64 monitor ds_cpl vmx smx est tm2 ssse3 sdbg fma cx16 xtpr pdcm pcid sse4_1 sse4_2 x2apic movbe popcnt tsc_deadline_timer aes xsave avx f16c rdrand lahf_lm abm epb invpcid_single kaiser tpr_shadow vnmi flexpriority ept vpid fsgsbase tsc_adjust bmi1 hle avx2 smep bmi2 erms invpcid rtm xsaveopt dtherm ida arat pln pts
```


##### mpstat
```
apt install sysstat
```

```
mpstat -P ALL -u -I SUM
```

Typical output:
```
Linux 4.9.0-8-amd64 (node11)    28/04/19        _x86_64_        (4 CPU)

17:30:18     CPU    %usr   %nice    %sys %iowait    %irq   %soft  %steal  %guest  %gnice   %idle
17:30:18     all    0.00    0.00    0.01    0.02    0.00    0.01    0.00    0.00    0.00   99.96
17:30:18       0    0.00    0.00    0.01    0.00    0.00    0.04    0.00    0.00    0.00   99.95
17:30:18       1    0.00    0.00    0.01    0.06    0.00    0.00    0.00    0.00    0.00   99.93
17:30:18       2    0.00    0.00    0.01    0.01    0.00    0.00    0.00    0.00    0.00   99.98
17:30:18       3    0.00    0.00    0.01    0.01    0.00    0.00    0.00    0.00    0.00   99.98

17:30:18     CPU    intr/s
17:30:18     all     39.26
17:30:18       0     32.71
17:30:18       1      3.47
17:30:18       2      1.67
17:30:18       3      1.41
```

### System Information

##### ps

```
ps -aux
```

Typical output:
```
USER       PID %CPU %MEM    VSZ   RSS TTY      STAT START   TIME COMMAND
root         1  0.1  0.0  58396  6936 ?        Ss   13:12   0:01 /sbin/init
root         2  0.0  0.0      0     0 ?        S    13:12   0:00 [kthreadd]
root         3  0.0  0.0      0     0 ?        S    13:12   0:00 [ksoftirqd/0]
root         5  0.0  0.0      0     0 ?        S<   13:12   0:00 [kworker/0:0H]
root         7  0.0  0.0      0     0 ?        S    13:12   0:00 [rcu_sched]
root         8  0.0  0.0      0     0 ?        S    13:12   0:00 [rcu_bh]
root         9  0.0  0.0      0     0 ?        S    13:12   0:00 [migration/0]
root        10  0.0  0.0      0     0 ?        S<   13:12   0:00 [lru-add-drain]
root        11  0.0  0.0      0     0 ?        S    13:12   0:00 [watchdog/0]
root        12  0.0  0.0      0     0 ?        S    13:12   0:00 [cpuhp/0]
root        13  0.0  0.0      0     0 ?        S    13:12   0:00 [cpuhp/1]
root        14  0.0  0.0      0     0 ?        S    13:12   0:00 [watchdog/1]
root        15  0.0  0.0      0     0 ?        S    13:12   0:00 [migration/1]
root        16  0.0  0.0      0     0 ?        S    13:12   0:00 [ksoftirqd/1]
root        18  0.0  0.0      0     0 ?        S<   13:12   0:00 [kworker/1:0H]
root        19  0.0  0.0      0     0 ?        S    13:12   0:00 [cpuhp/2]
root        20  0.0  0.0      0     0 ?        S    13:12   0:00 [watchdog/2]
root        21  0.0  0.0      0     0 ?        S    13:12   0:00 [migration/2]
root        22  0.0  0.0      0     0 ?        S    13:12   0:00 [ksoftirqd/2]
root        24  0.0  0.0      0     0 ?        S<   13:12   0:00 [kworker/2:0H]
root        25  0.0  0.0      0     0 ?        S    13:12   0:00 [cpuhp/3]
root        26  0.0  0.0      0     0 ?        S    13:12   0:00 [watchdog/3]
root        27  0.0  0.0      0     0 ?        S    13:12   0:00 [migration/3]
root        28  0.0  0.0      0     0 ?        S    13:12   0:00 [ksoftirqd/3]
root        30  0.0  0.0      0     0 ?        S<   13:12   0:00 [kworker/3:0H]
root        31  0.0  0.0      0     0 ?        S    13:12   0:00 [kdevtmpfs]
root        32  0.0  0.0      0     0 ?        S<   13:12   0:00 [netns]
root        33  0.0  0.0      0     0 ?        S    13:12   0:00 [khungtaskd]
root        34  0.0  0.0      0     0 ?        S    13:12   0:00 [oom_reaper]
root        35  0.0  0.0      0     0 ?        S<   13:12   0:00 [writeback]
root        36  0.0  0.0      0     0 ?        S    13:12   0:00 [kcompactd0]
root        38  0.0  0.0      0     0 ?        SN   13:12   0:00 [ksmd]
root        39  0.0  0.0      0     0 ?        SN   13:12   0:00 [khugepaged]
root        40  0.0  0.0      0     0 ?        S<   13:12   0:00 [crypto]
root        41  0.0  0.0      0     0 ?        S<   13:12   0:00 [kintegrityd]
root        42  0.0  0.0      0     0 ?        S<   13:12   0:00 [bioset]
root        43  0.0  0.0      0     0 ?        S<   13:12   0:00 [kblockd]
root        44  0.0  0.0      0     0 ?        S    13:12   0:00 [kworker/3:1]
root        45  0.0  0.0      0     0 ?        S    13:12   0:00 [kworker/0:1]
root        46  0.0  0.0      0     0 ?        S    13:12   0:00 [kworker/1:1]
root        47  0.0  0.0      0     0 ?        S<   13:12   0:00 [devfreq_wq]
root        48  0.0  0.0      0     0 ?        S<   13:12   0:00 [watchdogd]
root        49  0.0  0.0      0     0 ?        S    13:12   0:00 [kswapd0]
root        50  0.0  0.0      0     0 ?        S<   13:12   0:00 [vmstat]
root        62  0.0  0.0      0     0 ?        S<   13:12   0:00 [kthrotld]
root        63  0.0  0.0      0     0 ?        S    13:12   0:00 [kworker/3:2]
root        64  0.0  0.0      0     0 ?        S<   13:12   0:00 [ipv6_addrconf]
root        93  0.0  0.0      0     0 ?        S<   13:12   0:00 [acpi_thermal_pm]
root       104  0.0  0.0      0     0 ?        S<   13:12   0:00 [ata_sff]
root       105  0.0  0.0      0     0 ?        S    13:12   0:00 [scsi_eh_0]
root       106  0.0  0.0      0     0 ?        S<   13:12   0:00 [scsi_tmf_0]
root       107  0.0  0.0      0     0 ?        S    13:12   0:00 [scsi_eh_1]
root       108  0.0  0.0      0     0 ?        S<   13:12   0:00 [scsi_tmf_1]
root       109  0.0  0.0      0     0 ?        S    13:12   0:00 [scsi_eh_2]
root       110  0.0  0.0      0     0 ?        S<   13:12   0:00 [scsi_tmf_2]
root       111  0.0  0.0      0     0 ?        S    13:12   0:00 [scsi_eh_3]
root       112  0.0  0.0      0     0 ?        S<   13:12   0:00 [scsi_tmf_3]
root       113  0.0  0.0      0     0 ?        S    13:12   0:00 [scsi_eh_4]
root       114  0.0  0.0      0     0 ?        S<   13:12   0:00 [scsi_tmf_4]
root       115  0.0  0.0      0     0 ?        S    13:12   0:00 [scsi_eh_5]
root       116  0.0  0.0      0     0 ?        S<   13:12   0:00 [scsi_tmf_5]
root       120  0.0  0.0      0     0 ?        S    13:12   0:00 [kworker/u8:4]
root       150  0.0  0.0      0     0 ?        S<   13:12   0:00 [bioset]
root       151  0.0  0.0      0     0 ?        S    13:12   0:00 [kworker/0:2]
root       152  0.0  0.0      0     0 ?        S<   13:12   0:00 [bioset]
root       155  0.0  0.0      0     0 ?        S<   13:12   0:00 [kworker/3:1H]
root       156  0.0  0.0      0     0 ?        S<   13:12   0:00 [kworker/1:1H]
root       157  0.0  0.0      0     0 ?        S<   13:12   0:00 [kworker/0:1H]
root       169  0.0  0.0      0     0 ?        S<   13:12   0:00 [rpciod]
root       170  0.0  0.0      0     0 ?        S<   13:12   0:00 [xprtiod]
root       175  0.0  0.0      0     0 ?        S<   13:12   0:00 [nfsiod]
root       197  0.0  0.0      0     0 ?        S<   13:12   0:00 [kworker/2:1H]
root       242  0.0  0.0  47128  5068 ?        Ss   13:12   0:00 /lib/systemd/systemd-journald
root       255  0.0  0.0      0     0 ?        S    13:12   0:00 [kauditd]
root       267  0.0  0.0      0     0 ?        S    13:12   0:00 [kworker/2:2]
root       268  0.0  0.0      0     0 ?        S    13:12   0:00 [kworker/2:3]
root       272  0.0  0.0  47176  4556 ?        Ss   13:12   0:00 /lib/systemd/systemd-udevd
root       276  0.0  0.0  51928  3592 ?        Ss   13:12   0:00 /sbin/rpcbind -f -w
systemd+   277  0.0  0.0 128324  4076 ?        Ssl  13:12   0:00 /lib/systemd/systemd-timesyncd
root       296  0.0  0.0      0     0 ?        S    13:12   0:00 [irq/29-mei_me]
root       304  0.0  0.0      0     0 ?        S    13:12   0:00 [i915/signal:0]
root       305  0.0  0.0      0     0 ?        S    13:12   0:00 [i915/signal:1]
root       306  0.0  0.0      0     0 ?        S    13:12   0:00 [i915/signal:2]
root       307  0.0  0.0      0     0 ?        S    13:12   0:00 [i915/signal:4]
root       337  0.0  0.0  15560  2112 tty1     Ss+  13:12   0:00 /sbin/agetty --noclear tty1 linux
root       338  0.0  0.0  15560  2080 tty2     Ss+  13:12   0:00 /sbin/agetty --noclear tty2 linux
root       339  0.0  0.0  15560  2056 tty3     Ss+  13:12   0:00 /sbin/agetty --noclear tty3 linux
root       343  0.0  0.0  15560  2004 tty5     Ss+  13:12   0:00 /sbin/agetty --noclear tty5 linux
root       344  0.0  0.0  15560  2092 tty4     Ss+  13:12   0:00 /sbin/agetty --noclear tty4 linux
root       347  0.0  0.0  72028  5516 ?        Ss   13:12   0:00 /usr/sbin/sshd -D
root       349  0.0  0.0  15560  1984 tty6     Ss+  13:12   0:00 /sbin/agetty --noclear tty6 linux
root       365  0.0  0.0  94800  6688 ?        Ss   13:13   0:00 sshd: root@pts/0
root       371  0.0  0.0  21688  5036 pts/0    Ss+  13:13   0:00 -bash
root       397  0.0  0.0      0     0 ?        S    13:19   0:00 [kworker/u8:0]
root       429  0.0  0.0      0     0 ?        S    13:22   0:00 [kworker/1:2]
root       442  0.0  0.0      0     0 ?        S    13:25   0:00 [kworker/u8:1]
root       443  0.0  0.0  94800  6592 ?        Ss   13:27   0:00 sshd: root@pts/1
root       451  0.0  0.0  21688  5024 pts/1    Ss   13:27   0:00 -bash
root       463  0.0  0.0      0     0 ?        S    13:28   0:00 [kworker/1:0]
root       465  0.0  0.0  40100  3332 pts/1    R+   13:28   0:00 ps -aux
```

##### vmstat

```
vmstat -s
```

Typical output:
```
     16317720 K total memory
        76776 K used memory
        30460 K active memory
        30788 K inactive memory
     16178600 K free memory
            0 K buffer memory
        62344 K swap cache
            0 K total swap
            0 K used swap
            0 K free swap
           69 non-nice user cpu ticks
            0 nice user cpu ticks
          412 system cpu ticks
       409223 idle cpu ticks
          426 IO-wait cpu ticks
            0 IRQ cpu ticks
           67 softirq cpu ticks
            0 stolen cpu ticks
         3196 pages paged in
            0 pages paged out
            0 pages swapped in
            0 pages swapped out
        83059 interrupts
       151501 CPU context switches
   1556449942 boot time
          469 forks
```

