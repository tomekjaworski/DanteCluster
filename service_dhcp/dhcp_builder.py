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
    # Prepare etherware scripts for all nodes
    #

    for machine in hardware["machines"]:
        content = f"""#!/bin/bash
#
# Wake UP host {machine['ip']} with hardware addres {machine['hardware']}
#

echo Waking up node {machine['ip']} with HWAddr {machine['hardware']}...
etherwake -i inner {machine['hardware']}
echo Done.

"""
        fname = "wake_" + machine["hostname"] + ".sh"
        with open(fname, "wt") as f:
            f.write(content)

        SetExecutable(fname)


    print("Done.")

