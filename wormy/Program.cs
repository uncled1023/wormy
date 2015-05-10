using System;
using System.IO;
using wormy.Database;
using System.Collections.Generic;
using System.Threading;
using RedditSharp;
using System.Threading.Tasks;
using NHibernate.Linq;
using System.Linq;

namespace wormy
{
    class Program
    {
        public static WormyDatabase Database { get; set; }
        public static Configuration Configuration { get; set; }
        public static List<NetworkManager> NetworkManagers { get; set; }

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
            NetworkManagers = new List<NetworkManager>();
            foreach (var n in Configuration.Networks)
            {
                var network = new NetworkManager(n);
                NetworkManagers.Add(network);
            }
            using (var db = Database.SessionFactory.OpenSession())
            {
                foreach (var n in db.Query<Network>().Where(n => n.Enabled))
                {
                    if (Configuration.Networks.Any(_ => _.Name == n.Name))
                        continue;
                    var network = new NetworkManager(n);
                    NetworkManagers.Add(network);
                }
            }
            Thread.Yield();
            while (true)
                Thread.Sleep(1000);
        }
    }
}
