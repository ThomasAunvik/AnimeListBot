using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
    public interface IPageInfo
    {
        int total { get; }
        int perPage { get; }
        int currentPage { get; }
        int lastPage { get; }
        bool hasNextPage { get; }
    }
}
