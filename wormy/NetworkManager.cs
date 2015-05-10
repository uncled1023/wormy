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
        public Network Network { get; set; }
        public IrcClient Client { get; set; }
        public List<Module> Modules { get; set; }

        public event EventHandler ModulesLoaded;
        public event EventHandler<PrivateMessageEventArgs> HandleMessageBeforeModules;

        public NetworkManager(Network network)
        {
            Network = network;
            Modules = new List<Module>();
            Client = new IrcClient(network.Address, new IrcUser(network.Nick, network.User, network.Password, network.RealName));
            Client.RawMessageRecieved += (sender, e) => Console.WriteLine("<< {0}", e.Message);
            Client.RawMessageSent += (sender, e) => Console.WriteLine(">> {0}", e.Message);
            Client.ConnectionComplete += HandleConnectionComplete;
            Client.NetworkError += (sender, e) => Console.WriteLine("Network error {0}", e.SocketError);
            Client.Settings.WhoIsOnJoin = true;
            Client.ConnectAsync();
        }

        void RegisterModules()
        {
            // Help module has to be loaded first (TODO: Remove special case in favor of dependencies)
            var help = new HelpModule(this);
            Modules.Add(help);

            var moduleTypes = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes().Where(t =>
                    typeof(Module).IsAssignableFrom(t) && !t.IsAbstract && t != typeof(HelpModule)))
                {
                    moduleTypes.Add(type);
                }
            }
            // TODO: Sort modules by dependencies here
            moduleTypes.ToList().ForEach(t =>
                {
                    var module = (Module)Activator.CreateInstance(t, this);
                    Modules.Add(module);
                });
            ModulesLoaded(this, null);
        }

        void HandleConnectionComplete(object sender, EventArgs e)
        {
            Console.WriteLine("Connected to {0}.", Network.Name);
            RegisterModules();
            Client.ChannelMessageRecieved += HandleMessageRecieved;
            Client.UserMessageRecieved += HandleMessageRecieved;
            if (!Network.NickServ)
                JoinChannels();
            else
            {
                Client.NoticeRecieved += HandleNickServ;
                Client.SendMessage("identify " + Network.Password, "NickServ");
            }
        }

        void HandleNickServ(object sender, IrcNoticeEventArgs e)
        {
            if (new IrcUser(e.Source).Nick == "NickServ" && e.Notice.StartsWith("Password accepted"))
            {
                JoinChannels();
                Client.NoticeRecieved -= HandleNickServ;
            }
        }

        void JoinChannels()
        {
            using (var session = Program.Database.SessionFactory.OpenSession())
            {
                var network = session.Query<Network>().Single(n => n.ID == Network.ID);
                network.Channels.Where(c => c.Enabled).Select(c => c.Name).ToList().ForEach(Client.JoinChannel);
            }
        }

        void HandleMessageRecieved(object sender, PrivateMessageEventArgs e)
        {
            if (e.PrivateMessage.Message.StartsWith("\u200B"))
                return;
            if (HandleMessageBeforeModules != null)
                HandleMessageBeforeModules(this, e);
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
