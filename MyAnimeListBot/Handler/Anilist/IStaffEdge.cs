using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
    public interface IStaffEdge : IStaff
    {
        new int id { get; }
        string role { get; }
        int favouriteOrder { get; }
    }
}
