using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
    public interface IYearStats
    {
        int year { get; }
        int amount { get; }
        int meanScore { get; }
    }
}
