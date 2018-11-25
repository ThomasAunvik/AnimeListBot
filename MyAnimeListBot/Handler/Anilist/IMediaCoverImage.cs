using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
    public interface IMediaCoverImage
    {
        string large { get; }
        string medium { get; }
    }
}
