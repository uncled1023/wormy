using System;
using System.Linq;
using ChatSharp.Events;
using NHibernate.Linq;
using wormy.Database;

namespace wormy.Modules
{
    public class TrackingModule : Module
    {
        public override string Name { get { return "tracking"; } }
        public override string Description { get { return "Keeps track of when users join/part and provides the seen command"; } }

        public TrackingModule(NetworkManager network) : base(network)
        {
            network.Client.ChannelMessageRecieved += HandleChannelMessageRecieved;
            RegisterUserCommand("seen", (arguments, e) =>
                {
                    if (arguments.Length != 1) return;
                    using (var session = Program.Database.SessionFactory.OpenSession())
                    {
                        var channel = session.Query<WormyChannel>().SingleOrDefault(c => c.Name == e.PrivateMessage.Source);
                        var user = session.Query<ChannelUser>().SingleOrDefault(u => u.Nick == arguments[0] && u.Channels.Any(c => c == channel));
                        if (user == null)
                            RespondTo(e, "I have never seen that user.");
                        else
                            RespondTo(e, "I last saw {0} {1}, saying \"{2}\"", user.Nick, FriendlyTimeSpan(DateTime.Now - user.LastSeen), user.LastSaid);
                    }
                }, "seen [nick]: Tells you how long it's been since [nick] last spoke.");
        }

        void HandleChannelMessageRecieved(object sender, PrivateMessageEventArgs e)
        {
            if (e.PrivateMessage.Message.StartsWith(CommandPrefix + "seen"))
                return;
            using (var session = Program.Database.SessionFactory.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    var channel = session.Query<WormyChannel>().SingleOrDefault(c => c.Name == e.PrivateMessage.Source);
                    if (channel == null) return;
                    var user = session.Query<ChannelUser>().SingleOrDefault(u => u.Nick == e.PrivateMessage.User.Nick && u.Channels.Any(c => c == channel));
                    if (user == null)
                    {
                        user = new ChannelUser();
                        user.AddChannel(channel);
                        user.Nick = e.PrivateMessage.User.Nick;
                    }
                    user.LastSeen = DateTime.Now;
                    user.LastSaid = e.PrivateMessage.Message;
                    session.SaveOrUpdate(user);
                    session.SaveOrUpdate(channel);
                    transaction.Commit();
                }
            }
        }

        string FriendlyTimeSpan(TimeSpan span)
        {
            if (span.TotalSeconds < 60)
                return string.Format("{0} seconds ago", (int)span.TotalSeconds);
            if (span.TotalMinutes < 60)
                return string.Format("{0} minutes, {1} seconds ago", (int)span.TotalMinutes, (int)span.Seconds);
            if (span.TotalHours < 24)
                return string.Format("{0} hours, {1} minutes ago", (int)span.TotalHours, (int)span.Minutes);
            return string.Format("{0} days ago", (int)span.TotalDays);
        }
    }
}
