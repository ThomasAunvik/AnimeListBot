using Discord;
using Discord.WebSocket;
using JikanDotNet;
using AnimeListBot.Handler.Anilist;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AnimeListBot.Handler
{
    public class GlobalUser
    {
        public enum AnimeList
        {
            MAL,
            Anilist
        }

        public SaveDiscordUser savedUser;
        public string Username;

        public ulong userID;
        public List<ServerUser> serverUsers = new List<ServerUser>();
        
        public UserProfile malProfile;
        public IAniUser anilistProfile;
        
        public AnimeList animeList;

        public string MAL_Username = string.Empty;
        public string Anilist_Username = string.Empty;

        public string MAL_imageURL;
        public decimal? MAL_daysWatchedAnime = 0;
        public decimal? MAL_daysReadManga = 0;

        public string Anilist_imageURL;
        public decimal Anilist_minutesWatchedAnime = 0;
        public decimal Anilist_daysChaptersRead = 0;

        public GlobalUser(IUser user)
        {
            Username = user.Username;
            userID = user.Id;

            LoadData();
            SaveData();
        }

        public string GetAnimelistUsername()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return MAL_Username == null ? "" : MAL_Username;
                case AnimeList.Anilist:
                    return Anilist_Username == null ? "" : Anilist_Username;
                default:
                    return "";
            }
        }

        public string GetAnimelistThumbnail()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return MAL_imageURL;
                case AnimeList.Anilist:
                    return Anilist_imageURL;
                default:
                    return "";
            }
        }

        public string GetAnimelistLink()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile.URL;
                case AnimeList.Anilist:
                    return anilistProfile.siteUrl;
                default:
                    return "";
            }
        }

        #region AnimeStats

        public decimal GetAnimeWatchDays()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    decimal mal_days = MAL_daysWatchedAnime.GetValueOrDefault();
                    return decimal.Round(mal_days, 1);
                case AnimeList.Anilist:
                    decimal ani_days = Anilist_minutesWatchedAnime / (decimal)60.0 / (decimal)24.0;
                    return decimal.Round(ani_days, 1);
                default:
                    return 0;
            }
        }

        public decimal GetAnimeMeanScore()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile.AnimeStatistics.MeanScore.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile?.Stats?.animeListScores?.meanScore).GetValueOrDefault() / (decimal)10.0;
                default:
                    return 0;
            }
        }

        public int GetAnimeTotalEntries()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile.AnimeStatistics.TotalEntries.GetValueOrDefault();
                case AnimeList.Anilist:
                    int ani_entries = 0;
                    anilistProfile?.Stats?.animeStatusDistribution?.ForEach(x => ani_entries += x.amount);
                    return ani_entries;
                default:
                    return 0;
            }
        }

        public int GetAnimeEpisodesWatched()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile.AnimeStatistics.EpisodesWatched.GetValueOrDefault();
                case AnimeList.Anilist:
                    return 0;
                default:
                    return 0;
            }
        }

        public int GetAnimeRewatched()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile.AnimeStatistics.Rewatched.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile?.Stats?.animeStatusDistribution?.Find(x => x.status == AniMediaListStatus.REPEATING)?.amount).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetAnimeWatching()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile.AnimeStatistics.Watching.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile?.Stats?.animeStatusDistribution?.Find(x => x.status == AniMediaListStatus.CURRENT)?.amount).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetAnimeCompleted()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile.AnimeStatistics.Completed.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile?.Stats?.animeStatusDistribution?.Find(x => x.status == AniMediaListStatus.COMPLETED)?.amount).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetAnimeOnHold()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile.AnimeStatistics.OnHold.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile?.Stats?.animeStatusDistribution?.Find(x => x.status == AniMediaListStatus.PAUSED)?.amount).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetAnimeDropped()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile.AnimeStatistics.Dropped.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile?.Stats?.animeStatusDistribution?.Find(x => x.status == AniMediaListStatus.DROPPED)?.amount).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetAnimePlanToWatch()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile.AnimeStatistics.PlanToWatch.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile?.Stats?.animeStatusDistribution?.Find(x => x.status == AniMediaListStatus.PLANNING)?.amount).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        #endregion

        #region MangaStats

        public decimal GetMangaReadDays()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    decimal mal_mangaRead = decimal.Round(MAL_daysReadManga.GetValueOrDefault(), 1);
                    return mal_mangaRead;
                case AnimeList.Anilist:
                    return Math.Round(Anilist_daysChaptersRead, 1);
                default:
                    return 0;
            }
        }

        public decimal GetMangaMeanScore()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile.MangaStatistics.MeanScore.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile?.Stats?.mangaListScores?.meanScore).GetValueOrDefault() / (decimal)10.0;
                default:
                    return 0;
            }
        }

        public int GetMangaTotalEntries()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile.MangaStatistics.TotalEntries.GetValueOrDefault();
                case AnimeList.Anilist:
                    int ani_totalMangaEntries = 0;
                    anilistProfile.Stats.mangaStatusDistribution.ForEach(x => ani_totalMangaEntries += x.amount);
                    return ani_totalMangaEntries;
                default:
                    return 0;
            }
        }

        public int GetMangaChaptersRead()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile.MangaStatistics.ChaptersRead.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile?.Stats?.chaptersRead).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetMangaVolumesRead()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile.MangaStatistics.VolumesRead.GetValueOrDefault();
                case AnimeList.Anilist:
                    return 0;
                default:
                    return 0;
            }
        }

        public int GetMangaReread()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile.MangaStatistics.Reread.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile?.Stats?.mangaStatusDistribution?.Find(x => x.status == AniMediaListStatus.REPEATING)?.amount).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetMangaReading()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile.MangaStatistics.Reading.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile?.Stats?.mangaStatusDistribution?.Find(x => x.status == AniMediaListStatus.CURRENT)?.amount).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetMangaCompleted()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile.MangaStatistics.Completed.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile?.Stats?.mangaStatusDistribution?.Find(x => x.status == AniMediaListStatus.COMPLETED)?.amount).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetMangaOnHold()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile.MangaStatistics.OnHold.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile?.Stats?.mangaStatusDistribution?.Find(x => x.status == AniMediaListStatus.PAUSED)?.amount).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetMangaDropped()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile.MangaStatistics.Dropped.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile?.Stats?.mangaStatusDistribution?.Find(x => x.status == AniMediaListStatus.DROPPED)?.amount).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetMangaPlanToRead()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    return malProfile.MangaStatistics.PlanToRead.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile?.Stats?.mangaStatusDistribution?.Find(x => x.status == AniMediaListStatus.PLANNING)?.amount).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        #endregion

        public async Task UpdateCurrentAnimelist()
        {
            switch (animeList)
            {
                case AnimeList.MAL:
                    await UpdateMALInfo();
                    break;
                case AnimeList.Anilist:
                    await UpdateAnilistInfo();
                    break;
            }
        }

        public async Task UpdateMALInfo()
        {
            if (string.IsNullOrEmpty(MAL_Username))
            {
                malProfile = null;
                return;
            }

            this.malProfile = null;
            UserProfile profile = await Program._jikan.GetUserProfile(MAL_Username);
            if (profile != null)
            {
                this.malProfile = profile;
                MAL_Username = profile.Username;
                MAL_daysWatchedAnime = profile.AnimeStatistics.DaysWatched;
                MAL_daysReadManga = profile.MangaStatistics.DaysRead;
                MAL_imageURL = profile.ImageURL;
            }

            SaveData();
        }

        public async Task UpdateAnilistInfo()
        {
            if (string.IsNullOrEmpty(Anilist_Username))
            {
                anilistProfile = null;
                return;
            }

            this.anilistProfile = null;
            IAniUser anilistUser = await AniUserQuery.GetUser(Anilist_Username);
            if(anilistUser != null)
            {
                anilistProfile = anilistUser;
                Anilist_Username = anilistUser.name;
                
                Anilist_minutesWatchedAnime = (anilistUser?.Stats?.watchedTime).GetValueOrDefault();
                Anilist_daysChaptersRead = decimal.Multiply((anilistUser?.Stats?.chaptersRead).GetValueOrDefault(), (decimal)0.00556);
                Anilist_imageURL = anilistUser?.Avatar?.large;
            }
        }

        public SaveDiscordUser LoadData()
        {
            if(File.Exists("DiscordUserFiles/" + userID + ".json"))
            {
                string JSONstring = File.ReadAllText("DiscordUserFiles/" + userID + ".json");
                SaveDiscordUser save = JsonConvert.DeserializeObject<SaveDiscordUser>(JSONstring);
                if(save != null)
                {
                    Username = save.Username;
                    userID = save.UserID;

                    animeList = save.animeList;
                    if (save.toggleAnilist) animeList = AnimeList.Anilist;

                    MAL_Username = save.MAL_Username;
                    MAL_daysWatchedAnime = save.MAL_daysWatchedAnime;
                    MAL_daysReadManga = save.MAL_daysReadManga;

                    Anilist_Username = save.Anilist_Username;
                    Anilist_minutesWatchedAnime = save.Anilist_minutesWatchedAnime;
                    Anilist_daysChaptersRead = save.Anilist_daysChaptersRead;
                    return save;
                }
            }
            return null;
        }

        public void SaveData()
        {
            savedUser = new SaveDiscordUser(this);

            string outputJSON = JsonConvert.SerializeObject(savedUser);

            string jsonFormatted = JToken.Parse(outputJSON).ToString(Formatting.Indented);

            FileStream stream = null;
            if (!Directory.Exists("DiscordUserFiles/"))
                Directory.CreateDirectory("DiscordUserFiles/");
            if (!File.Exists("DiscordUserFiles/" + userID + ".json"))
                stream = File.Create("DiscordUserFiles/" + userID + ".json");

            if(stream != null)
                stream.Close();
            File.WriteAllText("DiscordUserFiles/" + userID + ".json", jsonFormatted);
        }

        public static void DeleteServerFile(SocketUser user)
        {
            if(File.Exists("DiscordUserFiles / " + user.Id + ".json"))
                File.Delete("DiscordUserFiles / " + user.Id + ".json");
        }
    }
}
