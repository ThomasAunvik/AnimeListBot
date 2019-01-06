namespace AnimeListBot.Handler.Anilist
{
    public interface IAniCharacter
    {
        int id { get; }
        AniCharacterName name { get; }

        AniCharacterImage image { get; }
        string description { get; }
        string siteUrl { get; }
    }
}
