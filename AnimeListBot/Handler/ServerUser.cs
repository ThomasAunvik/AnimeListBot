using Discord.WebSocket;
using System;
using Newtonsoft.Json;

namespace AnimeListBot.Handler
{
    [Serializable]
    public class ServerUser
    {
        public string username;
        public ulong userID;
        public bool isBot;

        public ulong currentAnimeRankId = 0;
        public ulong currentMangaRankId = 0;

        [JsonIgnore]
        public GlobalUser globalUser;

        public ServerUser(SocketGuildUser user)
        {
            if(user != null)
            {
                username = user.Username;
                userID = user.Id;
                isBot = user.IsBot;
            }
        }

        public void UpdateInfo(SocketGuildUser user)
        {
            if(user != null)
            {
                username = user.Username;
                userID = user.Id;
                isBot = user.IsBot;

                globalUser = Program.globalUsers.Find(x => x.userID == userID);
                if(globalUser != null)
                {
                    globalUser.serverUsers.Add(this);
                }
            }
        }
    }
}
