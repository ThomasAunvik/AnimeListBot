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
    public class AniMediaResponse
    {
        public AniMedia Media { get; set; }

        public class AniMedia : IAniMedia
        {
            public int? id { get; set; }

            public int? idMal { get; set; }

            public AniMediaTitle title { get; set; }

            public AniMediaType? type { get; set; }

            public AniMediaFormat? format { get; set; }

            public AniMediaStatus? status { get; set; }

            public string description { get; set; }

            public AniFuzzyDate startDate { get; set; }

            public AniFuzzyDate endDate { get; set; }

            public int? episodes { get; set; }

            public int? chapters { get; set; }

            public int? volumes { get; set; }

            public AniMediaCoverImage coverImage { get; set; }

            public string siteUrl { get; set; }
        }
    }
}
