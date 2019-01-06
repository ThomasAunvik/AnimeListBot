using System.Collections.Generic;

namespace AnimeListBot.Handler.Anilist
{
    public class AniCharacterName
    {
        public string first { get; set; }
        public string last { get; set; }
        public string native { get; set; }
        public List<string> alternative { get; set; }
    }
}
