namespace AnimeListBot.Handler.Anilist
{
    public class AniMediaListResponse
    {
        public AniMediaList MediaList { get; set; }

        public class AniMediaList : IAniMediaList
        {
            public int id { get; set; }

            public int userId { get; set; }

            public int mediaId { get; set; }

            public AniMediaListStatus? status { get; set; }

            public float? score { get; set; }

            public int? progress { get; set; }

            public int? progressVolumes { get; set; }

            public int? repeat { get; set; }

            public AniFuzzyDate startedAt { get; set; }

            public AniFuzzyDate completedAt { get; set; }
        }
    }
}
