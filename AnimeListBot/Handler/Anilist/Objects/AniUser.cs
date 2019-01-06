namespace AnimeListBot.Handler.Anilist
{
    public class AniUser : IAniUser
    {
        public int id { get; set; }
        public string name { get; set; }
        public string siteUrl { get; set; }

        public AniUserAvatar Avatar { get; set; }

        public AniUserStats Stats { get; set; }
    }
}
