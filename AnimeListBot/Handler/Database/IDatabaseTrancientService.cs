using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler.Database
{
    public interface IDatabaseTrancientService : IDatabaseService
    {
        public string GetServerPrefix(ulong guildId);
    }
}
