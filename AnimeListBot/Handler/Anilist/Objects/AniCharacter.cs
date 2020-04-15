namespace AnimeListBot.Handler.Anilist
{
    public class AniCharacterResponse
    {
        public AniCharacter Character { get; set; }

        public class AniCharacter : IAniCharacter
        {
            public int id { get; set; }

            public AniCharacterName name { get; set; }

            public AniCharacterImage image { get; set; }

            public string description { get; set; }

            public string siteUrl { get; set; }
        }
    }
}
