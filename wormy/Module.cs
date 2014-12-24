using System;
using System.Linq;
using System.Collections.Generic;
using NHibernate.Linq;
using ChatSharp.Events;
using wormy.Database;
using wormy.Modules;
using System.Text.RegularExpressions;

namespace wormy
{
    public abstract class Module
    {
        public delegate void CommandHandler(string[] arguments, PrivateMessageEventArgs e);
        public delegate void RegexHandler(PrivateMessageEventArgs e, MatchCollection matches);

        internal Dictionary<string, CommandHandler> AdminCommandHandlers;
        internal Dictionary<string, CommandHandler> UserCommandHandlers;
        internal Dictionary<Regex, RegexHandler> RegexHandlers;

        protected NetworkManager NetworkManager { get; set; }
        protected string CommandPrefix { get; private set; }

        public abstract string Name { get; }
        public abstract string Description { get; }

        protected Module(NetworkManager network)
        {
            NetworkManager = network;
            AdminCommandHandlers = new Dictionary<string, CommandHandler>();
            UserCommandHandlers = new Dictionary<string, CommandHandler>();
            RegexHandlers = new Dictionary<Regex, RegexHandler>();
        }

        protected void RegisterUserCommand(string command, CommandHandler handler, string help = null)
        {
            UserCommandHandlers.Add(command, handler);
            if (help != null)
            {
                var mod = NetworkManager.Modules.SingleOrDefault(m => m.GetType() == typeof(HelpModule)) as HelpModule;
                if (mod != null)
                    mod.AddHelp(command, help);
            }
        }

        protected void RegisterAdminCommand(string command, CommandHandler handler, string help = null)
        {
            AdminCommandHandlers.Add(command, handler);
            if (help != null)
            {
                var mod = NetworkManager.Modules.SingleOrDefault(m => m.GetType() == typeof(HelpModule)) as HelpModule;
                if (mod != null)
                    mod.AddHelp(command, help);
            }
        }

        protected T GetModule<T>() where T : Module
        {
            foreach (var module in NetworkManager.Modules)
            {
                if (module is T)
                    return module as T;
            }
            throw new Exception("Unable to locate dependent module");
        }

        protected void MatchRegex(string regex, RegexHandler handler, RegexOptions options = RegexOptions.None)
        {
            RegexHandlers.Add(new Regex(regex, RegexOptions.Compiled | options), handler);
        }

        protected void RespondTo(PrivateMessageEventArgs e, string format, params object[] arguments)
        {
            NetworkManager.Client.SendMessage(e.PrivateMessage.User.Nick + ": " + string.Format(format, arguments), e.PrivateMessage.Source);
        }

        protected void Respond(PrivateMessageEventArgs e, string format, params object[] arguments)
        {
            NetworkManager.Client.SendMessage(string.Format(format, arguments), e.PrivateMessage.Source);
        }

        protected bool IsAdmin(PrivateMessageEventArgs e)
        {
            return Program.Configuration.AdminMasks.Any(e.PrivateMessage.User.Match);
        }

        internal bool HandleAdminMessage(PrivateMessageEventArgs e)
        {
            return HandleMessage(AdminCommandHandlers, e);
        }

        internal bool HandleUserMessage(PrivateMessageEventArgs e)
        {
            foreach (var handler in RegexHandlers)
            {
                var matches = handler.Key.Matches(e.PrivateMessage.Message);
                if (matches.Count != 0)
                    handler.Value(e, matches);
            }
            return HandleMessage(UserCommandHandlers, e);
        }

        private bool HandleMessage(Dictionary<string, CommandHandler> handlers, PrivateMessageEventArgs e)
        {
            string prefix = null;
            if (e.PrivateMessage.IsChannelMessage)
            {
                using (var session = Program.Database.SessionFactory.OpenSession())
                {
                    var channel = session.Query<WormyChannel>().SingleOrDefault(cw => cw.Name == e.PrivateMessage.Source);
                    if (channel != null)
                    {
                        prefix = channel.CommandPrefix;
                        CommandPrefix = prefix;
                    }
                }
            }
            else
                prefix = ""; // User messages require no prefix

            if (prefix != null && e.PrivateMessage.Message.StartsWith(prefix))
            {
                var space = e.PrivateMessage.Message.IndexOf(' ');
                if (space == -1)
                    space = e.PrivateMessage.Message.Length;
                var command = e.PrivateMessage.Message.Substring(prefix.Length, space - prefix.Length);
                var parameters = e.PrivateMessage.Message.Substring(command.Length + prefix.Length).Trim().Split(' ');
                if (handlers.ContainsKey(command))
                {
                    try
                    {
                        handlers[command](parameters, e);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        // TODO: Log this exception better
                        Console.WriteLine(ex);
                        return false;
                    }
                }
            }
            return false;
        }
    }
}