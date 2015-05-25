using System;

namespace wormy.Modules
{
    public class BotsModule : Module
    {
        public override string Name { get { return "bots"; } }
        public override string Description { get { return "This module exists to appease some channels on Rizon"; } }

        public BotsModule(NetworkManager network) : base(network)
        {
            network.Client.ChannelMessageRecieved += (sender, e) => 
            {
                if (e.PrivateMessage.Message == ".bots")
                    Respond(e, "Reporting in! [C#] https://github.com/SirCmpwn/wormy");
            };
        }
    }
}