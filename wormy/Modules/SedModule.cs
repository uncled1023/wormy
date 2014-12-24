using System;
using System.Text.RegularExpressions;
using NHibernate.Linq;
using wormy.Database;
using System.Linq;
using System.Collections.Generic;

namespace wormy.Modules
{
    public class SedModule : Module
    {
        public override string Name { get { return "sed"; } }
        public override string Description { get { return "Recognizes sed usage in channel and corrects the previous statement of a user."; } }

        private Dictionary<string, string> LastSaid { get; set; }

        public SedModule(NetworkManager network) : base(network)
        {
            LastSaid = new Dictionary<string, string>();

            network.Client.ChannelMessageRecieved += (sender, e) =>
            {
                try
                {
                    if (e.PrivateMessage.Message.StartsWith("s/"))
                    {
                        // TODO: support for stuff like /i and whatever
                        var parts = e.PrivateMessage.Message.Split('/');
                        if (parts.Length != 4) return;
                        var regex = new Regex(parts[1]);
                        if (LastSaid.ContainsKey(e.PrivateMessage.User.Nick))
                        {
                            if (regex.IsMatch(LastSaid[e.PrivateMessage.User.Nick]))
                                Respond(e, "<{0}> {1}", e.PrivateMessage.User.Nick, regex.Replace(LastSaid[e.PrivateMessage.User.Nick], parts[2]));
                        }
                    }
                    else
                        LastSaid[e.PrivateMessage.User.Nick] = e.PrivateMessage.Message;
                }
                catch { /* who cares */ }
            };
        }
    }
}