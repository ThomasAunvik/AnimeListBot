using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
    public interface IListScoreStats
    {
        int meanScore { get; }
        int standardDeviation { get; }
    }
}
