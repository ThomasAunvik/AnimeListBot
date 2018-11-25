using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
    public interface ITagStats
    {
        IMediaTag tag { get; }

        int amount { get; }
        int meanScore { get; }
        int timeWatcheD { get; }
    }
}
