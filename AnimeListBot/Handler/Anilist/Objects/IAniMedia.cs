using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler.Anilist
{
    public interface IAnilistMedia
    {
        int? id { get; }
        int? idMal { get; }
        AniMediaTitle title { get; }
        AniMediaType? type { get; }
        AniMediaStatus? status { get; }

        string description { get; }

        AniFuzzyDate startDate { get; }
        AniFuzzyDate endDate { get; }

        int? episodes { get; }
        int? chapters { get; }
        int? volumes { get; }

        AniMediaCoverImage coverImage { get; }
        string siteUrl { get; }
    }
}
