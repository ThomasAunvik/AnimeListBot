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
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations.Schema;
using AnimeListBot.Modules;
using JikanDotNet.Exceptions;

namespace AnimeListBot.Handler
{
    public enum AnimeListPreference
    {
        MAL,
        Anilist,
        None
    }

    public class DiscordUser
    {
        public ulong UserId { get; set; }
        
        [NotMapped]
        public UserProfile malProfile;
        public string MalUsername { get; set; } = string.Empty;
        [NotMapped]
        public IAniUser anilistProfile;
        public string AnilistUsername { get; set; } = string.Empty;

        public AnimeListPreference ListPreference { get; set; }

        public double AnimeDays { get; set; }
        public double MangaDays { get; set; }

        public List<GuildUserInfo> Servers { get; set; } = new List<GuildUserInfo>();

        [NotMapped]
        public DateTime malCachedTime = DateTime.MinValue;
        [NotMapped]
        public DateTime anilistCachedTime = DateTime.MinValue;

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

        public void RefreshMutualGuilds()
        {
            List<SocketGuild> mutualGuilds = Program._client.Guilds.Where(g => g.GetUser(UserId) != null).ToList();
            for (int guildIndex = 0; guildIndex < mutualGuilds.Count; guildIndex++)
            {
                SocketGuild guild = mutualGuilds[guildIndex];
                SocketGuildUser guildUser = guild.GetUser(UserId);
                GuildUserInfo userInfo = Servers.Find(x => x.ServerId == guild.Id.ToString());

                if (userInfo == null) Servers.Add(new GuildUserInfo(guildUser));
                else userInfo.UpdateUserInfo(guildUser);
            }
        }

        public bool HasValidAnimelist()
        {
            switch (ListPreference)
            {
                case AnimeListPreference.MAL:
                    return malProfile != null;
                case AnimeListPreference.Anilist:
                    return anilistProfile != null;
                default:
                    return false;
            }
        }

        public bool HasValidMal()
        {
            return malProfile != null;
        }

        public bool HasValidAnilist()
        {
            return anilistProfile != null;
        }

        public string GetAnimelistUsername()
        {
            switch (ListPreference)
            {
                case AnimeListPreference.MAL:
                    return MalUsername;
                case AnimeListPreference.Anilist:
                    return AnilistUsername;
                default:
                    return "";
            }
        }

        public string GetAnimelistThumbnail()
        {
            switch (ListPreference)
            {
                case AnimeListPreference.MAL:
                    return malProfile.ImageURL;
                case AnimeListPreference.Anilist:
                    return anilistProfile.Avatar?.large;
                default:
                    return "";
            }
        }

        public string GetAnimelistLink()
        {
            switch (ListPreference)
            {
                case AnimeListPreference.MAL:
                    return malProfile?.URL;
                case AnimeListPreference.Anilist:
                    return anilistProfile?.siteUrl;
                default:
                    return "";
            }
        }

        #region AnimeStats

        public RoleRank GetAnimeServerRank(DiscordServer server)
        {
            if (server.ranks.AnimeRanks.Count <= 0) return null;

            var roles = server.ranks.AnimeRanks.OrderBy(x => x.Days).ToList();

            int roleIndex = roles.FindIndex(x => AnimeDays < x.Days);
            if (roleIndex == -1) roleIndex = roles.Count - 1;
            else roleIndex--;

            if (roleIndex < 0) return null;
            if (roles.Count <= roleIndex) return null;

            return roles[roleIndex];
        }

        public double GetAnimeWatchDays()
        {
            return AnimeDays;
        }

        public float GetAnimeMeanScore()
        {
            switch (ListPreference)
            {
                case AnimeListPreference.MAL:
                    return (float)malProfile.AnimeStatistics.MeanScore.GetValueOrDefault();
                case AnimeListPreference.Anilist:
                    return MathF.Round((anilistProfile?.statistics.anime?.meanScore).GetValueOrDefault(), 2);
                default:
                    return 0;
            }
        }

        public int GetAnimeTotalEntries()
        {
            switch (ListPreference)
            {
                case AnimeListPreference.MAL:
                    return malProfile.AnimeStatistics.TotalEntries.GetValueOrDefault();
                case AnimeListPreference.Anilist:
                    return anilistProfile.statistics.anime.count;
                default:
                    return 0;
            }
        }

