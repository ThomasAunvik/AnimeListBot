using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler.Anilist
{
    class AnilistMedia : IAnilistMedia
    {
        public int? id { get; set; }

        public int? idMal { get; set; }

        public AnilistMediaTitle title { get; set; }

        public AnilistMediaType? type { get; set; }

        public AnilistMediaStatus? status { get; set; }

        public string description { get; set; }

        public AniFuzzyDate startDate { get; set; }

        public AniFuzzyDate endDate { get; set; }

        public int? episodes { get; set; }

        public int? chapters { get; set; }

        public int? volumes { get; set; }

        public AnilistMediaCoverImage coverImage { get; set; }

        public string siteUrl { get; set; }
    }
}
