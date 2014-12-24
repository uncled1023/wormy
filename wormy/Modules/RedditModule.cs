using System;
using RedditSharp;
using System.Threading.Tasks;

namespace wormy.Modules
{
    public class RedditModule : Module
    {
        public override string Name { get { return "reddit"; } }
        public override string Description { get { return "Reddit meta module"; } }

        public Reddit Reddit { get; set; }
        public event EventHandler Loaded;

        public RedditModule(NetworkManager network) : base(network)
        {
            // TODO: Figure out some way of adding module-specific static configuration
            Reddit = new Reddit();
            // This is done after ModulesLoaded to make sure that all of the dependent modules have a chance to register with the event handler
            network.ModulesLoaded += (sender, e) => Task.Factory.StartNew(() =>
                {
                    Reddit.LogIn(Program.Configuration.Reddit.User, Program.Configuration.Reddit.Password);
                    if (Loaded != null)
                        Loaded(this, null);
                });
        }
    }
}