        public int GetAnimeEpisodesWatched()
        {
            switch (ListPreference)
            {
                case AnimeListPreference.MAL:
                    return malProfile.AnimeStatistics.EpisodesWatched.GetValueOrDefault();
                case AnimeListPreference.Anilist:
                    return anilistProfile.statistics.anime.episodesWatched;
                default:
                    return 0;
            }
        }

        public int GetAnimeRewatched()
        {
            switch (ListPreference)
            {
                case AnimeListPreference.MAL:
                    return malProfile.AnimeStatistics.Rewatched.GetValueOrDefault();
                case AnimeListPreference.Anilist:
                    return (anilistProfile.statistics.anime.statuses.Find(x => x.status == AniMediaListStatus.REPEATING)?.count).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetAnimeWatching()
        {
            switch (ListPreference)
            {
                case AnimeListPreference.MAL:
                    return malProfile.AnimeStatistics.Watching.GetValueOrDefault();
                case AnimeListPreference.Anilist:
                    return (anilistProfile.statistics.anime.statuses.Find(x => x.status == AniMediaListStatus.CURRENT)?.count).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetAnimeCompleted()
        {
            switch (ListPreference)
            {
                case AnimeListPreference.MAL:
                    return malProfile.AnimeStatistics.Completed.GetValueOrDefault();
                case AnimeListPreference.Anilist:
                    return (anilistProfile.statistics.anime.statuses.Find(x => x.status == AniMediaListStatus.COMPLETED)?.count).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetAnimeOnHold()
        {
            switch (ListPreference)
            {
                case AnimeListPreference.MAL:
                    return malProfile.AnimeStatistics.OnHold.GetValueOrDefault();
                case AnimeListPreference.Anilist:
                    return (anilistProfile.statistics.anime.statuses.Find(x => x.status == AniMediaListStatus.PAUSED)?.count).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetAnimeDropped()
        {
            switch (ListPreference)
            {
                case AnimeListPreference.MAL:
                    return malProfile.AnimeStatistics.Dropped.GetValueOrDefault();
                case AnimeListPreference.Anilist:
                    return (anilistProfile.statistics.anime.statuses.Find(x => x.status == AniMediaListStatus.DROPPED)?.count).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetAnimePlanToWatch()
        {
            switch (ListPreference)
            {
                case AnimeListPreference.MAL:
                    return malProfile.AnimeStatistics.PlanToWatch.GetValueOrDefault();
                case AnimeListPreference.Anilist:
                    return (anilistProfile.statistics.anime.statuses.Find(x => x.status == AniMediaListStatus.PLANNING)?.count).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        #endregion

        #region MangaStats

        public RoleRank GetMangaServerRank(DiscordServer server)
        {
            if (server.ranks.MangaRanks.Count <= 0) return null;

            var roles = server.ranks.MangaRanks.OrderBy(x => x.Days).ToList();

            int roleIndex = roles.FindIndex(x => MangaDays < x.Days);
            if (roleIndex == -1) roleIndex = roles.Count - 1;
            else roleIndex--;

            if (roleIndex < 0) return null;
            if (roles.Count <= roleIndex) return null;

            return roles[roleIndex];
        }

        public double GetMangaReadDays()
        {
            return MangaDays;
        }

        public float GetMangaMeanScore()
        {
            switch (ListPreference)
            {
                case AnimeListPreference.MAL:
                    return (float)malProfile.MangaStatistics.MeanScore.GetValueOrDefault();
                case AnimeListPreference.Anilist:
                    return (anilistProfile?.statistics.manga?.meanScore).GetValueOrDefault() / 10.0f;
                default:
                    return 0;
            }
        }

        public int GetMangaTotalEntries()
        {
            switch (ListPreference)
            {
                case AnimeListPreference.MAL:
                    return malProfile.MangaStatistics.TotalEntries.GetValueOrDefault();
                case AnimeListPreference.Anilist:
                    return anilistProfile.statistics.manga.count;
                default:
                    return 0;
            }
        }

        public int GetMangaChaptersRead()
        {
            switch (ListPreference)
            {
                case AnimeListPreference.MAL:
                    return malProfile.MangaStatistics.ChaptersRead.GetValueOrDefault();
                case AnimeListPreference.Anilist:
                    return anilistProfile.statistics.manga.chaptersRead;
                default:
                    return 0;
            }
        }

        public int GetMangaVolumesRead()
        {
            switch (ListPreference)
            {
                case AnimeListPreference.MAL:
                    return malProfile.MangaStatistics.VolumesRead.GetValueOrDefault();
                case AnimeListPreference.Anilist:
                    return anilistProfile.statistics.manga.volumesRead;
                default:
                    return 0;
            }
        }

        public int GetMangaReread()
        {
            switch (ListPreference)
            {
                case AnimeListPreference.MAL:
                    return malProfile.MangaStatistics.Reread.GetValueOrDefault();
                case AnimeListPreference.Anilist:
                    return (anilistProfile.statistics.manga.statuses.Find(x => x.status == AniMediaListStatus.REPEATING)?.count).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetMangaReading()
        {
            switch (ListPreference)
            {
                case AnimeListPreference.MAL:
                    return malProfile.MangaStatistics.Reading.GetValueOrDefault();
                case AnimeListPreference.Anilist:
                    return (anilistProfile.statistics.manga.statuses.Find(x => x.status == AniMediaListStatus.CURRENT)?.count).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetMangaCompleted()
        {
            switch (ListPreference)
            {
                case AnimeListPreference.MAL:
                    return malProfile.MangaStatistics.Completed.GetValueOrDefault();
                case AnimeListPreference.Anilist:
                    return (anilistProfile.statistics.manga.statuses.Find(x => x.status == AniMediaListStatus.COMPLETED)?.count).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetMangaOnHold()
        {
            switch (ListPreference)
            {
                case AnimeListPreference.MAL:
                    return malProfile.MangaStatistics.OnHold.GetValueOrDefault();
                case AnimeListPreference.Anilist:
                    return (anilistProfile.statistics.manga.statuses.Find(x => x.status == AniMediaListStatus.PAUSED)?.count).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetMangaDropped()
        {
            switch (ListPreference)
            {
                case AnimeListPreference.MAL:
                    return malProfile.MangaStatistics.Dropped.GetValueOrDefault();
                case AnimeListPreference.Anilist:
                    return (anilistProfile.statistics.manga.statuses.Find(x => x.status == AniMediaListStatus.DROPPED)?.count).GetValueOrDefault();
                default:
                    return 0;
            }
        }

        public int GetMangaPlanToRead()
        {
            switch (ListPreference)
            {
                case AnimeListPreference.MAL:
                    return malProfile.MangaStatistics.PlanToRead.GetValueOrDefault();
                case AnimeListPreference.Anilist:
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
                case AnimeListPreference.MAL:
                    if (malProfile == null) return false;
                    return await UpdateMALInfo(malProfile.Username);
                case AnimeListPreference.Anilist:
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
                if (malProfile == null)
                {
                    MalUsername = string.Empty;
                    return false;
                }

                if (ListPreference == AnimeListPreference.MAL)
                {
                    decimal? malMangaDays = malProfile.MangaStatistics?.DaysRead.GetValueOrDefault();
                    MangaDays = (double)decimal.Round(malMangaDays.GetValueOrDefault(), 1);

                    decimal mal_days = malProfile.AnimeStatistics.DaysWatched.GetValueOrDefault();
                    AnimeDays = (double)decimal.Round(mal_days, 1);
                }
                return malProfile != null;
            }catch(JikanRequestException e)
            {
                switch (e.ResponseCode)
                {
                    case HttpStatusCode.NotFound:
                    case HttpStatusCode.BadRequest:
                        return false;
                }
                throw new Exception("MyAnimeList is having troubles, try again later. Join support server for status updates: https://discord.gg/Q9cf46R \nError ResponseCode: " + e.ResponseCode + "");
            }
        }

        public async Task<bool> UpdateAnilistInfo(string username)
        {
            try
            {
                AnilistUsername = username;
                anilistProfile = await AniUserQuery.GetUser(username);
                if (anilistProfile == null)
                {
                    AnilistUsername = string.Empty;
                    return false;
                }

                if (ListPreference == AnimeListPreference.Anilist)
                {
                    double chaptersRead = (double)decimal.Multiply((anilistProfile.statistics?.manga.chaptersRead).GetValueOrDefault(), (decimal)0.00556);
                    MangaDays = Math.Round(chaptersRead, 1);

                    int minutesWatched = anilistProfile.statistics.anime.minutesWatched;
                    AnimeDays = Math.Round(minutesWatched / 60.0 / 24.0, 1);
                }
            } catch (Exception e) {
                await Program._logger.LogError(e);
            }

            return anilistProfile != null;
        }
    }
}
