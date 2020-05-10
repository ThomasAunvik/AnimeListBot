using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimeListBot.Handler.Database
{
    public class DatabaseTrancientService : DatabaseService, IDatabaseTrancientService
    {
        public DatabaseTrancientService(DatabaseConnection db) : base (db)
        {
            
        }

        public string GetServerPrefix(ulong guildId)
        {
            DiscordServer server = dbConn.DiscordServer.Find(guildId);
            if(server != null)
            {
                return server.Prefix;
            }
            return "al!";
        }
    }
}
