using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using ChatSharp.Events;
using System.Linq;

namespace wormy.Modules
{
    public class GoogleModule : Module
    {
        public override string Name { get { return "google"; } }
        public override string Description { get { return "Allows users to perform Google searches."; } }

        public GoogleModule(NetworkManager network) : base(network)
        {
            RegisterUserCommand("google", HandleCommand, "google [terms]: Searches Google for [terms] and returns the first result.");
            RegisterUserCommand("g", HandleCommand);
            RegisterUserCommand("search", HandleCommand);
        }

        public void HandleCommand(string[] arguments, PrivateMessageEventArgs e)
        {
            if (arguments.Length == 0) return;
            var terms = string.Join(" ", arguments);
            var results = DoGoogleSearch(terms);
            if (!results.Any())
                Respond(e, "No results found.");
            else
                Respond(e, results[0]);
        }

        public static List<string> DoGoogleSearch(string terms)
        {
            List<string> results = new List<string>();
            try
            {
                WebClient client = new WebClient();
                StreamReader sr = new StreamReader(client.OpenRead("http://ajax.googleapis.com/ajax/services/search/web?v=1.0&q=" + Uri.EscapeUriString(terms)));
                string json = sr.ReadToEnd();
                sr.Close();
                JObject jobject = JObject.Parse(json);
                foreach (var result in jobject["responseData"]["results"])
                    results.Add(WebUtility.HtmlDecode(HtmlRemoval.StripTagsRegexCompiled(Uri.UnescapeDataString(result["title"].Value<string>())) +
                        " " + Uri.UnescapeDataString(result["url"].Value<string>())));
            }
            catch (Exception)
            {
            }
            return results;
        }
    }
}