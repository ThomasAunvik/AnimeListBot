using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
    public interface ICharacter
    {
        int id { get; }
        ICharacterName name { get; }
        ICharacterImage image { get; }

        string description { get; }
        bool asHtml { get; }

        bool isFavourite { get; }
        string siteUrl { get; }

        IMediaConnection media { get; }
    }
}
