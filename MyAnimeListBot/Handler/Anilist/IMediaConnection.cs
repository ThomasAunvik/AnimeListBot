using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
    public interface IMediaConnection
    {
        List<IMediaEdge> edges { get; }

        List<IMedia> nodes { get; }

        IPageInfo pageInfo { get; }
    }
}
