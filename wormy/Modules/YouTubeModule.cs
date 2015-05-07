using System;
using System.Net;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System.Globalization;
using ChatSharp.Events;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Xml;

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
            if (string.IsNullOrEmpty(Program.Configuration.GoogleAPIKey))
                return;
            RegisterUserCommand("youtube", HandleCommand, "youtube [terms]: Searches YouTube for [terms] and shows information about the first result.");
            RegisterUserCommand("yt", HandleCommand);
            network.ModulesLoaded += (sender, e) => GetModule<LinksModule>().RegisterHostHandler("www.youtube.com", HandleLink);
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
                var sr = new StreamReader(client.OpenRead(string.Format(
                    "https://www.googleapis.com/youtube/v3/videos?part=id%2Csnippet%2Cstatistics%2CcontentDetails%2Cstatus%2Clocalizations&id={0}&key={1}",
                    Uri.EscapeUriString(vid), Uri.EscapeUriString(Program.Configuration.GoogleAPIKey))));
                string json = sr.ReadToEnd();
                JObject j = JObject.Parse(json);
                if (j["items"].Count() == 0)
                    return null;
                dynamic i = j["items"][0];

                Video video = new Video();
                video.Title = i.snippet.title;
                video.Author = i.snippet.channelTitle;

                video.CommentsEnabled = true; // NOTE: YouTube has removed this from their API, assholes
                video.RatingsEnabled = i.status.publicStatsViewable;
                if (video.RatingsEnabled)
                {
                    video.Likes = i.statistics.likeCount;
                    video.Dislikes = i.statistics.dislikeCount;
                    double average;
                    if (video.Likes + video.Dislikes == 0) average = 1;
                    else average = (double)video.Likes / (video.Likes + video.Dislikes);
                    video.Stars = "\u000303";
                    int starCount = (int)Math.Round(average * 5);
                    for (int k = 0; k < 5; k++)
                    {
                        if (k < starCount)
                            video.Stars += "★";
                        else if (k == starCount)
                            video.Stars += "\u000315☆";
                        else
                            video.Stars += "☆";
                    }
                    video.Stars += "\u000f";
                }
                video.Views = i.statistics.viewCount;
                video.Duration = XmlConvert.ToTimeSpan(i["contentDetails"]["duration"].Value);
                video.RegionLocked = i.contentDetails["regionRestriction"] != null;
                video.VideoUri = new Uri("http://youtu.be/" + vid);
                video.HD = i.contentDetails.definition == "hd";
                return video;
            }
            catch
            {
                return null;
            }
        }
    }
}

