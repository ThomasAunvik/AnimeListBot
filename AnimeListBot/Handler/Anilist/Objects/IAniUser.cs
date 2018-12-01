using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler.Anilist
{
     public interface IAniUser
     {
         int id { get; }
         string name { get; }
         string siteUrl { get; }

         AnilistUserAvatar Avatar { get; }
        
         AnilistUserStats Stats { get; }
    }
}
