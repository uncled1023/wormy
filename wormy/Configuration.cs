using System;
using Newtonsoft.Json;
using System.IO;

namespace wormy
{
    public class Configuration
    {
        public Configuration()
        {
            Database = new DatabaseConfiguration();
            Reddit = new RedditConfiguration();
        }

        public void Load(string path)
        {
            JsonConvert.PopulateObject(File.ReadAllText(path), this);
        }

        public void Save(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        [JsonProperty("networks")]
        public NetworkConfiguration[] Networks { get; set; }
        [JsonProperty("database")]
        public DatabaseConfiguration Database { get; set; }
        [JsonProperty("reddit")]
        public RedditConfiguration Reddit { get; set; }
        [JsonProperty("adminMasks")]
        public string[] AdminMasks { get; set; }

        public class DatabaseConfiguration
        {
            [JsonProperty("connectionString")]
            public string ConnectionString { get; set; }
        }

        public class NetworkConfiguration
        {
            [JsonProperty("name")]
            public string Name { get; set; }
            [JsonProperty("address")]
            public string Address { get; set; }
            [JsonProperty("user")]
            public string User { get; set; }
            [JsonProperty("nick")]
            public string Nick { get; set; }
            [JsonProperty("password")]
            public string Password { get; set; }
            [JsonProperty("realname")]
            public string RealName { get; set; }
        }

        public class RedditConfiguration
        {
            [JsonProperty("user")]
            public string User { get; set; }
            [JsonProperty("password")]
            public string Password { get; set; }
        }
    }
}