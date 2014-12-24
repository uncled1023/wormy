using System;
using System.Linq;

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
        }
    }
}