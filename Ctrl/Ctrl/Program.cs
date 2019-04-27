using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Reflection.PortableExecutable;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Ctrl
{
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

        public ClusterController(string configFileName)
        {
            string json = File.ReadAllText(configFileName);

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new IPAddressConverter());
            settings.Converters.Add(new JsonHardwareAddressConverter());
            settings.Formatting = Formatting.Indented;

            this.config = JsonConvert.DeserializeObject<ClusterConfiguration>(json, settings);
        }

        private async Task Pinger()
        {
            List<Task<PingReply>> tasks = new List<Task<PingReply>>();
            foreach (Node node in this.config.Nodes)
            {
                Ping ping = new Ping();
                Task<PingReply> pr = ping.SendPingAsync(node.IP, 2000);
                tasks.Add(pr);
            }

      

            PingReply[] replies = await Task.WhenAll(tasks);

        }

        public void Run()
        {

            Task.Run(() => Pinger());


            while (true)
            {
                Thread.Sleep(1000);
            }
        }
    }
}
