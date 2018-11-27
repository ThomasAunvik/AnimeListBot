using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler.Anilist
{
    public interface IAnilistListScoreStats
    {
        int meanScore { get; }
        int standardDeviation { get; }
    }
}
