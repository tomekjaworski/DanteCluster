using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Renci.SshNet;
using Renci.SshNet.Common;

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

        private NodeDescriptor descriptor;
        private InternalStateDescriptor isd;
        private ILogger log;

        public IPAddress IP => this.descriptor.IP;

        public NodeHealthMonitor(NodeDescriptor descriptor)
        {
            this.descriptor = descriptor;
            this.isd = new InternalStateDescriptor();
            this.log = LogManager.GetLogger(descriptor.Hostname);
        }


        public async Task RunMonitorAsync(CancellationToken ct)
        {
            log.Info($"Starting health monitor for node {this.descriptor.IP}");

            while (!ct.IsCancellationRequested)
            {
                int successCounter = 0;

                lock (this.isd)
                    this.isd.state = MonitorState.WaitingForPingResponses;

                int pingCount = 5;
                int timeout = 2000;
                int pingInterval = 1000;
                // wait for N consecutive ping replies

                log.Info(
                    $"Waiting for {pingCount} consecutive successful pings with timeout {timeout} and interval {pingInterval}");
                while (successCounter < pingCount && !ct.IsCancellationRequested) //todo config
                {
                    try
                    {
                        Ping pingService = new Ping();
                        PingReply pingReply = await pingService.SendPingAsync(this.IP, timeout); // todo config

                        if (pingReply.Status == IPStatus.Success)
                        {
                            log.Info(
                                $"Got response form {this.IP}; result={pingReply.Status}, RTT={pingReply.RoundtripTime} ms");
                            successCounter++;
                            await Task.Delay(pingInterval, ct);
                        }
                        else
                        {
                            log.Info($"No response form {this.IP}; result={pingReply.Status}");
                            successCounter = 0;
                        }


                    }
                    catch (SystemException ex)
                    {
                        log.Error(ex, $"Error while pinging node {this.descriptor.Hostname}");
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

                IPAddress ip = IPAddress.Parse("10.10.10.10");
                ip = this.IP;
                log.Info($"Connecting node via SSH {this.IP}:22...");
                ConnectionInfo ci = new ConnectionInfo(ip.ToString(), 22, auth[0].Username, auth);

                using (SshClient ssh = new SshClient(ci))
                {

                    try
                    {
                        await Task.Run(() => ssh.Connect(), ct);
                        log.Info(
                            $"Connected. Host: {ssh.ConnectionInfo.Host}, server: {ssh.ConnectionInfo.ServerVersion}, username: {ssh.ConnectionInfo.Username}.");
                    }
                    catch (SocketException ex)
                    {
                        log.Error(ex, "Error while connecting node via SSH");
                        continue;
                    }

                    // ===========================
                    // Now we are connected and can start monitoring the node in two stages:
                    // 1. Get information once per connection
                    // 2. Get informations in a loop

                    //
                    // Test host name
                    try
                    {
                        log.Info("Running 'hostname'...");
                        string commandResponse = (await ssh.RunCommandAsync("hostname", 5000)).Trim();
                        bool hostnameOk = commandResponse == this.descriptor.Hostname;
                        string msg =
                            $"HOSTNAME: expected={this.descriptor.Hostname}; recieved={commandResponse}; correct={(hostnameOk ? "YES" : "NO")}";
                        if (hostnameOk)
                            log.Info(msg);
                        else
                            log.Warn(msg);
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex, "Error while running command");
                        continue;
                    }

                    //
                    // Get CPU information
                    try
                    {
                        Regex lscpuRegex = new Regex("^(?<key>[^:]+?)[:](?<value>.+)$",
                            RegexOptions.Compiled | RegexOptions.Singleline);

                        log.Info("Running 'lscpu'...");
                        string commandResponse = (await ssh.RunCommandAsync("lscpu", 5000));

                        Dictionary<string, string> cpuinfo = commandResponse
                            .Split("\n", StringSplitOptions.RemoveEmptyEntries)
                            .Select(row => lscpuRegex.Match(row.Trim()))
                            .Where(m => m.Success)
                            .Select(m => new
                                {Key = m.Groups["key"].Value.Trim(), Value = m.Groups["value"].Value.Trim()})
                            .ToDictionary(v => v.Key, v => v.Value);

                        lock (this.isd)
                            this.isd.cpuInfo = cpuinfo;
                        log.Trace($"Got {cpuinfo.Count} entries");

                    }
                    catch (Exception ex)
                    {
                        log.Error(ex, "Error while running command");
                        continue;
                    }


                    //
                    // Unix timestamp
                    try
                    {
                        log.Info("Running 'date +%s'...");
                        string commandResponse = (await ssh.RunCommandAsync("date +%s", 5000)).Trim();
                        lock (this.isd)
                            this.isd.statsNodeTimestamp = UInt64.Parse(commandResponse.Trim());
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex, "Error while running command");
                        continue;
                    }

                    log.Info("Entering node health monitoringl loop...");
                    while (!ct.IsCancellationRequested)
                    {

                        DateTime begin = DateTime.Now;

                        //
                        // /proc/stat
                        try
                        {
                            string commandResponse = (await ssh.RunCommandAsync("cat /proc/stat", 5000)).Trim();
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
                            log.Error(ex, "Error while running command 'cat /proc/stat'");
                            break;
                        }

                        // /proc/loadavg
                        try
                        {
                            string commandResponse = (await ssh.RunCommandAsync("cat /proc/loadavg", 5000)).Trim();
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
                            log.Error(ex, "Error while running command 'cat /proc/loadavg'");
                            break;
                        }


                        //
                        // /proc/uptime
                        try
                        {

                            // 
                            string commandResponse = (await ssh.RunCommandAsync("cat /proc/uptime", 5000)).Trim();

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
                            log.Error(ex, "Error while running command 'cat /proc/uptime'");
                            break;
                        }

                        //
                        // Get virtual memory statistics
                        try
                        {

                            string commandResponse = (await ssh.RunCommandAsync("vmstat -sSB", 5000)).Trim();

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
                            log.Error(ex, "Error while running command 'vmstat -sSB'");
                            break;
                        }


                        // wait up to 10 seconds (count in previous SSH calls)
                        TimeSpan ts = DateTime.Now - begin;
                        int delta = (int) Math.Max(0, 10 * 1000 - ts.TotalMilliseconds);
                        await Task.Delay(delta);


                    }

                    log.Info($"Exited node health monitoring loop (ct.IsCancellationRequested={ct.IsCancellationRequested}");
                }

            }

            log.Info($"Health monitor for node {this.descriptor.IP} stopped.");

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