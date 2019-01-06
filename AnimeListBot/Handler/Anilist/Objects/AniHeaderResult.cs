using System;

namespace AnimeListBot.Handler.Anilist
{
    public class AniHeaderResult : IAniHeaderResult
    {
        public int RetryAfter { get; set; }

        public int RateLimit_Limit { get; set; }

        public int RateLimit_Remaining { get; set; }

        public DateTime? RateLimit_Reset { get; set; }
    }
}
