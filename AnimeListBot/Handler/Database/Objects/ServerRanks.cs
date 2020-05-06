using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler
{
    public enum RankOption
    {
        ANIME, MANGA
    }

    public class ServerRanks
    {
        public ulong RegisterChannelId { get; set; } = 0;
        public List<RoleRank> AnimeRanks { get; set; } = new List<RoleRank>();
        public List<RoleRank> MangaRanks { get; set; } = new List<RoleRank>();
        public List<RoleRank> NotSetRanks { get; set; } = new List<RoleRank>();
    }
}
