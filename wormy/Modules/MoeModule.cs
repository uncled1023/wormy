using System;
using RedditSharp;
using System.Threading.Tasks;
using RedditSharp.Things;
using System.Linq;
using NHibernate.Linq;
using wormy.Database;
using System.Text.RegularExpressions;

namespace wormy.Modules
{
    [Depends(typeof(RedditModule))]
    public class MoeModule : Module
    {
        public override string Name { get { return "moe"; } }
        public override string Description { get { return "Supplies channels with infinite moe pictures."; } }

        public MoeModule(NetworkManager network) : base(network)
        {
            Subreddit subreddit = null;
            network.ModulesLoaded += (sender, e) => // TODO: Sort out module dependencies and unwrap this a bit
            {
                var mod = GetModule<RedditModule>();
                mod.Loaded += (_sender, _e) =>
                {
                    subreddit = mod.Reddit.GetSubreddit("/r/awwnime");
                };
            };

            var sourceRegex = new Regex("\\[(?<source>.*)\\]"); // TODO: This doesn't work on titles like "foobar [Actual Source][Stupid Bullshit]"
            RegisterUserCommand("moe", (arguments, e) => Task.Factory.StartNew(() =>
                {
                    Listing<Post> listing;
                    if (arguments.Length == 0)
                        listing = subreddit.New;
                    else
                        listing = subreddit.Search(string.Join(" ", arguments));
                    using (var session = Program.Database.SessionFactory.OpenSession())
                    {
                        var channel = session.Query<WormyChannel>().SingleOrDefault(c => c.Name == e.PrivateMessage.Source);
                        // Finds the first post that has yet to be mentioned in this channel
                        var post = listing.FirstOrDefault(p => !session.Query<MoePost>().Any(mp => mp.Channel == channel && mp.RedditId == p.Id));
                        if (post == null)
                            RespondTo(e, "Sorry! I'm all out of moe.");
                        else
                        {
                            var source = sourceRegex.Match(post.Title);
                            if (!source.Success)
                                Respond(e, "Here's a cute picture: {0}", post.Url);
                            else
                            {
                                var s = source.Groups["source"].Value;
                                if (s.ToUpper() == "ORIGINAL")
                                    Respond(e, "Here's a cute original picture: {0}", post.Url);
                                else
                                    Respond(e, "Here's a cute picture from {0}: {1}", s, post.Url);
                            }
                            // Add to database
                            var moe = new MoePost();
                            moe.Channel = channel;
                            moe.RedditId = post.Id;
                            moe.SearchTerms = string.Join(" ", arguments);
                            session.Save(moe);
                        }
                    }
                }), "moe [terms]: Finds a moe pic based on search [terms]. Omit terms for a random pic.");
        }
    }
}