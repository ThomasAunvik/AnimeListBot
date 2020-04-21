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
    public class AniCharacterResponse
    {
        public AniCharacter Character { get; set; }

        public class AniCharacter : IAniCharacter
        {
            public int id { get; set; }

            public AniCharacterName name { get; set; }

            public AniCharacterImage image { get; set; }

            public string description { get; set; }

            public string siteUrl { get; set; }
        }
    }
}
