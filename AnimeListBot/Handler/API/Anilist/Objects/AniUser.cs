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
    public class AniUserResponse
    {
        public AniUser User { get; set; }

        public class AniUser : IAniUser
        {
            public int id { get; set; }
            public string name { get; set; }
            public string siteUrl { get; set; }

            public AniUserAvatar Avatar { get; set; }

            public AniUserStatisticTypes statistics { get; set; }
        }
    }
}
