using System;
using System.Linq;
using System.Collections.Generic;

namespace wormy.Modules
{
    public class HelpModule : Module
    {
        public override string Name { get { return "help"; } }
        public override string Description { get { return "Offers help about installed modules and commands."; } }

        private Dictionary<string, string> HelpText { get; set; }

        public HelpModule(NetworkManager network) : base(network)
        {
            HelpText = new Dictionary<string, string>();

            // We have to register our own commands _after_ our module is loaded, so that we can manage our own help text
            network.ModulesLoaded += (sender, _) =>
            {
                RegisterUserCommand("commands", (arguments, e) =>
                    {
                        var commands = network.Modules.SelectMany(mod => mod.UserCommandHandlers.Keys);
                        if (IsAdmin(e))
                            commands = commands.Concat(network.Modules.SelectMany(mod => mod.AdminCommandHandlers.Keys).Select(k => k + " (a)"));
                        var list = commands.ToList();
                        list.Sort();
                        Respond(e, "Available commands: {0}", string.Join(", ", list));
                        Respond(e, "You may use .help [command] for information on specific commands.");
                    }, "commands: lists available commands");
                RegisterUserCommand("help", (arguments, e) =>
                    {
                        if (arguments.Length != 1) return;
                        if (HelpText.ContainsKey(arguments[0]))
                            Respond(e, HelpText[arguments[0]]);
                        else
                            Respond(e, "No help available for '{0}'", arguments[0]);
                    }, "help [topic]: Gives documentation for [topic]");
            };
        }

        public void AddHelp(string command, string text)
        {
            HelpText[command] = text;
        }
    }
}