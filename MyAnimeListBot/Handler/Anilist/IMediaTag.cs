using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
    public interface IMediaTag
    {
        int id { get; }
        string name { get; }
        string description { get; }
        string category { get; }
        int rank { get; }
        bool isGeneralSpoiler { get; }
        bool isAdult { get; }
    }
}
