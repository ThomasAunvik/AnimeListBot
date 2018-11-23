using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler
{
    [Serializable]
    public class SaveDiscordUser
    {
        public string Username;
        public ulong UserID;

        public string MAL_Username;

        public string imageURL;
        public decimal? daysWatchedAnime = 0;
        public ulong currentRankId = 0;

        public SaveDiscordUser(GlobalUser user)
        {
            if (user != null)
            {
                UserID = user.userID;
                Username = user.Username;
                MAL_Username = user.MAL_Username;

                imageURL = user.imageURL;
                daysWatchedAnime = user.daysWatchedAnime;
            }
        }
    }
}
