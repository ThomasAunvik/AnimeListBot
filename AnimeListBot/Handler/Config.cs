using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AnimeListBot.Handler
{
    public class Config
    {
        public string bot_token;
        public int cluster_id;
        public List<ulong> bot_owners;

        // Database
        public string ip;
        public int port;
        public string catalog;
        public string userid;
        public string password;

        public static Config GetConfig()
        {
            if (!File.Exists("config.json")) throw new FileNotFoundException("Config File (config.json), Not Found.");

            string jsonFile = File.ReadAllText("config.json");
            Config config = JsonConvert.DeserializeObject<Config>(jsonFile);

            if (string.IsNullOrEmpty(config.bot_token)) throw new Exception("No bot token inserted in config file. (config.json)");

            return config;
        }
    }
}
