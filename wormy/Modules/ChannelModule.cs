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
            RegisterAdminCommand("join", (arguments, e) =>
            {
                if (arguments.Length != 1) return;
                using (var session = Program.Database.SessionFactory.OpenSession())
                {
                    var channel = session.Query<Channel>().SingleOrDefault(cw => cw.Name == arguments[0]);
                    if (channel == null)
                    {
                        channel = new Channel(arguments[0], network.Network);
                        session.Save(channel);
                        if (network.Client.Channels.All(c => c.Name != channel.Name))
                            network.Client.JoinChannel(channel.Name);
                        Respond(e, "I have added {0} to my channel list.", channel.Name);
                    }
                    else
                    {
                        if (channel.Enabled)
                            Respond(e, "I already know about {0}.", arguments[0]);
                        else
                        {
                            channel.Enabled = true;
                            session.SaveOrUpdate(channel);
                            if (network.Client.Channels.All(c => c.Name != channel.Name))
                                network.Client.JoinChannel(channel.Name);
                        }
                    }
                }
            }, "join [channel]: Adds the specified channel to the bot's channel list.");
            RegisterAdminCommand("part", (arguments, e) =>
            {
                if (arguments.Length != 1) return;
                using (var session = Program.Database.SessionFactory.OpenSession())
                {
                    var channel = session.Query<Channel>().SingleOrDefault(cw => cw.Name == arguments[0]);
                    if (channel == null)
                        Respond(e, "I don't know about that channel.");
                    else
                    {
                        if (!channel.Enabled)
                            Respond(e, "This channel is already enabled: {0}.", arguments[0]);
                        else
                        {
                            channel.Enabled = false;
                            session.SaveOrUpdate(channel);
                            if (network.Client.Channels.Any(c => c.Name != channel.Name))
                                network.Client.PartChannel(channel.Name);
                        }
                    }
                }
            }, "part [channel]: Disables a given channel.");
        }
    }
}