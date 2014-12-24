using System;
using System.Collections.Generic;
using ChatSharp.Events;

namespace wormy
{
    public abstract class Module
    {
        public delegate void CommandHandler(string[] arguments, PrivateMessageEventArgs e);

        private Dictionary<string, CommandHandler> AdminCommandHandlers;
        private Dictionary<string, CommandHandler> UserCommandHandlers;

        protected NetworkManager NetworkManager { get; set; }

        public abstract string Name { get; }
        public abstract string Description { get; }

        protected Module(NetworkManager network)
        {
            NetworkManager = network;
            AdminCommandHandlers = new Dictionary<string, CommandHandler>();
            UserCommandHandlers = new Dictionary<string, CommandHandler>();
        }

        protected void RegisterUserCommand(string command, CommandHandler handler)
        {
            UserCommandHandlers.Add(command, handler);
        }

        protected void RegisterAdminCommand(string command, CommandHandler handler)
        {
            AdminCommandHandlers.Add(command, handler);
        }

        protected void Respond(PrivateMessageEventArgs e, string format, params object[] arguments)
        {
            NetworkManager.Client.SendMessage(string.Format(format, arguments), e.PrivateMessage.Source);
        }

        internal bool HandleAdminMessage(PrivateMessageEventArgs e)
        {
            return HandleMessage(AdminCommandHandlers, e);
        }

        internal bool HandleUserMessage(PrivateMessageEventArgs e)
        {
            return HandleMessage(UserCommandHandlers, e);
        }

        private bool HandleMessage(Dictionary<string, CommandHandler> handlers, PrivateMessageEventArgs e)
        {
            string prefix = null;
            if (e.PrivateMessage.IsChannelMessage)
            {
                // TODO: Look up channel and corresponding command prefix
            }
            else
                prefix = ""; // User messages require no prefix

            if (prefix != null && e.PrivateMessage.Message.StartsWith(prefix))
            {
                var space = e.PrivateMessage.Message.IndexOf(' ');
                if (space == -1)
                    space = e.PrivateMessage.Message.Length - prefix.Length;
                var command = e.PrivateMessage.Message.Substring(prefix.Length, space);
                var parameters = e.PrivateMessage.Message.Substring(command.Length).Trim().Split(' ');
                if (handlers.ContainsKey(command))
                {
                    handlers[command](parameters, e);
                    return true;
                }
            }
            return false;
        }
    }
}