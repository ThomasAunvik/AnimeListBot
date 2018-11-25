using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
    public interface IMediaListOptions
    {
        ScoreFormat scoreFormat { get; }

        string rowOrder { get; }

        bool useLegacyList { get; }

        IMediaListTypeOptions animeList { get; }
        IMediaListTypeOptions mangaList { get; }
    }
}
