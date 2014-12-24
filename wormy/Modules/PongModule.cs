using System;
using System.Linq;
using NHibernate.Linq;
using wormy.Database;

namespace wormy.Modules
{
    public class PongModule : Module
    {
        public override string Name { get { return "pong"; } }
        public override string Description { get { return "Provides the 'ping' command."; } }

        public PongModule(NetworkManager network) : base(network)
        {
            RegisterUserCommand("ping", (arguments, e) => Respond(e, "pong!"));
        }
    }
}