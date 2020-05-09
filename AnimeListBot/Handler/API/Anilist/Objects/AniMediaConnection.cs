using GraphQL.Types.Relay.DataObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler.Anilist
{
    public class AniMediaConnection
    {
        public List<AniMediaResponse.AniMedia> nodes { get; set; }
        public AniPageInfo pageInfo { get; set; }
    }
}
