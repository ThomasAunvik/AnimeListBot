namespace AnimeListBot.Handler.Anilist
{
    public interface IAniStaff
    {
        int id { get; }
        AniStaffName name { get; }
        AniStaffLanguage language { get; }
        AniStaffImage image { get; }
        string description { get; }
        string siteUrl { get; }
    }
}
