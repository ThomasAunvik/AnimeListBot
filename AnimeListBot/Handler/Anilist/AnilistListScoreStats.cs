﻿using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler.Anilist
{
    public class AnilistListScoreStats : IAnilistListScoreStats
    {
        public int meanScore { get; set; }

        public int standardDeviation { get; set; }
    }
}