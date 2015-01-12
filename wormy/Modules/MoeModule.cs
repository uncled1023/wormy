using System;
using RedditSharp;
using System.Threading.Tasks;
using RedditSharp.Things;
using System.Linq;
using NHibernate.Linq;
using wormy.Database;
using System.Text.RegularExpressions;
using ChatSharp.Events;

namespace wormy.Modules
{
    [Depends(typeof(RedditModule))]
    public class MoeModule : Module
    {
        public override string Name { get { return "moe"; } }
        public override string Description { get { return "Supplies channels with infinite moe pictures."; } }

        private DateTime LastMoeDate { get; set; }
        private MoePost LastPost { get; set; }

        public MoeModule(NetworkManager network) : base(network)
        {
            LastPost = null;

            Subreddit subreddit = null;
            Reddit reddit = null;
            Random random = new Random();
            network.ModulesLoaded += (sender, e) => // TODO: Sort out module dependencies and unwrap this a bit
            {
                var mod = GetModule<RedditModule>();
                mod.Loaded += (_sender, _e) =>
                {
                    subreddit = mod.Reddit.GetSubreddit("/r/awwnime");
                    reddit = mod.Reddit;
                };
            };
                
            RegisterUserCommand("moe", (arguments, e) => Task.Factory.StartNew(() =>
                {
                    if (reddit == null)
                    {
                        RespondTo(e, "Sorry - I was just restarted and I'm still initializing the moe service.");
                        return;
                    }
                    if (arguments.Length == 1 && arguments[0].StartsWith("@"))
                    {
                        string username = arguments[0].Substring(1);
                        if (username == string.Empty)
                            username = e.PrivateMessage.User.Nick;
                        using (var session = Program.Database.SessionFactory.OpenSession())
                        {
                            var channel = session.Query<WormyChannel>().SingleOrDefault(c => c.Name == e.PrivateMessage.Source);
                            var user = session.Query<ChannelUser>().SingleOrDefault(u => u.Nick.ToUpper() == username.ToUpper()
                                && u.Channels.Any(c => c == channel));
                            if (user == null)
                            {
                                RespondTo(e, "Sorry, I'm not familiar with that user");
                            }
                            else
                            {
                                if (user.SavedPosts.Count == 0)
                                    RespondTo(e, "That person hasn't saved any moe.");
                                else
                                {
                                    var moe = user.SavedPosts[random.Next(user.SavedPosts.Count)];
                                    var post = reddit.GetThingByFullname("t3_" + moe.RedditId);
                                    RespondWithMoe(e, (Post)post, user);
                                    LastMoeDate = DateTime.Now;
                                    LastPost = moe;
                                }
                            }
                        }
                    }
                    else
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
                            var post = listing.FirstOrDefault(p => !p.IsSelfPost && !session.Query<MoePost>().Any(mp => mp.Channel == channel && mp.RedditId == p.Id));
                            if (post == null)
                                RespondTo(e, "Sorry! I'm all out of moe.");
                            else
                            {
                                RespondWithMoe(e, post);
                                // Add to database
                                var moe = new MoePost();
                                moe.Channel = channel;
                                moe.RedditId = post.Id;
                                moe.SearchTerms = string.Join(" ", arguments);
                                session.Save(moe);
                                LastPost = moe;
                                LastMoeDate = DateTime.Now;
                            }
                        }
                    }
                }), "moe [terms]: Finds a moe pic based on search [terms]. Omit terms for a random pic.");
            NetworkManager.Client.ChannelMessageRecieved += (sender, e) => Task.Factory.StartNew(() =>
            {
                if (e.PrivateMessage.Message.Trim().ToUpper() == "SAVED" && (DateTime.Now - LastMoeDate).TotalSeconds < 60)
                {
                    using (var session = Program.Database.SessionFactory.OpenSession())
                    {
                        using (var transaction = session.BeginTransaction())
                        {
                            var channel = session.Query<WormyChannel>().SingleOrDefault(c => c.Name == e.PrivateMessage.Source);
                            var user = session.Query<ChannelUser>().SingleOrDefault(u => u.Nick == e.PrivateMessage.User.Nick && u.Channels.Any(c => c == channel));
                            user.SavedPosts.Add(LastPost);
                            LastPost.SavedUsers.Add(user);
                            session.SaveOrUpdate(user);
                            session.SaveOrUpdate(LastPost);
                            transaction.Commit();
                        }
                    }
                }
            });
        }

        void RespondWithMoe(PrivateMessageEventArgs e, Post post, ChannelUser origin = null)
        {
            var sourceRegex = new Regex("\\[(?<source>.*)\\]"); // TODO: This doesn't work on titles like "foobar [Actual Source][Stupid Bullshit]"
            var source = sourceRegex.Match(post.Title);
            var prefix = "Here's a ";
            if (origin != null)
                prefix = origin.Nick + " likes this ";
            string response;
            if (!source.Success)
                response = string.Format(prefix + "cute picture: {0}", post.Url);
            else
            {
                var s = source.Groups["source"].Value;
                if (s.ToUpper() == "ORIGINAL")
                    response = string.Format(prefix + "cute original picture: {0}", post.Url);
                else
                    response = string.Format(prefix + "cute picture from {0}: {1}", s, post.Url);
            }
            Respond(e, response);
        }
    }
}