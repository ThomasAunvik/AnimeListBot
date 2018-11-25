using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
    public interface IStudioStats
    {
        IStudio studio { get; }
        int amount { get; }
        int meanSchore { get; }
        int timeWatched { get; }
    }
}
