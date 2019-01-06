namespace AnimeListBot.Handler.Anilist
{
     public interface IAniUser
     {
         int id { get; }
         string name { get; }
         string siteUrl { get; }

         AniUserAvatar Avatar { get; }
        
         AniUserStats Stats { get; }
    }
}
