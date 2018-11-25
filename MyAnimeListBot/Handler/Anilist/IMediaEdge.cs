using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
    public interface IMediaEdge : IMedia
    {
        new int id { get; }
        MediaRelation relationType { get; }

        bool isMainStudio { get; }

        new List<ICharacter> characters { get; }

        CharacterRole characterRole { get; }

        string staffRole { get; }

        List<IStaff> voiceActors { get; }

        int favouriteOrder { get; }
    }
}
