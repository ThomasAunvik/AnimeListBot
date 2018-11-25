using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
    public interface ICharacterConnection
    {
        List<ICharacterEdge> edges { get; }
        List<ICharacter> nodes { get; }
        IPageInfo pageInfo { get; }
    }
}
