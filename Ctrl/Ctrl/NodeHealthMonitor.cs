using System;
using System.Collections.Generic;
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
        private SshClient ssh;
        private MonitorState state;
        private NodeDescriptor descriptor;
        private Dictionary<string, string> cpuInfo;

        public IPAddress IP => this.descriptor.IP;

        public PingStatus PingStatus { get; }

        public NodeHealthMonitor(NodeDescriptor descriptor)
        {
            this.descriptor = descriptor;
            this.state = MonitorState.Created;

            this.cpuInfo = new Dictionary<string, string>();
            this.PingStatus = new PingStatus(this.descriptor.IP);
        }


        public async Task RunMonitorAsync(CancellationToken ct)
        {

            while (!ct.IsCancellationRequested)
            {
                Ping pingService = new Ping();
                int successCounter = 0;

                lock (this)
                    this.state = MonitorState.WaitingForPingResponses;

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

                lock (this)
                    this.state = MonitorState.EstablishingSSHConnection;

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
                string receivedHostname = (await ssh.RunCommandAsync("hostname", 5000)).Trim();
                bool hostnameOk = receivedHostname == this.descriptor.Hostname;
                Console.WriteLine(
                    $"HOSTNAME: expected={this.descriptor.Hostname}; recieved={receivedHostname}; correct={(hostnameOk ? "YES" : "NO")}");

                //
                // Get CPU information
                Regex lscpuRegex = new Regex("^(?<key>[^:]+)?[:](?<value>.+)$",
                    RegexOptions.Compiled | RegexOptions.Singleline);
                string lscpu = (await ssh.RunCommandAsync("lscpu", 5000));

                Dictionary<string, string> cpuinfo = lscpu.Split("\n", StringSplitOptions.RemoveEmptyEntries)
                    .Select(row => lscpuRegex.Match(row.Trim()))
                    .Where(m => m.Success)
                    .Select(m => new {Key = m.Groups["key"].Value.Trim(), Value = m.Groups["value"].Value.Trim()})
                    .ToDictionary(v => v.Key, v => v.Value);
                this.cpuInfo = cpuinfo;


                //
                // Get CPU dynamic state information
                Regex hhmmssRegex = new Regex("^[0-9]{2}[:][0-9]{2}[:][0-9]{2}",
                    RegexOptions.Compiled | RegexOptions.Singleline);

                string mpstat = (await ssh.RunCommandAsync("mpstat -P ALL -u -I SUM", 5000)).Trim();
                mpstat = @"
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
                string[][] rows = mpstat.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Where(r => hhmmssRegex.IsMatch(r))
                    .Select(r => r.Substring(2 + 1 + 2 + 1 + 2).Trim())
                    .Select(r => r.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    .ToArray();


                Console.WriteLine("????");

            }

            lock (this)
                this.state = MonitorState.Terminated;

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