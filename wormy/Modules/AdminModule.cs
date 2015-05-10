using System;
using System.Linq;
using wormy.Database;
using NHibernate.Linq;

namespace wormy.Modules
{
    public class AdminModule : Module
    {
        public override string Name { get { return "modules"; } }
        public override string Description { get { return "Meta module that does operations on other modules."; } }

        public AdminModule(NetworkManager network) : base(network)
        {
            // TODO: Only show modules available in the current channel?
            RegisterAdminCommand("modules", (arguments, e) =>
            {
                Respond(e, "Installed modules: {0}", string.Join(", ", network.Modules.Select(m => m.Name).OrderBy(m => m)));
            }, "modules: Lists installed modules");

            RegisterAdminCommand("modinfo", (arguments, e) =>
            {
                if (arguments.Length != 1) return;
                var mod = network.Modules.SingleOrDefault(m => m.Name == arguments[0]);
                if (mod == null)
                    Respond(e, "Module not found.");
                else
                    Respond(e, "Module description: {0}", mod.Description);
            }, "modinfo [module]: Gives information about the specified module");

            RegisterAdminCommand("raw", (arguments, e) => 
            {
                network.Client.SendRawMessage(string.Join(" ", arguments));
            }, "raw [text]: Sends a raw IRC message");

            RegisterAdminCommand("network", (args, e) =>
            {
                if (args.Length < 2)
                    return;
                using (var db = Program.Database.SessionFactory.OpenSession())
                {
                    if (args[0] == "add")
                    {
                        if (db.Query<Network>().Any(n => n.Name.ToUpper() == args[1].ToUpper()))
                        {
                            Respond(e, "A network by that name already exists.");
                            return;
                        }
                        var net = new Network
                        {
                            Name = args[1],
                            Address = args[2],
                            Nick = args[3],
                            User = args[4]
                        };
                        if (args.Length > 5)
                            net.Password = args[5];
                        if (args.Length > 6)
                            net.RealName = args[6];
                        else
                            net.RealName = "wormy - https://github.com/SirCmpwn/wormy";
                        db.SaveOrUpdate(net);
                        var manager = new NetworkManager(net);
                        Program.NetworkManagers.Add(manager);
                        Respond(e, "Network added.");
                    }
                    else if (args[0] == "disable")
                    {
                        var net = db.Query<Network>().SingleOrDefault(n => n.Name.ToUpper() == args[1].ToUpper());
                        if (net == null)
                        {
                            Respond(e, "I don't know of any networks by that name.");
                            return;
                        }
                        net.Enabled = false;
                        db.SaveOrUpdate(net);
                        var manager = Program.NetworkManagers.SingleOrDefault(m => m.Configuration.Name.ToUpper() == net.Name.ToUpper());
                        if (manager != null)
                        {
                            manager.Client.Quit("Network has been disabled by " + e.PrivateMessage.User.Nick);
                            Program.NetworkManagers.Remove(manager);
                        }
                        Respond(e, "Network disabled.");
                    }
                    else if (args[0] == "enable")
                    {
                        var net = db.Query<Network>().SingleOrDefault(n => n.Name.ToUpper() == args[1].ToUpper());
                        if (net == null)
                        {
                            Respond(e, "I don't know of any networks by that name.");
                            return;
                        }
                        net.Enabled = false;
                        db.SaveOrUpdate(net);
                        var manager = new NetworkManager(net);
                        Program.NetworkManagers.Add(manager);
                        Respond(e, "Network enabled.");
                    }
                }
            }, "network [add|disable|enable] [name address nick user password realname|name|name]: Manages networks");
        }
    }
}