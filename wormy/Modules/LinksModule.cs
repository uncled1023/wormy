using System;

namespace wormy.Modules
{
    public class LinksModule : Module
    {
        public override string Name { get { return "links"; } }
        public override string Description { get { return "Recognizes links in the channel and shows information about them."; } }

        public LinksModule(NetworkManager network) : base(network)
        {
        }
    }
}