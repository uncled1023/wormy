using System;
using System.Linq;
using NHibernate.Linq;
using wormy.Database;

namespace wormy.Modules
{
    public class ChannelModule : Module
    {
        public override string Name { get { return "channels"; } }
        public override string Description { get { return "Manages channels the bot is joined to on this network."; } }

        public ChannelModule(NetworkManager network) : base(network)
        {
            RegisterAdminCommand("join",
                (arguments, e) =>
                {
                    if (arguments.Length != 1) return;
                    using (var session = Program.Database.SessionFactory.OpenSession())
                    {
                        WormyChannel channel = session.Query<WormyChannel>().SingleOrDefault(cw => cw.Name == arguments[0]);
                        if (channel == null)
                        {
                            channel = new WormyChannel();
                            channel.CommandPrefix = "."; // Default prefix (TODO: customizable?)
                            channel.Key = null;
                            channel.Name = arguments[0];
                            channel.Network = network.Configuration.Name;
                            session.Save(channel);
                            network.Client.JoinChannel(channel.Name);
                            Respond(e, "I have added {0} to my channel list.", channel.Name);
                        }
                        else
                        {
                            Respond(e, "I already know about {0}.", arguments[0]);
                        }
                    }
                }, "join [channel]: Adds the specified channel to the bot's channel list.");
        }
    }
}