using System;
using System.Collections.Generic;
using NHibernate.Linq;
using wormy.Database;
using System.Linq;

namespace wormy.Modules
{
    public class IgnoreModule : Module
    {
        public override string Name { get { return "ignore"; } }
        public override string Description { get { return "Allows you to ignore specific users."; } }
        public Dictionary<string, string[]> Masks { get; set; }

        public IgnoreModule(NetworkManager network) : base(network)
        {
            // Cache masks
            Masks = new Dictionary<string, string[]>();
            using (var session = Program.Database.SessionFactory.OpenSession())
            {
                foreach (var mask in session.Query<IgnoredMask>())
                {
                    if (Masks.ContainsKey(mask.Channel.Name))
                    {
                        Masks[mask.Channel.Name] = Masks[mask.Channel.Name].Concat(new[] { mask.Mask }).ToArray();
                    }
                    else
                    {
                        Masks[mask.Channel.Name] = new[] { mask.Mask };
                    }
                }
            }

            network.HandleMessageBeforeModules += (sender, e) =>
            {
                if (!e.PrivateMessage.IsChannelMessage)
                    return;
                if (!Masks.ContainsKey(e.PrivateMessage.Source))
                    return;
                var masks = Masks[e.PrivateMessage.Source];
                if (masks.Any(e.PrivateMessage.User.Match))
                    e.PrivateMessage.Message = string.Empty; // Empty the message before any modules try to process it
            };

            RegisterAdminCommand("ignore", (args, e) =>
                {
                    if (args.Length != 1)
                        return;
                    if (!Masks.ContainsKey(e.PrivateMessage.Source))
                        Masks[e.PrivateMessage.Source] = new string[0];
                    var masks = Masks[e.PrivateMessage.Source];
                    if (masks.Any(m => m == args[0]))
                    {
                        RespondTo(e, "I'm already ignoring this person.");
                        return;
                    }
                    Masks[e.PrivateMessage.Source] = Masks[e.PrivateMessage.Source].Concat(new[] { args[0] }).ToArray();
                    var match = NetworkManager.Client.Channels[e.PrivateMessage.Source].Users.SingleOrDefault(u => u.Match(args[0]));
                    if (match == null)
                        RespondTo(e, "Warning: a user by that mask isn't present in this channel.");
                    else
                        RespondTo(e, "Okay, ignoring that mask. It matches {0}, fyi.", match.Nick);

                    using (var session = Program.Database.SessionFactory.OpenSession())
                    {
                        var channel = session.Query<WormyChannel>().SingleOrDefault(c => c.Name == e.PrivateMessage.Source);
                        var ignored = new IgnoredMask();
                        ignored.Mask = args[0];
                        ignored.Channel = channel;
                        session.Save(ignored);
                    }
                }, "ignore [mask]: Ignores the specified user mask in this channel.");

            RegisterAdminCommand("unignore", (args, e) =>
                {
                    if (args.Length != 1)
                        return;
                    if (!Masks.ContainsKey(e.PrivateMessage.Source))
                        Masks[e.PrivateMessage.Source] = new string[0];
                    var masks = Masks[e.PrivateMessage.Source];
                    if (masks.All(m => m != args[0]))
                    {
                        RespondTo(e, "I'm not ignoring that mask.");
                        return;
                    }
                    Masks[e.PrivateMessage.Source] = Masks[e.PrivateMessage.Source].Where(m => m != args[0]).ToArray();
                    var match = NetworkManager.Client.Channels[e.PrivateMessage.Source].Users.SingleOrDefault(u => u.Match(args[0]));
                    if (match == null)
                        RespondTo(e, "Okay, no longer ignoring that mask.");
                    else
                        RespondTo(e, "Okay, no longer ignoring that mask. It matches {0}, fyi.", match.Nick);

                    using (var session = Program.Database.SessionFactory.OpenSession())
                    {
                        var channel = session.Query<WormyChannel>().SingleOrDefault(c => c.Name == e.PrivateMessage.Source);
                        var ignored = session.Query<IgnoredMask>().SingleOrDefault(m => m.Mask == args[0] && m.Channel == channel);
                        session.Delete(ignored);
                        session.Flush();
                    }
                }, "unignore [mask]: Removes an ignore on a user in this channel.");
        }
    }
}