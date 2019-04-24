import ping3 # pyping needs to be installed manually (is not embedded)
import time, json
import threading

delays = []

def ReadFile(fileName: str) -> str:
    with open(fileName, "rt") as f:
        return f.read()

def PingWorker(id: int, ip: str):
    while True:

        delay = ping3.ping(ip, unit="ms", timeout=2)
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
