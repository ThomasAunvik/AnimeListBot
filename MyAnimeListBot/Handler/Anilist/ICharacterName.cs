using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
    public interface ICharacterName
    {
        string first { get; }
        string last { get; }
        string native { get; }
        List<string> alternative { get; }
    }
}
