using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
    public interface IStudioEdge : IStudio
    {
        new int id { get; }
        bool isMain { get; }
        int favouriteOrder { get; }
    }
}
