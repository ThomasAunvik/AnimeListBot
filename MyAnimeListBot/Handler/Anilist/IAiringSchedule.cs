using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
     public interface IAiringSchedule
     {
         int id { get; }
         int airingAt { get; }
         int timeUntilAiring { get; }
         int episode { get; }
         int mediaId { get; }
         IMedia media { get; }
     }
}
