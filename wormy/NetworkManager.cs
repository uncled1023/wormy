using System;
using NHibernate.Linq;
using System.Linq;
using ChatSharp;
using ChatSharp.Events;
using System.Collections.Generic;
using wormy.Database;
using wormy.Modules;

namespace wormy
{
    public class NetworkManager
    {
        public Configuration.NetworkConfiguration Configuration { get; set; }
        public IrcClient Client { get; set; }
        public List<Module> Modules { get; set; }

        public event EventHandler ModulesLoaded;

        public NetworkManager(Configuration.NetworkConfiguration config)
        {
            Configuration = config;
            Modules = new List<Module>();
            Client = new IrcClient(config.Address, new IrcUser(config.Nick, config.User, config.Password, config.RealName));
            Client.ConnectionComplete += HandleConnectionComplete;
            Client.ConnectAsync();
        }

        void RegisterModules()
        {
            // Help module has to be loaded first
            // TODO: Better dependency resolution, probably via attributes on the class
            var help = new HelpModule(this);
            Modules.Add(help);
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes().Where(t =>
                    typeof(Module).IsAssignableFrom(t) && !t.IsAbstract && t != typeof(HelpModule)))
                {
                    var module = (Module)Activator.CreateInstance(type, this);
                    Modules.Add(module);
                }
            }
            ModulesLoaded(this, null);
        }

        void HandleConnectionComplete(object sender, EventArgs e)
        {
            Console.WriteLine("Connected to {0}.", Configuration.Name);
            RegisterModules();
            Client.ChannelMessageRecieved += HandleMessageRecieved;
            Client.UserMessageRecieved += HandleMessageRecieved;
            using (var session = Program.Database.SessionFactory.OpenSession())
            {
                var channels = session.Query<WormyChannel>().Where(cw => cw.Network == Configuration.Name).Select(cw => cw.Name);
                channels.ToList().ForEach(Client.JoinChannel);
            }
        }

        void HandleMessageRecieved(object sender, PrivateMessageEventArgs e)
        {
            if (Program.Configuration.AdminMasks.Any(e.PrivateMessage.User.Match))
            {
                if (HandleAdminMessage(e))
                    return;
            }
            HandleUserMessage(e);
        }

        bool HandleAdminMessage(PrivateMessageEventArgs e)
        {
            foreach (var mod in Modules)
            {
                if (mod.HandleAdminMessage(e))
                    return true;
            }
            return false;
        }

        bool HandleUserMessage(PrivateMessageEventArgs e)
        {
            foreach (var mod in Modules)
            {
                if (mod.HandleUserMessage(e))
                    return true;
            }
            return false;
        }
    }
}