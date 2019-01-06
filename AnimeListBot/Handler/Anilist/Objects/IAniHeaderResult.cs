using System;

namespace AnimeListBot.Handler.Anilist
{
    public interface IAniHeaderResult
    {
        int RetryAfter { get; }
        int RateLimit_Limit { get; }
        int RateLimit_Remaining { get; }
        DateTime? RateLimit_Reset { get; }
    }
}
