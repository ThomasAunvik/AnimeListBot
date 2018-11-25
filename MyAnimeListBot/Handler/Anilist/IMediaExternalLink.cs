using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
    public interface IMediaExternalLink
    {
        int id { get; }
        string url { get; }
        string site { get; }
    }
}
