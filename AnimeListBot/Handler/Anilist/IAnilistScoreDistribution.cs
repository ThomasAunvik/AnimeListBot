using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler.Anilist
{
    public interface IAnilistScoreDistribution
    {
        int score { get; }
        int amount { get; }
    }
}
