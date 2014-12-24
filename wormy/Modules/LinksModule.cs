using System;
using System.Text.RegularExpressions;
using ChatSharp.Events;
using System.Net;
using System.IO;
using HtmlAgilityPack;
using System.Linq;

namespace wormy.Modules
{
    public class LinksModule : Module
    {
        public override string Name { get { return "links"; } }
        public override string Description { get { return "Recognizes links in the channel and shows information about them."; } }

        private const string UrlRegex = "((([A-Za-z]{3,9}:(?:\\/\\/)?)(?:[-;:&=\\+\\$,\\w]+@)?[A-Za-z0-9.-]+|(?:www.|[-;:&=\\+\\$,\\w]+@)[A-Za-z0-9.-]+)((?:\\/[\\+~%\\/.\\w-_]*)?\\??(?:[-\\+=&;%@.\\w_]*)#?(?:[\\w]*))?)";

        public LinksModule(NetworkManager network) : base(network)
        {
            MatchRegex(UrlRegex, (e, matches) =>
                {
                    foreach (Match match in matches)
                    {
                        Uri uri;
                        if (!Uri.TryCreate(match.Value, UriKind.Absolute, out uri))
                            Uri.TryCreate("http://" + match.Value, UriKind.Absolute, out uri);
                        if (uri != null)
                        {
                            if (uri.Scheme == "http" || uri.Scheme == "https")
                            {
                                // TODO: Domain handlers
                                var title = FetchPageTitle(uri.ToString());
                                Respond(e, "{0}: \"{1}\"", uri.Host, title);
                            }
                        }
                    }
                });
        }

        public static string FetchPageTitle(string url)
        {
            try
            {
                WebClient wc = new WebClient(); // I'm sorry, okay?
                StreamReader sr = new StreamReader(wc.OpenRead(url));
                string data = sr.ReadToEnd();
                sr.Close();
                HtmlDocument hDocument = new HtmlDocument();
                hDocument.LoadHtml(data);
                var title = hDocument.DocumentNode.Descendants("title");
                if (title != null)
                {
                    if (title.Count() > 0)
                    {
                        string text = title.First().InnerText;
                        text = text.Replace("\n", "").Replace("\r", "").Trim();
                        if (text.Length < 100)
                            return WebUtility.HtmlDecode(HtmlRemoval.StripTagsRegexCompiled(text));
                    }
                }
            }
            catch { return null; }
            return null;
        }
    }

    public static class HtmlRemoval
    {
        /// <summary>
        /// Remove HTML from string with Regex.
        /// </summary>
        public static string StripTagsRegex(string source)
        {
            return Regex.Replace(source, "<.*?>", string.Empty);
        }

        /// <summary>
        /// Compiled regular expression for performance.
        /// </summary>
        static Regex _htmlRegex = new Regex("<.*?>", RegexOptions.Compiled);

        /// <summary>
        /// Remove HTML from string with compiled Regex.
        /// </summary>
        public static string StripTagsRegexCompiled(string source)
        {
            return _htmlRegex.Replace(source, string.Empty);
        }

        /// <summary>
        /// Remove HTML tags from string using char array.
        /// </summary>
        public static string StripTagsCharArray(string source)
        {
            char[] array = new char[source.Length];
            int arrayIndex = 0;
            bool inside = false;

            for (int i = 0; i < source.Length; i++)
                {
                    char let = source[i];
                    if (let == '<')
                        {
                            inside = true;
                            continue;
                        }
                    if (let == '>')
                        {
                            inside = false;
                            continue;
                        }
                    if (!inside)
                        {
                            array[arrayIndex] = let;
                            arrayIndex++;
                        }
                }
            return new string(array, 0, arrayIndex);
        }
    }

    public static class TimeSpanExtensions
    {
        public static int GetYears(this TimeSpan timespan)
        {
            return (int)((double)timespan.Days / 365.2425);
        }
        public static int GetMonths(this TimeSpan timespan)
        {
            return (int)((double)timespan.Days / 30.436875);
        }
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
        public static string StripIrcColors(this string value)
        {
            string result = "";
            value = value.Trim().Replace("\u00031\u000315", "");
            for (int i = 0; i < value.Length; i++)
                {
                    if (value[i] == '\u0003')
                        i++;
                    else if (value[i] == '\u000f') { }
                    else
                        result += value[i];
                }
            return result;
        }
    }
}