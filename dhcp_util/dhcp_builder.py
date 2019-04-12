import jinja2
import json
import random


def ReadFile(fileName: str) -> str:
    with open(fileName, "rt") as f:
        return f.read()

if __name__ == "__main__":


    template = jinja2.Template(ReadFile("dhcpd.conf.j2template"))
    hardware = json.loads(ReadFile("machines.json"))


    for machine in hardware["machines"]:
        lease_id = random.randint(0, 0xffffffff)
        machine["lease_id"] = f"lease-{lease_id:08x}"

    output = template.render(machines=hardware["machines"])

    with open("dhcpd.conf", "wt") as f:
        f.write(output)

    print("")