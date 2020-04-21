/*
 * This file is part of AnimeList Bot
 *
 * AnimeList Bot is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * AnimeList Bot is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with AnimeList Bot.  If not, see <https://www.gnu.org/licenses/>
 */
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
