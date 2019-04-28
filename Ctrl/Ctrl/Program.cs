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
using Renci.SshNet;


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
        private CancellationTokenSource cts;
        private Dictionary<IPAddress, NodeHealthMonitor> nodeStatuses;

        public ClusterController(string configFileName)
        {
            string json = File.ReadAllText(configFileName);

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new IPAddressConverter());
            settings.Converters.Add(new JsonHardwareAddressConverter());
            settings.Formatting = Formatting.Indented;

            this.config = JsonConvert.DeserializeObject<ClusterConfiguration>(json, settings);

            this.cts = new CancellationTokenSource();
            this.nodeStatuses = new Dictionary<IPAddress, NodeHealthMonitor>();
            foreach (NodeDescriptor node in this.config.NodesDescriptor)
                this.nodeStatuses.Add(node.IP, new NodeHealthMonitor(node));
        }

        private void Pinger()
        {


            /*
            List<Task<PingReply>> tasks = new List<Task<PingReply>>();

            await Task.Run(() => X());

            while (!this.cts.IsCancellationRequested)
            {
                tasks.Clear();
                foreach (NodeDescriptor node in this.config.NodesDescriptor)
                {
                    Ping ping = new Ping();
                    Task<PingReply> pr = ping.SendPingAsync(node.IP, 2000);
                    tasks.Add(pr);
                }

                PingReply[] replies = await Task.WhenAll(tasks);

                foreach (PingReply reply in replies)
                    this.nodeStatuses[reply.Address].PingStatus.SetICMPStatus(reply.Status);

            }*/

        }

        void X()
        {
            Thread.Sleep(10000);
        }
        public void Run()
        {
            /*   AuthenticationMethod[] auth = new AuthenticationMethod[]
               {
                   //TODO: replace login/password authorization method with public keys; to be done at the final stages of cluster configuration process
                   new PasswordAuthenticationMethod("testlogin", "testpassword"), 
               };
               ConnectionInfo ci = new ConnectionInfo("10.24.188.189", 22, auth[0].Username, auth);

               SshClient cli = new SshClient(ci);

               cli.Connect();


               SshCommand cmd = cli.CreateCommand("sleep 10");
               cmd.CommandTimeout = TimeSpan.FromSeconds(5);

               Task.Factory.FromAsync((callback, state) => cmd.BeginExecute(callback, state), cmd.EndExecute, null);

               //output.
               */

            List<Task> tasks = new List<Task>();
            foreach (NodeHealthMonitor nhm in this.nodeStatuses.Values)
            {
                Task task = nhm.RunMonitorAsync(this.cts.Token);
                tasks.Add(task);
            }

            Task monitorTasks = Task.WhenAll(tasks.ToArray());
            monitorTasks.Wait(this.cts.Token);


            while (true)
                Thread.Sleep(1000);
        }
    }
}
