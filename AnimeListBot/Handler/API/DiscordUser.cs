/*
 * This file is part of AnimeList Bot
 *
 * AnimeList Bot is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * AnimeList Bot is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with AnimeList Bot.  If not, see <https://www.gnu.org/licenses/>
 */
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
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnimeListBot.Handler
{
    public class DiscordUser
    {
        public enum AnimeList
        {
            MAL,
            Anilist
        }

        public ulong UserId { get; set; }
        
        [NotMapped]
        public UserProfile malProfile;
        public string MalUsername { get; set; } = string.Empty;
        [NotMapped]
        public IAniUser anilistProfile;
        public string AnilistUsername { get; set; } = string.Empty;

        public AnimeList ListPreference { get; set; }

        public double AnimeDays { get; set; }
        public double MangaDays { get; set; }

        public List<long> Servers { get; set; } = new List<long>();

        public DiscordUser() { }

        // Most of the reasons you do this part is to create a new user and upload it to the db automaticly
        public DiscordUser(IUser user)
        {
            if (user == null) return;
            UserId = user.Id;
        }

        public void OverrideData(DiscordUser user)
        {
            MalUsername = user.MalUsername;
            AnilistUsername = user.AnilistUsername;
            AnimeDays = user.AnimeDays;
            MangaDays = user.MangaDays;
        }

        public SocketUser GetUser() { return Program._client.GetUser(UserId); }

        public static async Task<DiscordUser> CheckAndCreateUser(ulong user_id)
        {
            DiscordUser discordUser;
            if (!DatabaseRequest.DoesUserIdExist(user_id))
                await DatabaseRequest.CreateUser(discordUser = new DiscordUser(Program._client.GetUser(user_id)));
            else discordUser = await DatabaseRequest.GetUserById(user_id);
            return discordUser;
        }

        public async Task CreateUserDatabase()
        {
            if (!DatabaseRequest.DoesUserIdExist(UserId))
                await DatabaseRequest.CreateUser(this);
            else await UpdateDatabase();
        }

        public async Task UpdateDatabase()
        {
            if (DatabaseRequest.DoesUserIdExist(UserId))
                await DatabaseRequest.UpdateUser(this);
        }

        public async Task RefreshMutualGuilds()
        {
            SocketUser user = GetUser();
            if (user == null) return;
            if (Servers == null) Servers = new List<long>();
            List<SocketGuild> mutualGuilds = user.MutualGuilds.ToList();
            for(int guildIndex = 0; guildIndex < mutualGuilds.Count; guildIndex++)
            {
                long guildId = (long)mutualGuilds[guildIndex].Id;
                if (!Servers.Contains(guildId))
                {
                    Servers.Add(guildId);
                }
            }
            await UpdateDatabase();
        }

        public string GetAnimelistUsername()
        {
            switch (ListPreference)
            {
                case AnimeList.MAL:
                    return MalUsername;
                case AnimeList.Anilist:
                    return AnilistUsername;
                default:
                    return "";
            }
        }

        public string GetAnimelistThumbnail()
        {
            switch (ListPreference)
            {
                case AnimeList.MAL:
                    return malProfile.ImageURL;
                case AnimeList.Anilist:
                    return anilistProfile.Avatar?.large;
                default:
                    return "";
            }
        }

        public string GetAnimelistLink()
        {
            switch (ListPreference)
            {
                case AnimeList.MAL:
                    return malProfile?.URL;
                case AnimeList.Anilist:
                    return anilistProfile?.siteUrl;
                default:
                    return "";
            }
        }

        #region AnimeStats

        public (ulong, double) GetAnimeServerRank(DiscordServer server)
        {
            if (server.AnimeroleId == null || server.AnimeroleId.Count < 1) return (0, 0);

            double animeDays = GetAnimeWatchDays();
            for(int roleIndex = 0; roleIndex < server.AnimeroleDays.Count; roleIndex++)
            {
                if (animeDays < server.AnimeroleDays[roleIndex]) {
                    if (roleIndex < 1) return (0,0);
                    roleIndex--;
                    return ((ulong)server.AnimeroleId[roleIndex], server.AnimeroleDays[roleIndex]);
                }
            }
            return ((ulong)server.AnimeroleId[server.AnimeroleId.Count - 1], server.AnimeroleDays[server.AnimeroleId.Count - 1]);
        }

        public double GetAnimeWatchDays()
        {
            return AnimeDays;
        }

        public float GetAnimeMeanScore()
        {
            switch (ListPreference)
            {
                case AnimeList.MAL:
                    return (float)malProfile.AnimeStatistics.MeanScore.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile?.statistics.anime?.meanScore).GetValueOrDefault() / 10.0f;
                default:
                    return 0;
            }
        }

        public int GetAnimeTotalEntries()
        {
            switch (ListPreference)
            {
                case AnimeList.MAL:
                    return malProfile.AnimeStatistics.TotalEntries.GetValueOrDefault();
                case AnimeList.Anilist:
                    return anilistProfile.statistics.anime.count;
                default:
                    return 0;
            }
        }

        public int GetAnimeEpisodesWatched()
        {
            switch (ListPreference)
            {
                case AnimeList.MAL:
                    return malProfile.AnimeStatistics.EpisodesWatched.GetValueOrDefault();
                case AnimeList.Anilist:
                    return anilistProfile.statistics.anime.episodesWatched;
                default:
                    return 0;
            }
        }

        public int GetAnimeRewatched()
        {
            switch (ListPreference)
            {
                case AnimeList.MAL:
                    return malProfile.AnimeStatistics.Rewatched.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile.statistics.anime.statuses.Find(x => x.status == AniMediaListStatus.REPEATING)?.count).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetAnimeWatching()
        {
            switch (ListPreference)
            {
                case AnimeList.MAL:
                    return malProfile.AnimeStatistics.Watching.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile.statistics.anime.statuses.Find(x => x.status == AniMediaListStatus.CURRENT)?.count).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetAnimeCompleted()
        {
            switch (ListPreference)
            {
                case AnimeList.MAL:
                    return malProfile.AnimeStatistics.Completed.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile.statistics.anime.statuses.Find(x => x.status == AniMediaListStatus.COMPLETED)?.count).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetAnimeOnHold()
        {
            switch (ListPreference)
            {
                case AnimeList.MAL:
                    return malProfile.AnimeStatistics.OnHold.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile.statistics.anime.statuses.Find(x => x.status == AniMediaListStatus.PAUSED)?.count).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetAnimeDropped()
        {
            switch (ListPreference)
            {
                case AnimeList.MAL:
                    return malProfile.AnimeStatistics.Dropped.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile.statistics.anime.statuses.Find(x => x.status == AniMediaListStatus.DROPPED)?.count).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetAnimePlanToWatch()
        {
            switch (ListPreference)
            {
                case AnimeList.MAL:
                    return malProfile.AnimeStatistics.PlanToWatch.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile.statistics.anime.statuses.Find(x => x.status == AniMediaListStatus.PLANNING)?.count).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        #endregion

        #region MangaStats

        public (ulong, double) GetMangaServerRank(DiscordServer server)
        {
            if (server.MangaroleId == null || server.MangaroleId.Count < 1) return (0, 0);

            double mangaDays = GetMangaReadDays();
            for (int roleIndex = 0; roleIndex < server.MangaroleDays.Count; roleIndex++)
            {
                if (mangaDays < server.MangaroleDays[roleIndex])
                {
                    if (roleIndex < 1) return (0,0);
                    roleIndex--;
                    return ((ulong)server.MangaroleId[roleIndex], server.MangaroleDays[roleIndex]);
                }
            }
            return ((ulong)server.MangaroleId[server.MangaroleId.Count - 1], server.MangaroleDays[server.MangaroleId.Count - 1]);
        }

        public double GetMangaReadDays()
        {
            return MangaDays;
        }

        public float GetMangaMeanScore()
        {
            switch (ListPreference)
            {
                case AnimeList.MAL:
                    return (float)malProfile.MangaStatistics.MeanScore.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile?.statistics.manga?.meanScore).GetValueOrDefault() / 10.0f;
                default:
                    return 0;
            }
        }

        public int GetMangaTotalEntries()
        {
            switch (ListPreference)
            {
                case AnimeList.MAL:
                    return malProfile.MangaStatistics.TotalEntries.GetValueOrDefault();
                case AnimeList.Anilist:
                    return anilistProfile.statistics.manga.count;
                default:
                    return 0;
            }
        }

        public int GetMangaChaptersRead()
        {
            switch (ListPreference)
            {
                case AnimeList.MAL:
                    return malProfile.MangaStatistics.ChaptersRead.GetValueOrDefault();
                case AnimeList.Anilist:
                    return anilistProfile.statistics.manga.chaptersRead;
                default:
                    return 0;
            }
        }

        public int GetMangaVolumesRead()
        {
            switch (ListPreference)
            {
                case AnimeList.MAL:
                    return malProfile.MangaStatistics.VolumesRead.GetValueOrDefault();
                case AnimeList.Anilist:
                    return anilistProfile.statistics.manga.volumesRead;
                default:
                    return 0;
            }
        }

        public int GetMangaReread()
        {
            switch (ListPreference)
            {
                case AnimeList.MAL:
                    return malProfile.MangaStatistics.Reread.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile.statistics.manga.statuses.Find(x => x.status == AniMediaListStatus.REPEATING)?.count).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetMangaReading()
        {
            switch (ListPreference)
            {
                case AnimeList.MAL:
                    return malProfile.MangaStatistics.Reading.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile.statistics.manga.statuses.Find(x => x.status == AniMediaListStatus.CURRENT)?.count).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetMangaCompleted()
        {
            switch (ListPreference)
            {
                case AnimeList.MAL:
                    return malProfile.MangaStatistics.Completed.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile.statistics.manga.statuses.Find(x => x.status == AniMediaListStatus.COMPLETED)?.count).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetMangaOnHold()
        {
            switch (ListPreference)
            {
                case AnimeList.MAL:
                    return malProfile.MangaStatistics.OnHold.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile.statistics.manga.statuses.Find(x => x.status == AniMediaListStatus.PAUSED)?.count).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetMangaDropped()
        {
            switch (ListPreference)
            {
                case AnimeList.MAL:
                    return malProfile.MangaStatistics.Dropped.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile.statistics.manga.statuses.Find(x => x.status == AniMediaListStatus.DROPPED)?.count).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetMangaPlanToRead()
        {
            switch (ListPreference)
            {
                case AnimeList.MAL:
                    return malProfile.MangaStatistics.PlanToRead.GetValueOrDefault();
                case AnimeList.Anilist:
                    return (anilistProfile.statistics.manga.statuses.Find(x => x.status == AniMediaListStatus.PLANNING)?.count).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        #endregion

        public async Task<bool> UpdateUserInfo()
        {
            switch (ListPreference)
            {
                case AnimeList.MAL:
                    if (malProfile == null) return false;
                    return await UpdateMALInfo(malProfile.Username);
                case AnimeList.Anilist:
                    if (anilistProfile == null) return false;
                    return await UpdateAnilistInfo(anilistProfile.name);
                default:
                    return false;
            }
        }

        public async Task<bool> UpdateMALInfo(string username)
        {
            try
            {
                MalUsername = username;
                malProfile = await Program._jikan.GetUserProfile(username);
                if (malProfile == null) return false;

                if (ListPreference == AnimeList.MAL)
                {
                    decimal? malMangaDays = malProfile.MangaStatistics?.DaysRead.GetValueOrDefault();
                    MangaDays = (double)decimal.Round(malMangaDays.GetValueOrDefault(), 1);

                    decimal mal_days = malProfile.AnimeStatistics.DaysWatched.GetValueOrDefault();
                    AnimeDays = (double)decimal.Round(mal_days, 1);
                }

                await DatabaseRequest.UpdateUser(this);
            } catch(Exception e)
            {
                await Program._logger.LogError(e);
            }
            return malProfile != null;
        }

        public async Task<bool> UpdateAnilistInfo(string username)
        {
            try
            {
                AnilistUsername = username;
                anilistProfile = await AniUserQuery.GetUser(username);
                if (anilistProfile == null) return false;

                if (ListPreference == AnimeList.Anilist)
                {
                    double chaptersRead = (double)decimal.Multiply((anilistProfile.statistics?.manga.chaptersRead).GetValueOrDefault(), (decimal)0.00556);
                    MangaDays = Math.Round(chaptersRead, 1);

                    int minutesWatched = anilistProfile.statistics.anime.minutesWatched;
                    AnimeDays = Math.Round(minutesWatched / 60.0 / 24.0, 1);
                }

                await DatabaseRequest.UpdateUser(this);
            } catch (Exception e) {
                await Program._logger.LogError(e);
            }

            return anilistProfile != null;
        }
    }
}
