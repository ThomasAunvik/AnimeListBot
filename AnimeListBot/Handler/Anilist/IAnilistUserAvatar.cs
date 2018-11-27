using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler.Anilist
{
    public interface IAnilistUserAvatar
    {
        string large { get; }
        string medium { get; }
    }
}
