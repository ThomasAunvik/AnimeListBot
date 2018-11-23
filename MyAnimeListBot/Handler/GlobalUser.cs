using Discord;
using Discord.WebSocket;
using JikanDotNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MALBot.Handler
{
    public class GlobalUser
    {
        public SaveDiscordUser savedUser;
        public string Username;

        public ulong UserID;

        public string MAL_Username;

        public string imageURL;
        public decimal? daysWatchedAnime = 0;

        public GlobalUser(IUser user)
        {
            Username = user.Username;
            UserID = user.Id;

            LoadData();
            SaveData();
        }

        public async Task UpdateMALInfo()
        {
            UserProfile profile = await Program._jikan.GetUserProfile(MAL_Username);
            daysWatchedAnime = profile.AnimeStatistics.DaysWatched;
            imageURL = profile.ImageURL;

            SaveData();
        }

        public SaveDiscordUser LoadData()
        {
            if(File.Exists("DiscordUserFiles/" + UserID + ".json"))
            {
                string JSONstring = File.ReadAllText("DiscordUserFiles/" + UserID + ".json");
                SaveDiscordUser save = JsonConvert.DeserializeObject<SaveDiscordUser>(JSONstring);
                if(save != null)
                {
                    Username = save.Username;
                    UserID = save.UserID;
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
            if (!File.Exists("DiscordUserFiles/" + UserID + ".json"))
                stream = File.Create("DiscordUserFiles/" + UserID + ".json");

            if(stream != null)
                stream.Close();
            File.WriteAllText("DiscordUserFiles/" + UserID + ".json", jsonFormatted);
        }

        public static void DeleteServerFile(SocketUser user)
        {
            if(File.Exists("DiscordUserFiles / " + user.Id + ".json"))
                File.Delete("DiscordUserFiles / " + user.Id + ".json");
        }
    }

    public class SaveDiscordUser
    {
        public string Username;
        public ulong UserID;

        public string MAL_Username;

        public string imageURL;
        public decimal? daysWatchedAnime = 0;

        public SaveDiscordUser(GlobalUser user)
        {
            if(user != null)
            {
                UserID = user.UserID;
                Username = user.Username;
                MAL_Username = user.MAL_Username;

                imageURL = user.imageURL;
                daysWatchedAnime = user.daysWatchedAnime;
            }
        }
    }
}
