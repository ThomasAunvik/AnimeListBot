using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler.Anilist
{
    public interface IAnilistStatusDistribution
    {
        AnilistMediaListStatus status { get; }
        int amount { get; }
    }
}
