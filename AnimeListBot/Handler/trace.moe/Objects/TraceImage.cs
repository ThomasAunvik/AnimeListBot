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
