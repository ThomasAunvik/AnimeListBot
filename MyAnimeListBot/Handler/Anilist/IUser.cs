using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
     public interface IUser
     {
        int id { get; }
         string name { get; }

         string about { get; }
         bool asHtml { get; }

         IUserAvatar avatar { get; }
         string bannerImage { get; }

         bool isFollowing { get; }

         IUserOptions options { get; }

         IMediaListOptions mediaListOptions { get; }

         IFavourites favourites { get; }
         int page { get; }

         IUserStats stats { get; }

         int unreadNotificationCount { get; }

         string siteUrl { get; }

         string moderatorStatus { get; }

         int updatedAt { get; }
     }
}
