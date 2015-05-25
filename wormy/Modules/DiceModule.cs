using System;

namespace wormy.Modules
{
    public class DiceModule : Module
    {
        public override string Name { get { return "bots"; } }
        public override string Description { get { return "This module exists to appease some channels on Rizon"; } }

        public DiceModule(NetworkManager network) : base(network)
        {
            var random = new Random();
            network.Client.ChannelMessageRecieved += (sender, e) => 
            {
                var message = e.PrivateMessage.Message;
                if (message.StartsWith("\x0001ACTION rolls a d") && message.EndsWith("\x0001"))
                {
                    var dice = message.Substring("\x0001ACTION rolls a d".Length);
                    dice = dice.Remove(dice.Length - 1);
                    int count = 1, die;
                    if (!int.TryParse(dice, out die))
                        return;
                    if (count > 10 || count < 1 || die < 2 || die > 100)
                        return;
                    int result = 0;
                    while (count-- > 0)
                        result += random.Next(1, die);
                    Respond(e, "{0} rolls a d{1} and gets {2}", e.PrivateMessage.User.Nick, dice, result);
                }
                else if (message.StartsWith("\x0001ACTION rolls ") && message.EndsWith("\x0001"))
                {
                    var dice = message.Substring("\x0001ACTION rolls ".Length);
                    dice = dice.Remove(dice.Length - 1);
                    var parts = dice.Split('d');
                    if (parts.Length != 2) return;
                    int count, die;
                    if (!int.TryParse(parts[0], out count) || !int.TryParse(parts[1], out die))
                        return;
                    if (count > 10 || count < 1 || die < 2 || die > 100)
                        return;
                    int result = 0;
                    while (count-- > 0)
                        result += random.Next(1, die);
                    Respond(e, "{0} rolls {1} and gets {2}", e.PrivateMessage.User.Nick, dice, result);
                }
            };
        }
    }
}