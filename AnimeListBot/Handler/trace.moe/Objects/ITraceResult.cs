using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler.trace.moe.Objects
{
    public interface ITraceResult
    {
        ITraceImage trace { get; }

        bool failed { get; }
        string errorMessage { get; }
        string errorDescription { get; }
    }
}
