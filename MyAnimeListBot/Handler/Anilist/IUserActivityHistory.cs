using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
    public interface IUserActivityHistory
    {
        int date { get; }
        int amount { get; }
        int level { get; }
    }
}
