using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler.Anilist
{
    public class AnilistUser : IAnilistUser
    {
        public int id { get; set; }
        public string name { get; set; }
        public string siteUrl { get; set; }

        public AnilistUserAvatar Avatar { get; set; }

        public AnilistUserStats Stats { get; set; }
    }
}
