namespace AnimeListBot.Handler.Anilist
{
    public class AniMediaResponse
    {
        public AniMedia Media { get; set; }

        public class AniMedia : IAniMedia
        {
            public int? id { get; set; }

            public int? idMal { get; set; }

            public AniMediaTitle title { get; set; }

            public AniMediaType? type { get; set; }

            public AniMediaFormat? format { get; set; }

            public AniMediaStatus? status { get; set; }

            public string description { get; set; }

            public AniFuzzyDate startDate { get; set; }

            public AniFuzzyDate endDate { get; set; }

            public int? episodes { get; set; }

            public int? chapters { get; set; }

            public int? volumes { get; set; }

            public AniMediaCoverImage coverImage { get; set; }

            public string siteUrl { get; set; }
        }
    }
}
