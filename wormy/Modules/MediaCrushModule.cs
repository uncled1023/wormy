using System;
using ChatSharp.Events;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Collections.Generic;
using System.Linq;

namespace wormy.Modules
{
    [Depends(typeof(LinksModule))]
    public class MediaCrushModule : Module
    {
        public override string Name { get { return "mediacrush"; } }
        public override string Description { get { return "Provides better URL handling for mediacru.sh links"; } }

        public MediaCrushModule(NetworkManager network) : base(network)
        {
            network.ModulesLoaded += (sender, e) => GetModule<LinksModule>().RegisterHostHandler("mediacru.sh", HandleUri);
        }

        private void HandleUri(Uri uri, PrivateMessageEventArgs e)
        {
            if (!new Regex("^https?://mediacru\\.sh/[0-9A-Za-z-_]{12}$").IsMatch(uri.ToString()))
                return;
            var json = JToken.Parse(new WebClient().DownloadString(uri.ToString() + ".json"));
            string response = "";
            switch (json["blob_type"].Value<string>())
            {
                case "image":
                    response += "Image";
                    break;
                case "video":
                    response += "Video";
                    break;
                case "audio":
                    response += "Audio";
                    break;
                case "album":
                    Respond(e, "MediaCrush: Album, " + json["files"].ToArray().Length + " files.");
                    return;
            }
            if (json["flags"]["nsfw"] != null && json["flags"]["nsfw"].Value<bool>())
                response += " (NSFW)";
            response += ":";
            if (json["metadata"] != null)
                {
                    var info = new List<string>();
                    if (json["metadata"]["dimensions"] != null)
                    {
                        info.Add(json["metadata"]["dimensions"]["width"].Value<int>()
                            + "x" + json["metadata"]["dimensions"]["height"].Value<int>());
                    }
                    if (json["metadata"]["duration"] != null && json["metadata"]["duration"].Value<double?>() != null)
                    {
                        var span = TimeSpan.FromSeconds(Math.Round(json["metadata"]["duration"].Value<double>()));
                        info.Add(span.ToString());
                    }
                    if (json["metadata"]["artist"] != null || json["metadata"]["title"] != null)
                    {
                        var song = "";
                        if (json["metadata"]["artist"] != null)
                            song += json["metadata"]["artist"].Value<string>();
                        if (json["metadata"]["artist"] != null && json["metadata"]["title"] != null)
                            song += " - ";
                        if (json["metadata"]["title"] != null)
                            song += json["metadata"]["title"].Value<string>();
                        info.Add(song);
                    }
                    if (json["metadata"]["has_audio"] != null)
                    {
                        if (json["metadata"]["has_audio"].Value<bool>())
                            info.Add("contains audio");
                        else
                            info.Add("no audio");
                    }
                    if (json["metadata"]["has_subtitles"] != null)
                    {
                        if (json["metadata"]["has_subtitles"].Value<bool>())
                            info.Add("has subtitles");
                    }
                    response += " " + string.Join(", ", info.ToArray());
                }
            Respond(e, "MediaCrush: " + response);
        }
    }
}