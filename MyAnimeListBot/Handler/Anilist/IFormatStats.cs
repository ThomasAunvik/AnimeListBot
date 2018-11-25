using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
    public interface IFormatStats
    {
        MediaFormat format { get; }
        int amount { get; }
    }
}
