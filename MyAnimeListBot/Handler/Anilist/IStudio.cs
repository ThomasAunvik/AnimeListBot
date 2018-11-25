using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
    public interface IStudio
    {
        int id { get; }
        string name { get; }
        IMediaConnection media { get; }
        string siteUrl { get; }
        bool isFavourite { get; }
    }
}