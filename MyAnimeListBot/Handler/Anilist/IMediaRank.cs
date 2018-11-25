using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
    public interface IMediaRank
    {
        int id { get; }
        int rank { get; }
        MediaRankType type { get; }
        MediaFormat format { get; }
        int year { get; }
        MediaSeason season { get; }
        bool allTime { get; }
        string context { get; }
    }
}
