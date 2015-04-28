using System;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using wormy.Database;
using NHibernate.Linq;
using System.Linq;

namespace wormy.Modules
{
    public class OsuModule : Module
    {
        public override string Name { get { return "osu"; } }
        public override string Description { get { return "Allows users to interact a bit with osu (video game)"; } }

        public OsuModule(NetworkManager network) : base(network)
        {
            if (string.IsNullOrEmpty(Program.Configuration.OsuAPIKey))
                return;
            RegisterUserCommand("osu", (a, e) =>
            {
                if (a.Length > 1)
                    return;
                if (a.Length == 0)
                    a = new[] { e.PrivateMessage.User.Nick };
                OsuAlias alias = null;
                using (var session = Program.Database.SessionFactory.OpenSession())
                {
                    var channel = session.Query<WormyChannel>().SingleOrDefault(c => c.Name == e.PrivateMessage.Source);
                    if (channel == null) return;
                    alias = session.Query<OsuAlias>().SingleOrDefault(u => u.IrcNick.ToUpper() == a[0].ToUpper() && u.Channel == channel);
                    if (alias != null)
                        a[0] = alias.OsuNick;
                }
                var client = new WebClient();
                const string userApi = "https://osu.ppy.sh/api/get_user?k={0}&u={1}";
                const string recentApi = "https://osu.ppy.sh/api/get_user_recent?k={0}&u={1}";
                const string beatmapApi = "https://osu.ppy.sh/api/get_beatmaps?k={0}&b={1}";
                try
                {
                    var user = JToken.Parse(client.DownloadString(string.Format(userApi,
                        Program.Configuration.OsuAPIKey,
                        Uri.EscapeUriString(a[0])
                    )));
                    var recent = JToken.Parse(client.DownloadString(string.Format(recentApi,
                        Program.Configuration.OsuAPIKey,
                        Uri.EscapeUriString(a[0])
                    )));
                    JToken map = null;
                    var mods = new List<string>();
                    var modString = "No Mods";
                    if (((JArray)recent).Count != 0)
                    {
                        map = JToken.Parse(client.DownloadString(string.Format(beatmapApi,
                            Program.Configuration.OsuAPIKey,
                            recent[0]["beatmap_id"].Value<int>()
                        )));
                        var modse = (Mods)recent[0]["enabled_mods"].Value<int>();
                        if ((modse & Mods.DoubleTime) == Mods.DoubleTime) mods.Add("DT");
                        if ((modse & Mods.Easy) == Mods.Easy) mods.Add("EZ");
                        if ((modse & Mods.Flashlight) == Mods.Flashlight) mods.Add("FL");
                        if ((modse & Mods.HalfTime) == Mods.HalfTime) mods.Add("HT");
                        if ((modse & Mods.HardRock) == Mods.HardRock) mods.Add("HR");
                        if ((modse & Mods.Hidden) == Mods.Hidden) mods.Add("HD");
                        if ((modse & Mods.Nightcore) == Mods.Nightcore) mods.Add("NC");
                        if ((modse & Mods.NoFail) == Mods.NoFail) mods.Add("NF");
                        if ((modse & Mods.Perfect) == Mods.Perfect) mods.Add("PF");
                        if ((modse & Mods.SuddenDeath) == Mods.SuddenDeath) mods.Add("SD");
                        if ((modse & Mods.SpunOut) == Mods.SpunOut) mods.Add("SO");
                        if (mods.Count != 0)
                            modString = string.Join("/", mods);
                    }

                    var date = DateTime.Parse(recent[0]["date"].Value<string>() + " +8");
                    const string responseFormatA = "{0} [https://osu.ppy.sh/u/{1}] is #{2:N0} ({3:N0}pp), level {4:0}. "
                        + "{5:0}% accuracy {6:N0} plays.";
                    const string responseFormatB = "{0} last played {3} - {4} ({5:N1} stars, {7}) and scored {1:N0} ({2}{8}) {6}.";
                    Respond(e, responseFormatA,
                        alias == null ? user[0]["username"].Value<string>() : user[0]["username"].Value<string>() + " (aka " + alias.IrcNick + ")",
                        Uri.EscapeUriString(a[0]),
                        user[0]["pp_rank"].Value<int>(),
                        user[0]["pp_raw"].Value<double>(),
                        user[0]["level"].Value<double>(),
                        user[0]["accuracy"].Value<double>(),
                        user[0]["playcount"].Value<double>(),
                        user[0]["username"].Value<string>()
                    );
                    if (map != null)
                    {
                        Respond(e, responseFormatB,
                            user[0]["username"].Value<string>(),
                            recent[0]["score"].Value<int>(),
                            recent[0]["rank"].Value<string>(),
                            map[0]["artist"].Value<string>(),
                            map[0]["title"].Value<string>(),
                            map[0]["difficultyrating"].Value<double>(),
                            TrackingModule.FriendlyTimeSpan(DateTime.Now - date),
                            modString,
                            recent[0]["perfect"].Value<int>() == 1 ? " - Full Combo" : ""
                        );
                    }
                }
                catch (WebException ex)
                {
                    if ((int)ex.Status == 404)
                        Respond(e, "User not found.");
                }
            }, ".osu [user]: Fetches info about [user]'s osu profile");
            RegisterUserCommand("osua", (a, e) =>
            {
                if (a.Length != 1)
                    return;
                using (var session = Program.Database.SessionFactory.OpenSession())
                {
                    using (var transaction = session.BeginTransaction())
                    {
                        var channel = session.Query<WormyChannel>().SingleOrDefault(c => c.Name == e.PrivateMessage.Source);
                        if (channel == null) return;
                        var alias = session.Query<OsuAlias>().SingleOrDefault(
                            u => u.IrcNick.ToUpper() == e.PrivateMessage.User.Nick.ToUpper() && u.Channel == channel);
                        if (a[0] == "--drop")
                        {
                            if (alias == null)
                            {
                                RespondTo(e, "You haven't set an alias.");
                                return;
                            }
                            session.Delete(alias);
                            transaction.Commit();
                            RespondTo(e, "Your alias has been removed.");
                            return;
                        }
                        if (alias == null)
                        {
                            alias = new OsuAlias
                            {
                                IrcNick = e.PrivateMessage.User.Nick,
                                OsuNick = a[0],
                                Channel = channel
                            };
                        }
                        alias.OsuNick = a[0];
                        session.SaveOrUpdate(alias);
                        transaction.Commit();
                        RespondTo(e, "Your alias has been set.");
                    }
                }
            }, ".osua [alias]: Sets your osu alias (if your IRC nick != your osu handle). Use --drop to remove alias.");
        }

        [Flags]
        enum Mods
        {
            None           = 0,
            NoFail         = 1,
            Easy           = 2,
            //NoVideo      = 4,
            Hidden         = 8,
            HardRock       = 16,
            SuddenDeath    = 32,
            DoubleTime     = 64,
            Relax          = 128,
            HalfTime       = 256,
            Nightcore      = 512,
            Flashlight     = 1024,
            Autoplay       = 2048,
            SpunOut        = 4096,
            Relax2         = 8192,  // Autopilot?
            Perfect        = 16384,
            Key4           = 32768,
            Key5           = 65536,
            Key6           = 131072,
            Key7           = 262144,
            Key8           = 524288,
            keyMod         = Key4 | Key5 | Key6 | Key7 | Key8,
            FadeIn         = 1048576,
            Random         = 2097152,
            LastMod        = 4194304,
            FreeModAllowed = NoFail | Easy | Hidden | HardRock | SuddenDeath | Flashlight | FadeIn | Relax | Relax2 | SpunOut | keyMod,
            Key9           = 16777216,
            Key10          = 33554432,
            Key1           = 67108864,
            Key3           = 134217728,
            Key2           = 268435456
        }
    }
}
