using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
    public interface IMediaTrailer
    {
        string id { get; }
        string site { get; }
    }
}
