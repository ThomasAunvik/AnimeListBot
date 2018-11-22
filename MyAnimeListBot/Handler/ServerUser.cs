using Discord.WebSocket;
using System;

namespace MALBot.Handler
{
    [Serializable]
    public class ServerUser
    {
        public string username;
        public ulong userID;
        public bool isBot;

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
            }
        }
    }
}
