using System.Collections.Generic;

namespace AnimeListBot.Handler.Anilist
{
    public class AniUserStatistics
    {
        public int count;
        public float meanScore;
        public int minutesWatched;
        public int chaptersRead;
        public int volumesRead;
        public int episodesWatched;

        public List<AniUserStatusStatistic> statuses;
    }
}
