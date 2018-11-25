using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
    public interface IFavourites
    {
        IMediaConnection anime { get; }
        IMediaConnection manga { get; }

        ICharacterConnection characters { get; }

        IStaffConnection staff { get; }

        IStudioConnection studios { get; }
    }
}
