using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshNet;

namespace Ctrl
{
    public class NodeHealthMonitor
    {
        private class InternalStateDescriptor
        {
            public MonitorState state;
            public Dictionary<string, string> cpuInfo;
            public ulong statsNodeTimestamp;
            public double machineUptime;
            public double cpuTotalIdleTime;
            public double load1min;
            public double load5min;
            public double load15min;
            public int runningProcesses;
            public int availableProcesses;
            public int lastPid;
            public ulong[] cpuAllStatistics;
            public ulong memoryTotal;
            public ulong memoryUsed;
            public ulong memoryFree;
            public ulong memorySwapTotal;
            public ulong memorySwapUsed;
            public ulong memorySwapFree;

            public InternalStateDescriptor()
            {
                this.state = MonitorState.Created;
                this.cpuInfo = new Dictionary<string, string>();

            }
        }

        private SshClient ssh;
        private NodeDescriptor descriptor;
        private InternalStateDescriptor isd;

        public IPAddress IP => this.descriptor.IP;

        public NodeHealthMonitor(NodeDescriptor descriptor)
        {
            this.descriptor = descriptor;
            this.isd = new InternalStateDescriptor();
        }


        public async Task RunMonitorAsync(CancellationToken ct)
        {

            while (!ct.IsCancellationRequested)
            {
                Ping pingService = new Ping();
                int successCounter = 0;

                lock (this.isd)
                    this.isd.state = MonitorState.WaitingForPingResponses;

                int npings = 5;
                int timeout = 2000;
                // wait for N consecutive ping replies
                while (successCounter < npings && !ct.IsCancellationRequested) //todo config
                {
                    PingReply pingReply = await pingService.SendPingAsync(this.IP, timeout); // todo config
                    Console.WriteLine(
                        $"Pinging {this.IP}; result={pingReply.Status}, RTT={pingReply.RoundtripTime} ms");

                    if (pingReply.Status == IPStatus.Success)
                    {
                        successCounter++;
                        await Task.Delay(1000, ct);
                    }
                    else
                    {
                        successCounter = 0;
                    }
                }

                if (ct.IsCancellationRequested)
                    break;

                //
                // ok, the node is responding to pings, now try to connect via SSH
                //

                //TODO: replace login/password authorization method with public keys; to be done at the final stages of cluster configuration process
                AuthenticationMethod[] auth = new AuthenticationMethod[]
                {
                    new PasswordAuthenticationMethod("testlogin", "testpassword"),
                };

                lock (this.isd)
                    this.isd.state = MonitorState.EstablishingSSHConnection;

                Console.WriteLine($"Connecting to SSH server {this.IP}:22...");
                ConnectionInfo ci = new ConnectionInfo(this.IP.ToString(), 22, auth[0].Username, auth);
                SshClient ssh = new SshClient(ci);

                await Task.Run(() => ssh.Connect(), ct);

                // ===========================
                // Now we are connected and can start monitoring the node in two stages:
                // 1. Get information once per connection
                // 2. Get informations in a loop

                //
                // Test host name
                string commandResponse = (await ssh.RunCommandAsync("hostname", 5000)).Trim();
                bool hostnameOk = commandResponse == this.descriptor.Hostname;
                Console.WriteLine(
                    $"HOSTNAME: expected={this.descriptor.Hostname}; recieved={commandResponse}; correct={(hostnameOk ? "YES" : "NO")}");

                //
                // Get CPU information
                try
                {
                    Regex lscpuRegex = new Regex("^(?<key>[^:]+?)[:](?<value>.+)$",
                        RegexOptions.Compiled | RegexOptions.Singleline);
                    commandResponse = (await ssh.RunCommandAsync("lscpu", 5000));

                    Dictionary<string, string> cpuinfo = commandResponse
                        .Split("\n", StringSplitOptions.RemoveEmptyEntries)
                        .Select(row => lscpuRegex.Match(row.Trim()))
                        .Where(m => m.Success)
                        .Select(m => new {Key = m.Groups["key"].Value.Trim(), Value = m.Groups["value"].Value.Trim()})
                        .ToDictionary(v => v.Key, v => v.Value);
                    lock (this.isd)
                        this.isd.cpuInfo = cpuinfo;


                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }


                //
                // Unix timestamp
                try
                {
                    commandResponse = (await ssh.RunCommandAsync("date +%s", 5000)).Trim();
                    lock (this.isd)
                        this.isd.statsNodeTimestamp = UInt64.Parse(commandResponse.Trim());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                //
                // /proc/stat
                try
                {
                    commandResponse = (await ssh.RunCommandAsync("cat /proc/stat", 5000)).Trim();
                    Regex procStatRegex = new Regex("^(?<key>[a-zA-Z0-9]+)[ ](?<value>.+)$",
                        RegexOptions.Compiled | RegexOptions.Singleline);

                    Dictionary<string, UInt64[]> dict = commandResponse
                        .Split("\n", StringSplitOptions.RemoveEmptyEntries)
                        .Select(row => procStatRegex.Match(row.Trim()))
                        .Where(m => m.Success)
                        .ToDictionary(
                            x => x.Groups["key"].Value.Trim(),
                            v => v.Groups["value"].Value
                                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                .Select(UInt64.Parse).ToArray()
                        );

                    lock (this.isd)
                    {
                        this.isd.cpuAllStatistics = dict["cpu"];
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                // /proc/loadavg
                try
                {
                    commandResponse = (await ssh.RunCommandAsync("cat /proc/loadavg", 5000)).Trim();
                    Regex loadavgRegex = new Regex(
                        "^(?<l1>[0-9]+[.][0-9]+)\\s*(?<l5>[0-9]+[.][0-9]+)\\s*(?<l15>[0-9]+[.][0-9]+)\\s*(?<rp>[0-9]+)[/]*(?<trp>[0-9]+)\\s*(?<lastpid>[0-9]+)$",
                        RegexOptions.Compiled | RegexOptions.Singleline);

                    Match m = loadavgRegex.Match(commandResponse);

                    lock (this.isd)
                    {
                        this.isd.load1min = double.Parse(m.Groups["l1"].Value, CultureInfo.InvariantCulture);
                        this.isd.load5min = double.Parse(m.Groups["l5"].Value, CultureInfo.InvariantCulture);
                        this.isd.load15min = double.Parse(m.Groups["l15"].Value, CultureInfo.InvariantCulture);
                        this.isd.runningProcesses = int.Parse(m.Groups["rp"].Value);
                        this.isd.availableProcesses = int.Parse(m.Groups["trp"].Value);
                        this.isd.lastPid = int.Parse(m.Groups["lastpid"].Value);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }


                //
                // /proc/uptime
                try
                {

                    // 
                    commandResponse = (await ssh.RunCommandAsync("cat /proc/uptime", 5000)).Trim();

                    double[] values = commandResponse
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => double.Parse(x, CultureInfo.InvariantCulture)).ToArray();

                    lock (this.isd)
                    {
                        this.isd.machineUptime = values[0];
                        this.isd.cpuTotalIdleTime = values[1];
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                //
                // Get virtual memory statistics
                try
                {

                    commandResponse = (await ssh.RunCommandAsync("vmstat -sSB", 5000)).Trim();

                    Dictionary<string, UInt64> dict = commandResponse
                        .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .ToDictionary(
                            (string key) =>
                            {
                                key = key.Substring(key.IndexOf(' ') + 1).Trim().ToLower();
                                if (key.StartsWith("b "))
                                    key = key.Substring(2).Trim();
                                return key;
                            },
                            x => UInt64.Parse(x.Substring(0, x.IndexOf(' ')).Trim()));

                    lock (this.isd)
                    {
                        this.isd.memoryTotal = dict["total memory"];
                        this.isd.memoryUsed = dict["used memory"];
                        this.isd.memoryFree = dict["free memory"];

                        this.isd.memorySwapTotal = dict["total swap"];
                        this.isd.memorySwapUsed = dict["used swap"];
                        this.isd.memorySwapFree = dict["free swap"];
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }


/*
                //
                // Get CPU dynamic state information
                Regex hhmmssRegex = new Regex("^[0-9]{2}[:][0-9]{2}[:][0-9]{2}",
                    RegexOptions.Compiled | RegexOptions.Singleline);

                commandResponse = (await ssh.RunCommandAsync("mpstat -P ALL -u -I SUM", 5000)).Trim();
                commandResponse = @"
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
";
                string[][] rows = commandResponse.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Where(r => hhmmssRegex.IsMatch(r))
                    .Select(r => r.Substring(2 + 1 + 2 + 1 + 2).Trim())
                    .Select(r => r.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    .ToArray();
                    */

                Console.WriteLine("????");

            }

            lock (this)
                this.isd.state = MonitorState.Terminated;

        }

    }

    public enum MonitorState
    {
        WaitingForPingResponses,
        Created,
        EstablishingSSHConnection,
        Terminated
    }
}