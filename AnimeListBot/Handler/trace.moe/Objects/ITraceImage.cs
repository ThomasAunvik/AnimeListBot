using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler.trace.moe.Objects
{
    public interface ITraceImage
    {
        long RawDocsCount { get; }
        long RawDocsSearchTime { get; }
        long ReRankSearchTIme { get; }
        bool CacheHit { get; }
        int trial { get; }

        List<TraceDoc> docs { get; }

        int limit { get; }
        int limit_ttl { get; }
        int quota { get; }
        int quata_ttl { get; }
    }
}
