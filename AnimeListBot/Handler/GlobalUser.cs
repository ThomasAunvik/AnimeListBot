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
        public SaveDiscordUser savedUser;
        public string Username;

        public ulong userID;
        public List<ServerUser> serverUsers = new List<ServerUser>();
        
        public UserProfile malProfile;
        public IAniUser anilistProfile;

        public bool toggleAnilist = false;

        public string MAL_Username;
        public string Anilist_Username;

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
            return toggleAnilist ? Anilist_Username : MAL_Username;
        }

        public string GetAnimelistThumbnail()
        {
            return toggleAnilist ? Anilist_imageURL : MAL_imageURL;
        }

        public string GetAnimelistLink()
        {
            return toggleAnilist ? anilistProfile.siteUrl : malProfile.URL;
        }

        #region AnimeStats

        public decimal GetAnimeWatchDays()
        {
            return decimal.Round(toggleAnilist ? Anilist_minutesWatchedAnime / (decimal)60.0 / (decimal)24.0 : MAL_daysWatchedAnime.GetValueOrDefault(), 1);
        }

        public decimal GetAnimeMeanScore()
        {
            return toggleAnilist ? (anilistProfile?.Stats?.animeListScores?.meanScore).GetValueOrDefault() / (decimal)10.0 : malProfile.AnimeStatistics.MeanScore.GetValueOrDefault();
        }

        public int GetAnimeTotalEntries()
        {
            int totalEntries = 0;
            anilistProfile?.Stats?.animeStatusDistribution?.ForEach(x => totalEntries += x.amount);
            return toggleAnilist ? totalEntries : malProfile.AnimeStatistics.TotalEntries.GetValueOrDefault();
        }

        public int GetAnimeEpisodesWatched()
        {
            return toggleAnilist ? 0 : malProfile.AnimeStatistics.EpisodesWatched.GetValueOrDefault();
        }

        public int GetAnimeRewatched()
        {
            return toggleAnilist ? (anilistProfile?.Stats?.animeStatusDistribution?.Find(x => x.status == AniMediaListStatus.REPEATING)?.amount).GetValueOrDefault() : malProfile.AnimeStatistics.Rewatched.GetValueOrDefault();
        }

        public int GetAnimeWatching()
        {
            return toggleAnilist ? (anilistProfile?.Stats?.animeStatusDistribution?.Find(x => x.status == AniMediaListStatus.CURRENT)?.amount).GetValueOrDefault() : malProfile.AnimeStatistics.Watching.GetValueOrDefault();
        }

        public int GetAnimeCompleted()
        {
            return toggleAnilist ? (anilistProfile?.Stats?.animeStatusDistribution?.Find(x => x.status == AniMediaListStatus.COMPLETED)?.amount).GetValueOrDefault() : malProfile.AnimeStatistics.Completed.GetValueOrDefault();
        }

        public int GetAnimeOnHold()
        {
            return toggleAnilist ? (anilistProfile?.Stats?.animeStatusDistribution?.Find(x => x.status == AniMediaListStatus.PAUSED)?.amount).GetValueOrDefault() : malProfile.AnimeStatistics.OnHold.GetValueOrDefault();
        }

        public int GetAnimeDropped()
        {
            return toggleAnilist ? (anilistProfile?.Stats?.animeStatusDistribution?.Find(x => x.status == AniMediaListStatus.DROPPED)?.amount).GetValueOrDefault() : malProfile.AnimeStatistics.Dropped.GetValueOrDefault();
        }

        public int GetAnimePlanToWatch()
        {
            return toggleAnilist ? (anilistProfile?.Stats?.animeStatusDistribution?.Find(x => x.status == AniMediaListStatus.PLANNING)?.amount).GetValueOrDefault() : malProfile.AnimeStatistics.PlanToWatch.GetValueOrDefault();
        }

        #endregion

        #region MangaStats

        public decimal GetMangaReadDays()
        {
            return decimal.Round(toggleAnilist ? Anilist_daysChaptersRead : MAL_daysReadManga.GetValueOrDefault(), 1);
        }

        public decimal GetMangaMeanScore()
        {
            return toggleAnilist ? (anilistProfile?.Stats?.mangaListScores?.meanScore).GetValueOrDefault() / (decimal)10.0 : malProfile.MangaStatistics.MeanScore.GetValueOrDefault();
        }

        public int GetMangaTotalEntries()
        {
            int totalEntries = 0;
            if(anilistProfile != null && toggleAnilist) anilistProfile.Stats.mangaStatusDistribution.ForEach(x => totalEntries += x.amount);

            return toggleAnilist ? totalEntries : malProfile.MangaStatistics.TotalEntries.GetValueOrDefault();
        }

        public int GetMangaChaptersRead()
        {
            return toggleAnilist ? (anilistProfile?.Stats?.chaptersRead).GetValueOrDefault() : malProfile.MangaStatistics.ChaptersRead.GetValueOrDefault();
        }

        public int GetMangaVolumesRead()
        {
            return toggleAnilist ? 0 : malProfile.MangaStatistics.VolumesRead.GetValueOrDefault();
        }

        public int GetMangaReread()
        {
            return toggleAnilist ? (anilistProfile?.Stats?.mangaStatusDistribution?.Find(x => x.status == AniMediaListStatus.REPEATING)?.amount).GetValueOrDefault() : malProfile.MangaStatistics.Reread.GetValueOrDefault();
        }

        public int GetMangaReading()
        {
            return toggleAnilist ? (anilistProfile?.Stats?.mangaStatusDistribution?.Find(x => x.status == AniMediaListStatus.CURRENT)?.amount).GetValueOrDefault() : malProfile.MangaStatistics.Reading.GetValueOrDefault();
        }

        public int GetMangaCompleted()
        {
            return toggleAnilist ? (anilistProfile?.Stats?.mangaStatusDistribution?.Find(x => x.status == AniMediaListStatus.COMPLETED)?.amount).GetValueOrDefault() : malProfile.MangaStatistics.Completed.GetValueOrDefault();
        }

        public int GetMangaOnHold()
        {
            return toggleAnilist ? (anilistProfile?.Stats?.mangaStatusDistribution?.Find(x => x.status == AniMediaListStatus.PAUSED)?.amount).GetValueOrDefault() : malProfile.MangaStatistics.OnHold.GetValueOrDefault();
        }

        public int GetMangaDropped()
        {
            return toggleAnilist ? (anilistProfile?.Stats?.mangaStatusDistribution?.Find(x => x.status == AniMediaListStatus.DROPPED)?.amount).GetValueOrDefault() : malProfile.MangaStatistics.Dropped.GetValueOrDefault();
        }

        public int GetMangaPlanToRead()
        {
            return toggleAnilist ? (anilistProfile?.Stats?.mangaStatusDistribution?.Find(x => x.status == AniMediaListStatus.PLANNING)?.amount).GetValueOrDefault() : malProfile.MangaStatistics.PlanToRead.GetValueOrDefault();
        }

        #endregion

        public async Task UpdateMALInfo()
        {
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

                    toggleAnilist = save.toggleAnilist;

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
