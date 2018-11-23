using Discord;
using Discord.WebSocket;
using JikanDotNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MALBot.Handler
{
    public class GlobalUser
    {
        public SaveDiscordUser savedUser;
        public string Username;

        public ulong userID;
        public List<ServerUser> serverUsers = new List<ServerUser>();

        public string MAL_Username;

        public string imageURL;
        public decimal? daysWatchedAnime = 0;

        public GlobalUser(IUser user)
        {
            Username = user.Username;
            userID = user.Id;

            LoadData();
            SaveData();
        }

        public async Task UpdateMALInfo()
        {
            UserProfile profile = await Program._jikan.GetUserProfile(MAL_Username);
            if (profile != null)
            {
                daysWatchedAnime = profile.AnimeStatistics.DaysWatched;
                imageURL = profile.ImageURL;
            }

            SaveData();
        }

        public SaveDiscordUser LoadData()
        {
            if(File.Exists("DiscordUserFiles/" + userID + ".json"))
            {
                string JSONstring = File.ReadAllText("DiscordUserFiles/" + userID + ".json");
                SaveDiscordUser save = JsonConvert.DeserializeObject<SaveDiscordUser>(JSONstring);
                if(save != null)
                {
                    Username = save.Username;
                    userID = save.UserID;
                    MAL_Username = save.MAL_Username;
                    return save;
                }
            }
            return null;
        }

        public void SaveData()
        {
            savedUser = new SaveDiscordUser(this);

            string outputJSON = JsonConvert.SerializeObject(savedUser);

            string jsonFormatted = JToken.Parse(outputJSON).ToString(Formatting.Indented);

            FileStream stream = null;
            if (!Directory.Exists("DiscordUserFiles/"))
                Directory.CreateDirectory("DiscordUserFiles/");
            if (!File.Exists("DiscordUserFiles/" + userID + ".json"))
                stream = File.Create("DiscordUserFiles/" + userID + ".json");

            if(stream != null)
                stream.Close();
            File.WriteAllText("DiscordUserFiles/" + userID + ".json", jsonFormatted);
        }

        public static void DeleteServerFile(SocketUser user)
        {
            if(File.Exists("DiscordUserFiles / " + user.Id + ".json"))
                File.Delete("DiscordUserFiles / " + user.Id + ".json");
        }
    }
}
