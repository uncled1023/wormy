using System;
using System.Net;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System.Globalization;
using ChatSharp.Events;
using System.Threading.Tasks;

namespace wormy.Modules
{
    // TODO: Add some stuff to ChatSharp to make formatting colored messages less painful
    [Depends(typeof(GoogleModule))]
    [Depends(typeof(LinksModule))]
    public class YouTubeModule : Module
    {
        public override string Name { get { return "youtube"; } }
        public override string Description { get { return "Searches YouTube videos and handles YouTube links."; } }

        public YouTubeModule(NetworkManager network) : base(network)
        {
            RegisterUserCommand("youtube", HandleCommand, "youtube [terms]: Searches YouTube for [terms] and shows information about the first result.");
            RegisterUserCommand("yt", HandleCommand);
            network.ModulesLoaded += (sender, e) => GetModule<LinksModule>().RegisterHostHandler("youtube.com", HandleLink);
            network.ModulesLoaded += (sender, e) => GetModule<LinksModule>().RegisterHostHandler("youtu.be", HandleLink);
        }

        void HandleLink(Uri uri, PrivateMessageEventArgs e)
        {
            HandleCommand(new[] { uri.ToString() }, e);
        }

        void HandleCommand(string[] arguments, PrivateMessageEventArgs e)
        {
            Task.Factory.StartNew(() =>
                {
                    if (arguments.Length == 0)
                        return;
                    string vid;
                    if (arguments.Length != 1 || (!arguments[0].StartsWith("http://") && !arguments[0].StartsWith("https://")))
                    {
                        var results = GoogleModule.DoGoogleSearch("site:youtube.com " + string.Join(" ", arguments));
                        if (results.Count == 0)
                        {
                            Respond(e, "No results found.");
                            return;
                        }
                        else
                            vid = results.First().Substring(results.First().LastIndexOf("http://"));
                    }
                    else
                        vid = arguments[0];
                    if (!vid.StartsWith("http://") && !vid.StartsWith("https://"))
                        vid = "http://" + vid;
                    if (Uri.IsWellFormedUriString(vid, UriKind.Absolute))
                    {
                        Uri uri = new Uri(vid);
                        if (uri.Host == "youtu.be" || uri.Host == "www.youtu.be")
                            vid = uri.LocalPath.Trim('/');
                        else
                        {
                            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                            vid = query["v"];
                        }
                        if (vid == null)
                        {
                            Respond(e, "Video not found.");
                            return;
                        }
                    }
                    var video = GetYoutubeVideo(vid);
                    if (video == null)
                    {
                        Respond(e, "Video not found.");
                        return;
                    }
                    string partOne = "\"\u0002" + video.Title + "\u000f\" [" + video.Duration.ToString("m\\:ss")
                             + "] (\u000312" + video.Author + "\u000f)\u000303 " + (video.HD ? "HD" : "SD");
                    string partTwo = video.Views.ToString("N0", CultureInfo.InvariantCulture) + " views";
                    if (video.RatingsEnabled)
                    {
                        partTwo += ", " + "(+\u000303" + video.Likes.ToString("N0", CultureInfo.InvariantCulture)
                        + "\u000f|-\u000304" + video.Dislikes.ToString("N0", CultureInfo.InvariantCulture) + "\u000f) [" + video.Stars + "]";
                    }
                    if (video.RegionLocked | !video.CommentsEnabled || !video.RatingsEnabled)
                    {
                        partTwo += " ";
                        if (video.RegionLocked)
                            partTwo += "\u000304Region locked\u000f, ";
                        if (!video.CommentsEnabled)
                            partTwo += "\u000304Comments disabled\u000f, ";
                        if (!video.RatingsEnabled)
                            partTwo += "\u000304Ratings disabled\u000f, ";
                        partTwo = partTwo.Remove(partTwo.Length - 3);
                    }
                    if (partOne.Length < partTwo.Length)
                        partOne += "\u000f " + video.VideoUri.ToString();
                    else
                        partTwo += "\u000f " + video.VideoUri.ToString();
                    Respond(e, partOne);
                    Respond(e, partTwo);
                });
        }

        class Video
        {
            public string Title, Author;
            public int Views, Likes, Dislikes;
            public TimeSpan Duration;
            public bool RegionLocked, HD, CommentsEnabled, RatingsEnabled;
            public string Stars;
            public Uri VideoUri;
        }

        private Video GetYoutubeVideo(string vid)
        {
            try
            {
                WebClient client = new WebClient();
                var sr = new StreamReader(client.OpenRead(string.Format("http://gdata.youtube.com/feeds/api/videos/{0}?v=2", Uri.EscapeUriString(vid))));
                string xml = sr.ReadToEnd();
                XDocument document = XDocument.Parse(xml);
                XNamespace media = XNamespace.Get("http://search.yahoo.com/mrss/");
                XNamespace youtube = XNamespace.Get("http://gdata.youtube.com/schemas/2007");
                XNamespace root = XNamespace.Get("http://www.w3.org/2005/Atom");
                XNamespace googleData = XNamespace.Get("http://schemas.google.com/g/2005");
                Video video = new Video();
                video.Title = document.Root.Element(root + "title").Value;
                video.Author = document.Root.Element(root + "author").Element(root + "name").Value;

                video.CommentsEnabled = document.Root.Elements(youtube + "accessControl")
                .First(e => e.Attribute("action").Value == "comment").Attribute("permission").Value == "allowed";
                video.RatingsEnabled = document.Root.Elements(youtube + "accessControl")
                .First(e => e.Attribute("action").Value == "rate").Attribute("permission").Value == "allowed";
                if (video.RatingsEnabled)
                    {
                        video.Likes = int.Parse(document.Root.Element(youtube + "rating").Attribute("numLikes").Value);
                        video.Dislikes = int.Parse(document.Root.Element(youtube + "rating").Attribute("numDislikes").Value);
                    }
                video.Views = int.Parse(document.Root.Element(youtube + "statistics").Attribute("viewCount").Value);
                video.Duration = TimeSpan.FromSeconds(
                    double.Parse(document.Root.Element(media + "group").Element(youtube + "duration").Attribute("seconds").Value));
                video.RegionLocked = document.Root.Element(media + "group").Element(media + "restriction") != null;
                video.VideoUri = new Uri("http://youtu.be/" + vid);
                video.HD = document.Root.Element(youtube + "hd") != null;
                if (video.RatingsEnabled)
                    {
                        video.Stars = "\u000303";
                        int starCount = (int)Math.Round(double.Parse(document.Root.Element(googleData + "rating").Attribute("average").Value));
                        for (int i = 0; i < 5; i++)
                            {
                                if (i < starCount)
                                    video.Stars += "★";
                                else if (i == starCount)
                                    video.Stars += "\u000315☆";
                                else
                                    video.Stars += "☆";
                            }
                        video.Stars += "\u000f";
                    }
                return video;
            }
            catch
            {
                return null;
            }
        }
    }
}

