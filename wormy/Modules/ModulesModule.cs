using System;
using System.Linq;
using System.Diagnostics;

namespace wormy.Modules
{
    public class ModulesModlue : Module
    {
        public override string Name { get { return "modules"; } }
        public override string Description { get { return "Meta module that does operations on other modules."; } }

        public ModulesModlue(NetworkManager network) : base(network)
        {
            // TODO: Only show modules available in the current channel?
            RegisterAdminCommand("modules", (arguments, e) =>
                Respond(e, "Installed modules: {0}", string.Join(", ", network.Modules.Select(m => m.Name).OrderBy(m => m))),
                "modules: Lists installed modules");
            RegisterAdminCommand("modinfo", (arguments, e) =>
                {
                    if (arguments.Length != 1) return;
                    var mod = network.Modules.SingleOrDefault(m => m.Name == arguments[0]);
                    if (mod == null)
                        Respond(e, "Module not found.");
                    else
                        Respond(e, "Module description: {0}", mod.Description);
                }, "modinfo [module]: Gives information about the specified module");
            RegisterAdminCommand("raw", (arguments, e) => 
                {
                    network.Client.SendRawMessage(string.Join(" ", arguments));
                }, "raw [text]: Sends a raw IRC message");
            RegisterAdminCommand("reload", (arguments, e) => 
            {
                Process.GetCurrentProcess().Kill();
            }, "reload: Kills the bot. You should have a watchdog of some sort to restart it.");
        }
    }
}