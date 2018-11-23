using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler
{
    [Serializable]
    public class SaveDiscordServer
    {
        public string serverName;
        public ulong serverId;
        public List<(ulong roleId, decimal days)> animeRoles;

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

                if (server.Users != null)
                    users = server.Users;
            }
        }
    }
}
