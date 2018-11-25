using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
    public interface IStaffConnection
    {
        List<IStaffEdge> edges { get; }
        List<IStaff> nodes { get; }

        IPageInfo pageInfo { get; }
    }
}
