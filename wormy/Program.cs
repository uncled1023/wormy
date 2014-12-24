using System;
using System.IO;
using wormy.Database;
using System.Collections.Generic;
using System.Threading;

namespace wormy
{
    class Program
    {
        public static WormyDatabase Database { get; set; }
        public static Configuration Configuration { get; set; }

        public static void Main(string[] args)
        {
            Configuration = new Configuration();
            string configPath = "config.json";
            if (args.Length != 0)
                configPath = args[0];
            if (File.Exists(configPath))
                Configuration.Load(configPath);
            else
            {
                Configuration.Save(configPath);
                Console.WriteLine("A new {0} has been generated. Populate it and restart.", configPath);
                return;
            }
            Configuration.Save(configPath);
            Console.WriteLine("Connecting to database...");
            Database = new WormyDatabase(Configuration.Database.ConnectionString);
            Console.WriteLine("Connecting to IRC networks...");
            List<NetworkManager> networkManagers = new List<NetworkManager>();
            foreach (var n in Configuration.Networks)
            {
                var network = new NetworkManager(n);
                networkManagers.Add(network);
            }
            Thread.Yield();
            while (true)
                Thread.Sleep(1000);
        }
    }
}
