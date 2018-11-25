using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
    public interface IScoreDistribution
    {
        int score { get; }
        int amount { get; }
    }
}
