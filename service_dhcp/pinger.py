import time, json
import threading
import subprocess

#
# This script calls 'ping' system command, hence Debian package 'iputils-ping' is needed.
# I've tested ping3 library, however it has problems with non-existent arp entries -
#      whole script has to be restarted for the app to "see" new entry.
#      Should be fixed in free time ;)
#


delays = []

def ReadFile(fileName: str) -> str:
    with open(fileName, "rt") as f:
        return f.read()

def Ping(ip: str) -> bool:
    res = subprocess.call(['ping','-c','1','-w','1', ip], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    return res == 0

def PingWorker(id: int, ip: str):
    while True:

        delay = Ping(ip)
        delays[id][1] = delay

        time.sleep(1)

if __name__ == "__main__":

   
    #
    # Read hardware description file
    #
    hardware = json.loads(ReadFile("machines.json"))

    threads = []
    id = 0
    for mach in hardware["machines"]:
        ip = mach["ip"]

        delays.append([ip, None])
        t = threading.Thread(target=PingWorker, args=(id, ip))
        threads.append(t)
        t.start()
        id += 1

    print("Pinging threads created...")

    while True:

        print("===============")
        for i in range(0, id):
            print(f"{delays[i][0]} {delays[i][1]}")

        time.sleep(1)
