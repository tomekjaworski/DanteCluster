using System;
using System.Threading.Tasks;
using Renci.SshNet;

namespace Ctrl
{
    public static class RenciSshClientExtensions
    {
        public static Task<string> RunCommandAsync(this SshClient client, string command)
        {
            SshCommand cmd = client.CreateCommand(command);

            return Task<String>.Factory.FromAsync((callback, state) => cmd.BeginExecute(callback, state),
                cmd.EndExecute, null);
        }

        public static Task<string> RunCommandAsync(this SshClient client, string command, int timeout)
        {
            SshCommand cmd = client.CreateCommand(command);
            cmd.CommandTimeout = TimeSpan.FromMilliseconds(timeout);

            return Task<String>.Factory.FromAsync((callback, state) => cmd.BeginExecute(callback, state),
                cmd.EndExecute, null);
        }
    }
}