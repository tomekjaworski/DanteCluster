import jinja2
import json
import random
import os


def ReadFile(fileName: str) -> str:
    with open(fileName, "rt") as f:
        return f.read()

def SetExecutable(fname: str):
    mode = os.stat(fname).st_mode
    mode |= 0o111
    os.chmod(fname, mode)


if __name__ == "__main__":


    #
    # Read hardware description file
    #
    hardware = json.loads(ReadFile("machines.json"))
    os.makedirs("scripts",exist_ok=True)


    for machine in hardware["machines"]:
        lease_id = random.randint(0, 0xffffffff)
        machine["lease_id"] = "lease-%08x" % lease_id

        print("Machine %s -> ip=%s" % (machine['hardware'], machine['ip']))

    #
    # Create DHCP server configuration file by template (dhcpd.conf)
    #

    template = jinja2.Template(ReadFile("dhcpd.conf.j2template"))
    output = template.render(machines=hardware["machines"])

    with open("dhcpd.conf", "wt") as f:
        f.write(output)

    #
    # Prepare wakeup and poweroff scripts for all nodes
    #


    wakeup_all = f"""#!/bin/bash
#
# Wake UP ALL hosts
#

"""

    poweroff_all = f"""#!/bin/bash
#
# Powering OFF ALL hosts
#

"""


    for machine in hardware["machines"]:
        wakeup_single = f"""#!/bin/bash
#
# Wake UP host {machine['ip']} with hardware addres {machine['hardware']}
#

echo Waking up node {machine['ip']} with HWAddr {machine['hardware']}...
etherwake -i inner {machine['hardware']}
echo Done.

"""
        wakeup_all += f"""echo Waking up node {machine['ip']} with HWAddr {machine['hardware']}...
etherwake -i inner {machine['hardware']}
"""
        poweroff_all += f"""echo Powering down node {machine['ip']} with HWAddr {machine['hardware']}...
ssh {machine['ip']} "shutdown -P now" &
"""        

        fname = "scripts/wake_" + machine["hostname"] + ".sh"
        with open(fname, "wt") as f:
            f.write(wakeup_single)

        SetExecutable(fname)


        poweroff_single = f"""#!/bin/bash
#
# Power off host {machine['ip']} with hardware addres {machine['hardware']}
#

echo Powering down node {machine['ip']} with HWAddr {machine['hardware']}...
ssh {machine['ip']} "shutdown -P now" &
echo Done.

"""
        fname = "scripts/poweroff_" + machine["hostname"] + ".sh"
        with open(fname, "wt") as f:
            f.write(poweroff_single)

        SetExecutable(fname)

    #
    # Save WHOLE-CLUSTER operations...
    #

    with open("scripts/wake_all.sh", "wt") as f:
        f.write(wakeup_all)
    with open("scripts/poweroff_all.sh", "wt") as f:
        f.write(poweroff_all)

    SetExecutable("scripts/wake_all.sh")
    SetExecutable("scripts/poweroff_all.sh")


    print("Done.")

