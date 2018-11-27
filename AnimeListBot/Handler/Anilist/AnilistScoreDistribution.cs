using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler.Anilist
{
    public class AnilistScoreDistribution : IAnilistScoreDistribution
    {
        public int score { get; set; }
        public int amount { get; set; }
    }
}
