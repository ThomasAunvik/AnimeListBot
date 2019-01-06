namespace AnimeListBot.Handler.Anilist
{
    public class AniStaff : IAniStaff
    {
        public int id { get; set; }

        public AniStaffName name { get; set; }

        public AniStaffLanguage language { get; set; }

        public AniStaffImage image { get; set; }

        public string description { get; set; }

        public string siteUrl { get; set; }
    }
}
