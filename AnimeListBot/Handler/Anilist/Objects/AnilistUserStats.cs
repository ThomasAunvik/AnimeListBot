using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler.Anilist
{
    public class AnilistUserStats
    {
        public int watchedTime { get; set; }
        public int chaptersRead { get; set; }
        
        public List<AnilistStatusDistribution> animeStatusDistribution { get; set; }
        public List<AnilistStatusDistribution> mangaStatusDistribution { get; set; }

        public List<AniScoreDistribution> animeScoreDistribution { get; set; }
        public List<AniScoreDistribution> mangaScoreDistribution { get; set; }

        public AniListScoreStats animeListScores { get; set; }
        public AniListScoreStats mangaListScores { get; set; }
    }
}
