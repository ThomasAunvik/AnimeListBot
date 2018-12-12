using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler.trace.moe.Objects
{
    public class TraceResult : ITraceResult
    {
        public ITraceImage trace { get; set; }

        public bool failed { get; set; }
        public string errorMessage { get; set; }
        public string errorDescription { get; set; }
    }
}
