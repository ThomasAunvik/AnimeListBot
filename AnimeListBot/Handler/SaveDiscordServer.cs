using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler
{
    [Serializable]
    public class SaveDiscordServer
    {
        public string serverName;
        public ulong serverId;
        public ulong animeListChannelId = 0;

        public List<(ulong roleId, decimal days)> animeRoles;
        public List<(ulong roleId, decimal days)> mangaRoles;

        public List<ServerUser> users;

        public SaveDiscordServer(DiscordServer server)
        {
            if (server != null)
            {
                if (server.Guild != null)
                {
                    serverName = server.Guild.Name;
                    serverId = server.Guild.Id;
                }

                if (server.animeRoles != null)
                    animeRoles = server.animeRoles;

                if (server.mangaRoles != null)
                    mangaRoles = server.mangaRoles;

                if (server.Users != null)
                    users = server.Users;

                animeListChannelId = server.animeListChannelId;
            }
        }
    }
}
