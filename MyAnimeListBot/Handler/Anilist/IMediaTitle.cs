using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
    public interface IMediaTitle
    {
        string romanji { get; }
        bool romanjiStylised { get; }

        string english { get; }

        bool englishStylised { get; }

        string native { get; }
        bool nativeStylised { get; }

        string userPreferred { get; }
    }
}
