/*
 * This file is part of AnimeList Bot
 *
 * AnimeList Bot is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * AnimeList Bot is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with AnimeList Bot.  If not, see <https://www.gnu.org/licenses/>
 */
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
        public static Config cached { get; private set; }

        public string bot_token;
        public int cluster_id;
        public int override_shard_amount = 0;
        public List<ulong> bot_owners = new List<ulong>();

        public HashSet<string> ignoredExceptionMessages = new HashSet<string>();

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

            cached = config;
            return config;
        }

        public void Save()
        {
            if (!File.Exists("config.json")) File.Create("config.json");

            string jsonText = JsonConvert.SerializeObject(this);
            File.WriteAllText("config.json", jsonText);
            cached = this;
        }
    }
}
