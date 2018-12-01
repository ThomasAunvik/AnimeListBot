using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler.Anilist
{
    public interface IAniMediaList
    {
        int id { get; }
        int userId { get; }
        int mediaId { get; }
        AniMediaListStatus? status { get; }
        float? score { get; }
        int? progress { get; }
        int? progressVolumes { get; }
        int? repeat { get; }
        AniFuzzyDate startedAt { get; }
        AniFuzzyDate completedAt { get; }
    }
}
