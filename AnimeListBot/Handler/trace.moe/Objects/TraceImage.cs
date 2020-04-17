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
    public class TraceImage : ITraceImage
    {
        public long RawDocsCount { get; set; }

        public long RawDocsSearchTime { get; set; }

        public long ReRankSearchTIme { get; set; }

        public bool CacheHit { get; set; }

        public int trial { get; set; }

        public List<TraceDoc> docs { get; set; }

        public int limit { get; set; }

        public int limit_ttl { get; set; }

        public int quota { get; set; }

        public int quata_ttl { get; set; }
    }
}
