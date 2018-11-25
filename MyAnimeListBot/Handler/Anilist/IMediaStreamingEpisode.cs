using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
    public interface IMediaStreamingEpisode
    {
        string title { get; }
        string thumbnail { get; }
        string url { get; }
        string site { get; }
    }
}
