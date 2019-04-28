using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection.PortableExecutable;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Ctrl
{

    public class NodeStatus
    {
        public IPAddress IP { get; }

        public PingStatus PingStatus { get;  }

        public NodeStatus(IPAddress ip)
        {
            this.PingStatus = new PingStatus(ip);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
           
            ClusterController ctrl = new ClusterController(@"..\..\..\..\..\service_dhcp\machines.json");
            ctrl.Run();
        }
    }

    public class ClusterController
    {
        private ClusterConfiguration config;
        private CancellationTokenSource cts;
        private Dictionary<IPAddress, NodeStatus> nodeStatuses;

        public ClusterController(string configFileName)
        {
            string json = File.ReadAllText(configFileName);

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new IPAddressConverter());
            settings.Converters.Add(new JsonHardwareAddressConverter());
            settings.Formatting = Formatting.Indented;

            this.config = JsonConvert.DeserializeObject<ClusterConfiguration>(json, settings);

            this.cts = new CancellationTokenSource();
            this.nodeStatuses = new Dictionary<IPAddress, NodeStatus>();
            foreach (Node node in this.config.Nodes)
                this.nodeStatuses.Add(node.IP, new NodeStatus(node.IP));
        }

        private async Task Pinger()
        {
            List<Task<PingReply>> tasks = new List<Task<PingReply>>();

            while (!this.cts.IsCancellationRequested)
            {
                tasks.Clear();
                foreach (Node node in this.config.Nodes)
                {
                    Ping ping = new Ping();
                    Task<PingReply> pr = ping.SendPingAsync(node.IP, 2000);
                    tasks.Add(pr);
                }

                PingReply[] replies = await Task.WhenAll(tasks);

                foreach (PingReply reply in replies)
                    this.nodeStatuses[reply.Address].PingStatus.SetICMPStatus(reply.Status);

            }
        }

        public void Run()
        {


            Task.Run(() => Pinger());


            while (true)
                Thread.Sleep(1000);
        }
    }
}
