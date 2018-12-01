using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler.Anilist
{
    public interface IAnilistMediaList
    {
        int id { get; }
        int userId { get; }
        int mediaId { get; }
        AnilistMediaListStatus? status { get; }
        float? score { get; }
        int? progress { get; }
        int? progressVolumes { get; }
        int? repeat { get; }
        AnilistFuzzyDate startedAt { get; }
        AnilistFuzzyDate completedAt { get; }
    }
}
