﻿using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler.Anilist
{
    public class AnilistStatusDistribution : IAnilistStatusDistribution
    {
        public AnilistMediaListStatus status { get; set; }
        public int amount { get; set; }
    }
}
