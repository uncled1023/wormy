using System;
using System.Collections.Generic;
using NHibernate.Linq;
using wormy.Database;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;

namespace wormy.Modules
{
    public class ShellModule : Module
    {
        public override string Name { get { return "shell"; } }
        public override string Description { get { return "Allows you to execute shell commands."; } }
        public Dictionary<string, string[]> Masks { get; set; }

        public ShellModule(NetworkManager network) : base(network)
        {
            RegisterAdminCommand("$", (args, e) => Task.Factory.StartNew(() =>
                {
                    var startInfo = new ProcessStartInfo("/usr/bin/env", "bash -c \"" +
                        string.Join(" ", args).Replace("\"", "\\\"") +
                        "\"");
                    startInfo.RedirectStandardOutput = true;
                    startInfo.RedirectStandardError = true;
                    startInfo.UseShellExecute = false;
                    startInfo.CreateNoWindow = true;
                    var process = Process.Start(startInfo);
                    while (!process.StandardOutput.EndOfStream)
                    {
                        var line = process.StandardOutput.ReadLine();
                        if (!string.IsNullOrEmpty(line.Trim()))
                            network.Client.SendMessage(line, e.PrivateMessage.Source);
                    }
                    while (!process.StandardError.EndOfStream)
                    {
                        var line = process.StandardError.ReadLine();
                        if (!string.IsNullOrEmpty(line.Trim()))
                            network.Client.SendMessage(line, e.PrivateMessage.Source);
                    }
                    process.WaitForExit();
                    if (process.ExitCode == 0)
                        network.Client.SendAction("[Done]", e.PrivateMessage.Source);
                    else
                        network.Client.SendAction(string.Format("[Done (status code {0})]", process.ExitCode), e.PrivateMessage.Source);
                }), "$ [command]: Runs a shell command.");
        }
    }
}