using System;
using Chronic;
using System.Text.RegularExpressions;
using ChatSharp.Events;
using System.Collections.Generic;
using wormy.Database;
using NHibernate.Linq;
using System.Timers;
using System.Linq;

namespace wormy.Modules
{
    public class ReminderModule : Module
    {
        public override string Name { get { return "reminders"; } }
        public override string Description { get { return "Allows users to set reminders for themselves."; } }

        private List<Reminder> Reminders { get; set; }
        private object lockObject = new object();
        
        public ReminderModule(NetworkManager network)
            : base(network)
        {
            MatchRegex("^ *" + network.Client.User.Nick + "(,|:) *remind me (?<time>(in|at) .*) +to +(?<action>.*) *$", HandleMatches, RegexOptions.IgnoreCase);
            MatchRegex("^ *remind me (?<time>(in|at) .*) +to +(?<action>.*)(,|:) *" + network.Client.User.Nick + " *$", HandleMatches, RegexOptions.IgnoreCase);
            MatchRegex("^ *" + network.Client.User.Nick + "(,|:) *remind me to +(?<action>.*) (?<time>(in|at) .*) *$", HandleMatches, RegexOptions.IgnoreCase);
            MatchRegex("^ *remind me to +(?<action>.*) (?<time>(in|at) .*)(,|:) *" + network.Client.User.Nick + " *$", HandleMatches, RegexOptions.IgnoreCase);

            Reminders = new List<Reminder>();
            using (var session = Program.Database.SessionFactory.OpenSession())
            {
                foreach (var reminder in session.Query<Reminder>())
                {
                    Reminders.Add(reminder);
                }
            }

            var timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += UpdateReminders;
            timer.Start();
        }

        private void UpdateReminders(object sender, ElapsedEventArgs e)
        {
            lock (lockObject)
            {
                var expired = Reminders.Where(r => r.DueDate <= DateTime.Now);
                using (var session = Program.Database.SessionFactory.OpenSession())
                {
                    foreach (var reminder in expired)
                    {
                        Reminders.Remove(reminder);
                        NetworkManager.Client.SendMessage(string.Format("{0}: Reminder to {1}", reminder.Target, reminder.Action), reminder.Source);
                        session.Delete(reminder);
                    }
                }
            }
        }

        private void HandleMatches(PrivateMessageEventArgs e, MatchCollection matches)
        {
            // TODO: Check to make sure the user is present
            var time = matches[0].Groups["time"].Value;
            var action = matches[0].Groups["action"].Value;
            if (time.StartsWith("at"))
                time = time.Substring(2);
            time = time.Trim();
            action = action.Trim();
            var parsed = new Parser().Parse(time);
            if (parsed == null)
            {
                RespondTo(e, "I don't know what '{0}' means. Can you rephrase that?", time);
                return;
            }
            if (action == string.Empty)
            {
                RespondTo(e, "Remind you to what?");
                return;
            }
            if (parsed.ToTime() <= DateTime.Now.AddSeconds(5))
            {
                RespondTo(e, "No.");
                return;
            }
            RespondTo(e, "Okay, I'll remind you to {0} later.", action);
            lock (lockObject)
            {
                using (var session = Program.Database.SessionFactory.OpenSession())
                {
                    var reminder = new Reminder
                    {
                        Action = action,
                        DueDate = parsed.ToTime(),
                        Source = e.PrivateMessage.Source,
                        Target = e.PrivateMessage.User.Nick
                    };
                    session.SaveOrUpdate(reminder);
                    Reminders.Add(reminder);
                }
            }
        }
    }
}