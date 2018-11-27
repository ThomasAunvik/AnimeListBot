using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler.Anilist
{
    public class AnilistUserStats : IAnilistUserStats
    {
        public int watchedTime { get; set; }
        public int chaptersRead { get; set; }
        
        public List<AnilistStatusDistribution> animeStatusDistribution { get; set; }
        public List<AnilistStatusDistribution> mangaStatusDistribution { get; set; }

        public List<AnilistScoreDistribution> animeScoreDistribution { get; set; }
        public List<AnilistScoreDistribution> mangaScoreDistribution { get; set; }

        public AnilistListScoreStats animeListScores { get; set; }
        public AnilistListScoreStats mangaListScores { get; set; }
    }
}
