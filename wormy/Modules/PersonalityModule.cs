using System;
using System.Text.RegularExpressions;
using ChatSharp.Events;

namespace wormy.Modules
{
    public class PersonalityModule : Module
    {
        public override string Name { get { return "personality"; } }
        public override string Description { get { return "Adds minor things to make the bot more likable."; } }

        public PersonalityModule(NetworkManager network) : base(network)
        {
            var random = new Random();
            
            string[] gratitudeResponses = new[] { "You're welcome!", "Sure thing!", "Any time!", "My pleasure." };
            MatchRegex("^ *thank(s| you),? *wormy *$", (PrivateMessageEventArgs e, MatchCollection matches) => 
            {
                Respond(e, gratitudeResponses[random.Next(gratitudeResponses.Length)]);
            }, RegexOptions.Compiled);
        }
    }
}