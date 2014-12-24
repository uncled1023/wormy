using System;

namespace wormy.Modules
{
    public class BotsModule : Module
    {
        public override string Name { get { return "bots"; } }
        public override string Description { get { return "This module exists to appease some channels on Rizon"; } }

        public BotsModule(NetworkManager network) : base(network)
        {
            RegisterUserCommand("bots", (s, e) => Respond(e, "Reporting in! [C#]"));
        }
    }
}