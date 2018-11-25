using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
    public interface IStatusDistribution
    {
        MediaListStatus status { get; }
        int amount { get; }
    }
}
