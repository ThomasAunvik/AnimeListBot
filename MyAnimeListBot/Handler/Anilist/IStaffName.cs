using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
    public interface IStaffName
    {
        string first { get; }
        string last { get; }
        string native { get; }
    }
}
