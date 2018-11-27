using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler
{
    [Serializable]
    public class SaveDiscordUser
    {
        public string Username;
        public ulong UserID;

        public bool toggleAnilist = false;

        public string MAL_Username;
        public string Anilist_Username;

        public string MAL_imageURL;
        public decimal? MAL_daysWatchedAnime = 0;
        public decimal? MAL_daysReadManga = 0;

        public string Anilist_imageURL;
        public decimal Anilist_minutesWatchedAnime = 0;
        public decimal Anilist_daysChaptersRead = 0;

        public ulong currentRankId = 0;

        public SaveDiscordUser(GlobalUser user)
        {
            if (user != null)
            {
                UserID = user.userID;
                Username = user.Username;

                toggleAnilist = user.toggleAnilist;

                MAL_Username = user.MAL_Username;
                Anilist_Username = user.Anilist_Username; 

                MAL_imageURL = user.MAL_imageURL;
                MAL_daysWatchedAnime = user.MAL_daysWatchedAnime;
                MAL_daysReadManga = user.MAL_daysReadManga;

                Anilist_imageURL = user.Anilist_imageURL;
                Anilist_minutesWatchedAnime = user.Anilist_minutesWatchedAnime;
                Anilist_daysChaptersRead = user.Anilist_daysChaptersRead;
            }
        }
    }
}
