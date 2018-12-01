using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler.Anilist
{
    public class AnilistMediaList : IAnilistMediaList
    {
        public int id { get; set; }

        public int userId { get; set; }

        public int mediaId { get; set; }

        public AnilistMediaListStatus? status { get; set; }

        public float? score { get; set; }

        public int? progress { get; set; }

        public int? progressVolumes { get; set; }

        public int? repeat { get; set; }

        public AniFuzzyDate startedAt { get; set; }

        public AniFuzzyDate completedAt { get; set; }
    }
}
