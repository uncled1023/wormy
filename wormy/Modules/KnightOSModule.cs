using System;
using ChatSharp.Events;
using System.Net;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace wormy.Modules
{
	public class KnightOSModule : Module
	{
		public override string Name { get { return "knightos"; } }
		public override string Description { get { return "Allows users to browse online KnightOS documentation"; } }

		public KnightOSModule(NetworkManager network) : base(network)
		{
			RegisterUserCommand("k", SearchKOS, ".k [function]: Searches KnightOS kernel documentation for [function]");
		}

		private void SearchKOS(string[] parameters, PrivateMessageEventArgs e)
		{
			var webClient = new WebClient();
			var json = JObject.Parse(webClient.DownloadString("http://www.knightos.org/documentation/reference/data.json"));
			var functions = json.Children<JProperty>().SelectMany(c => ((JObject)c.Value).Properties());
			var results = functions.Where(f => f.Name.ToUpper() == string.Join(" ", parameters).ToUpper());
			if (!results.Any())
				Respond(e, "No function by that name is documented.");
			else
			{
				var item = results.First().Value as JObject;
				NetworkManager.Client.SendMessage(string.Format("\u0002{0}\u000f: {1}", results.First().Name,
					new string(item["description"].Value<string>().Take(100).ToArray())), e.PrivateMessage.Source);
				if (item["sections"] != null && item["sections"]["Inputs"] != null)
				{
					var inputs = new List<string>();
					foreach (var _input in item["sections"]["Inputs"])
					{
						var input = (JProperty)_input;
						var name = input.Name;
						var use = input.Value.Value<string>();
						if (use.Length > 25)
							use = new string(use.Take(25).ToArray()) + "...";
						inputs.Add(name + ": " + use);
					}
					NetworkManager.Client.SendMessage("\u0002Inputs\u000f: " + string.Join("; ", inputs.ToArray()), e.PrivateMessage.Source);
				}
				if (item["sections"] != null && item["sections"]["Outputs"] != null)
				{
					var outputs = new List<string>();
					foreach (var _input in item["sections"]["Outputs"])
					{
						var input = (JProperty)_input;
						var name = input.Name;
						var use = input.Value.Value<string>();
						if (use.Length > 25)
							use = new string(use.Take(25).ToArray()) + "...";
						outputs.Add(name + ": " + use);
					}
					NetworkManager.Client.SendMessage("\u0002Outputs\u000f: " + string.Join("; ", outputs.ToArray()), e.PrivateMessage.Source);
				}
				NetworkManager.Client.SendMessage("\u0002Source\u000f: " + item["path"].Value<string>() + ", line " +
					item["line"].Value<int>(), e.PrivateMessage.Source);
				NetworkManager.Client.SendMessage("\u0002More info\u000f: http://www.knightos.org/documentation/reference/" +
					(item.Parent.Parent.Parent as JProperty).Name.ToLower() + ".html#" + item["name"].Value<string>(),
					e.PrivateMessage.Source);
			}
		}
	}
}