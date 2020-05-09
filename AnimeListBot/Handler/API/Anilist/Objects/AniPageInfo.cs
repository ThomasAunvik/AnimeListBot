using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler.Anilist
{
    public class AniPageInfo
    {
        public int total { get; set; }
        public int perPage { get; set; }
        public int currentPage { get; set; }
        public int lastPage { get; set; }
        public bool hasNextPage { get; set; }
    }
}
