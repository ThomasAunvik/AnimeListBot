using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
    public interface IGenreStats
    {
        string genre { get; }
        int amount { get; }
        int meanScore { get; }
        int timeWatched { get; }
    }
}
