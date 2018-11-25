using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
    public interface IFuzzyDate
    {
        int year { get; }
        int month { get; }
        int day { get; }
    }
}
