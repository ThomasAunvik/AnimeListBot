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
using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler.trace.moe.Objects
{
    [Serializable]
    public class TraceDoc
    {
        public decimal? from { get; set; }
        public decimal? to { get; set; }
        public int? anilist_id { get; set; }
        public decimal? at { get; set; }
        public string season { get; set; }
        public string anime { get; set; }
        public string filename { get; set; }
        public string episode { get; set; }
        public string tokenthumb { get; set; }
        public decimal? similarity { get; set; }
        public string title { get; set; }
        public string title_native { get; set; }
        public string title_chinese { get; set; }
        public string title_english { get; set; }
        public string title_romanji { get; set; }
        public string mal_id { get; set; }
        public List<string> synonyms { get; set; }
        public List<string> synonyms_chinese { get; set; }
        public bool? is_adult { get; set; }
    }
}
