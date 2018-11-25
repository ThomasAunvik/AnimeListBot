using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
    public interface ICharacterEdge : ICharacter
    {
        new int id { get; }
        CharacterRole role { get; }

        List<IStaff> voiceActors { get; }

        new List<IMedia> media { get; }

        int favouriteOrder { get; }
    }
}
