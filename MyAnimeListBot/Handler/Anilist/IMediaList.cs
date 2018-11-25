using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
    public interface IMediaList
    {
        int id { get; }
        int userId { get; }
        int mediaId { get; }
        MediaListStatus status { get; }

        float score { get; }
        ScoreFormat format { get; }

        int progress { get; }
        int progressVolumes { get; }
        int repeat { get; }
        int priority { get; }
        bool @private { get; }
        string notes { get; }
        bool hiddenFromStatusList { get; }

        string customLists { get; }
        bool asArray { get; }

        string advancedScores { get; }
        IFuzzyDate startedAt { get; }
        IFuzzyDate completedAt { get; }

        int updatedAt { get; }

        int createdAt { get; }

        IMedia media { get; }

        IUser user { get; }
    }
}
