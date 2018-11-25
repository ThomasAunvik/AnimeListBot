using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
    public interface IUserStats
    {
        int watchedTime { get; }
        int chaptersRead { get; }

        List<IUserActivityHistory> activityHistory { get; }
        List<IStatusDistribution> animeStatusDistribution { get; }
        List<IStatusDistribution> mangaStatusDistribution { get; }
        List<IScoreDistribution> animeScoreDistribution { get; }
        List<IScoreDistribution> mangaScoreDistribution { get; }
        IListScoreStats animeListScores { get; }
        IListScoreStats mangaListScores { get; }
        List<IGenreStats> favouredGenresOvervie { get; }
        List<IGenreStats> favouredGenres { get; }
        List<ITagStats> favouredTags { get; }
        List<IStaffStats> favouredActors { get; }
        List<IStaffStats> favouredStaff { get; }
        List<IStudioStats> favouredStudios { get; }
        List<IYearStats> favouredYears { get; }
        List<IFormatStats> favouredFormats { get; }
}
}
