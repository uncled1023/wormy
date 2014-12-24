using System;
using System.Linq;
using ChatSharp.Events;

namespace wormy.Modules
{
    public class MiscSearchesModule : Module
    {
        public override string Name { get { return "searches"; } }
        public override string Description { get { return "Provides search commands for various websites."; } }
        
        public MiscSearchesModule(NetworkManager network) : base(network)
        {
            RegisterUserCommand("wiki", (arguments, e) => HandleSearch(arguments, e, "en.wikipedia.org"),
                "wiki [terms]: Searches Wikipedia for [terms]");
            RegisterUserCommand("bakabt", (arguments, e) => HandleSearch(arguments, e, "bakabt.me"),
                "bakabt [terms]: Searches BakaBT for [terms]");
            RegisterUserCommand("nyaa", (arguments, e) => HandleSearch(arguments, e, "nyaa.se"),
                "nyaa [terms]: Searches nyaa for [terms]");
            RegisterUserCommand("github", (arguments, e) => HandleSearch(arguments, e, "github.com"),
                "gh [terms]: Searches GitHub for [terms]");
            RegisterUserCommand("xkcd", (arguments, e) => HandleSearch(arguments, e, "xkcd.com"),
                "xkcd [terms]: Searches xkcd for [terms]");
            // Aliases
            RegisterUserCommand("w", (arguments, e) => HandleSearch(arguments, e, "en.wikipedia.org"));
            RegisterUserCommand("baka", (arguments, e) => HandleSearch(arguments, e, "bakabt.me"));
            RegisterUserCommand("bbt", (arguments, e) => HandleSearch(arguments, e, "bakabt.me"));
            RegisterUserCommand("gh", (arguments, e) => HandleSearch(arguments, e, "github.com"));
        }

        private void HandleSearch(string[] arguments, PrivateMessageEventArgs e, string host)
        {
            GetModule<GoogleModule>().HandleCommand(new[] { "site:" + host }.Concat(arguments).ToArray(), e);
        }
    }
}