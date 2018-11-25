using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
    public interface IStudioConnection
    {
        List<IStudioEdge> edges { get; }
        List<IStudio> nodes { get; }

        IPageInfo pageInfo { get; }
    }
}
