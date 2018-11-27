using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler.Anilist
{
    public interface IAnilistUserStats
    {
        int watchedTime { get; }
        int chaptersRead { get; }
        
        List<AnilistStatusDistribution> animeStatusDistribution { get; }
        List<AnilistStatusDistribution> mangaStatusDistribution { get; }

        List<AnilistScoreDistribution> animeScoreDistribution { get; }
        List<AnilistScoreDistribution> mangaScoreDistribution { get; }
        
        AnilistListScoreStats animeListScores { get; }
        AnilistListScoreStats mangaListScores { get; }
    }
}
