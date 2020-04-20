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
using Discord.Commands;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AnimeListBot.Handler;
using JikanDotNet;
using System.Globalization;
using Discord;
using AnimeListBot.Handler.Anilist;
using AnimeListBot.Handler.Misc;

namespace AnimeListBot.Modules
{
    public class Search : ModuleBase<ShardedCommandContext>
    {
        [Command("anime")]
        public async Task SearchAnime(IUser targetUser, [Remainder]string search)
        {
            EmbedHandler embed = new EmbedHandler(targetUser, "Searching for " + search + "...");
            await embed.SendMessage(Context.Channel);

            await SearchAnime(embed, targetUser, search);
        }
        
        [Command("anime")]
        public async Task SearchAnime([Remainder]string search)
        {
            await SearchAnime(Context.User, search);
        }

        [Command("midanime")]
        public static async Task GetAnime(EmbedHandler embed, IUser targetUser, long id)
        {
            DiscordUser globalUser = await DatabaseRequest.GetUserById(targetUser.Id);

            Anime malAnime = await Program._jikan.GetAnime(id);

            embed.Title = "No Anime Found";

            if (malAnime != null)
            {
                AnimeSearchEntry entry = MalClassTransfer.AnimeToSearchEntry(malAnime);
                await SetAnimeMalInfo(entry, embed, globalUser, targetUser);
            }

            await embed.UpdateEmbed();
        }

        public static async Task SearchAnime(EmbedHandler embed, IUser targetUser, string search)
        {
            embed.Title = "Searching for " + search + "...";
            await embed.UpdateEmbed();

            DiscordUser globalUser = await DatabaseRequest.GetUserById(targetUser.Id);

            AnimeSearchResult searchResult = await Program._jikan.SearchAnime(search);
            IAniMedia media = await AniMediaQuery.SearchMedia(search, AniMediaType.ANIME);

            embed.Title = "No Anime Found";

            bool mal = globalUser.ListPreference == DiscordUser.AnimeList.MAL;
            if (mal && (searchResult != null && searchResult.Results.Count > 0))
            {
                List<AnimeSearchEntry> results = searchResult.Results.ToList();
                AnimeSearchEntry entry = results[0];
                await SetAnimeMalInfo(entry, embed, globalUser, targetUser);
            }
            else if (mal && media != null)
            {
                mal = false;
            }

            if (!mal && media != null)
            {
                await SetAnimeAniInfo(media, embed, globalUser, targetUser);
            }
            else if (!mal && (searchResult != null && searchResult.Results.Count > 0))
            {
                List<AnimeSearchEntry> results = searchResult.Results.ToList();
                AnimeSearchEntry entry = results[0];
                await SetAnimeMalInfo(entry, embed, globalUser, targetUser);
            }
            await embed.UpdateEmbed();
        }

        [Command("manga")]
        public static async Task SearchManga(EmbedHandler embed, IUser targetUser, [Remainder]string search)
        {
            DiscordUser globalUser = await DatabaseRequest.GetUserById(targetUser.Id);

            MangaSearchResult searchResult = await Program._jikan.SearchManga(search);
            IAniMedia media = await AniMediaQuery.SearchMedia(search, AniMediaType.MANGA);

            embed.Title = "No Manga Found";

            bool mal = globalUser.ListPreference == DiscordUser.AnimeList.MAL;
            if (mal && (searchResult != null && searchResult.Results.Count > 0))
            {
                List<MangaSearchEntry> results = searchResult.Results.ToList();
                MangaSearchEntry entry = results[0];
                await SetMangaMalInfo(entry, embed, globalUser, targetUser);
            }
            else if (mal && media != null)
            {
                mal = false;
            }

            if (!mal && media != null)
            {
                await SetMangaAniInfo(media, embed, globalUser, targetUser);
            }
            else if (!mal && (searchResult != null && searchResult.Results.Count > 0))
            {
                List<MangaSearchEntry> results = searchResult.Results.ToList();
                MangaSearchEntry entry = results[0];
                await SetMangaMalInfo(entry, embed, globalUser, targetUser);
            }

            await embed.UpdateEmbed();
        }

        [Command("midmanga")]
        public static async Task GetManga(EmbedHandler embed, IUser targetUser, long id)
        {
            DiscordUser globalUser = await DatabaseRequest.GetUserById(targetUser.Id);

            Manga malManga = await Program._jikan.GetManga(id);
            
            embed.Title = "No Manga Found";

            if (malManga != null)
            {
                MangaSearchEntry entry = MalClassTransfer.MangaToSearchEntry(malManga);
                await SetMangaMalInfo(entry, embed, globalUser, targetUser);
            }
            await embed.UpdateEmbed();
        }

        [Command("manga")]
        public async Task SearchManga(IUser targetUser, [Remainder]string search)
        {
            EmbedHandler embed = new EmbedHandler(targetUser, "Searching for " + search + "...");
            await embed.SendMessage(Context.Channel);

            await SearchManga(embed, targetUser, search);
        }

        [Command("manga")]
        public async Task SearchManga([Remainder]string search)
        {
            await SearchManga(Context.User, search);
        }

        [Command("character")]
        public async Task SearchCharacter([Remainder]string search)
        {
            EmbedHandler embed = new EmbedHandler(Context.User, "Searching for " + search + "...");
            await embed.SendMessage(Context.Channel);

            embed.Title = "No Character Found";

            DiscordUser globalUser = await DatabaseRequest.GetUserById(Context.User.Id);

            CharacterSearchResult result = await Program._jikan.SearchCharacter(search);
            IAniCharacter character = await AniCharacterQuery.SearchCharacter(search);

            bool mal = globalUser.ListPreference == DiscordUser.AnimeList.MAL;
            if (mal && result != null && result.Results.Count > 0)
            {
                List<CharacterSearchEntry> results = result.Results.ToList();
                CharacterSearchEntry entry = results[0];
                Character malCharacter = await Program._jikan.GetCharacter(entry.MalId);
                
                embed.Title = malCharacter.Name;
                embed.Url = malCharacter.LinkCanonical;
                embed.ThumbnailUrl = malCharacter.ImageURL;
                embed.Description = EmbedHandler.SecureEmbedText(malCharacter.About);
                
                if(malCharacter.Nicknames.Count > 0) embed.AddFieldSecure("Nicknames", string.Join("\n", malCharacter.Nicknames));
                if(malCharacter.VoiceActors.Count > 0) embed.AddFieldSecure("Voice Actors", string.Join("\n", malCharacter.VoiceActors));
            }else if(!mal && character != null)
            {
                embed.Title = (string.IsNullOrEmpty(character.name.last) ? "" : character.name.last + ", ") + character.name.first;
                embed.Url = character.siteUrl;
                embed.ThumbnailUrl = character.image.large;
                embed.Description = EmbedHandler.SecureEmbedText(character.description);
                if (character.name.alternative.Count > 0 && !string.IsNullOrEmpty(character.name.alternative[0]))
                {
                    embed.AddFieldSecure("Alternative Names", string.Join("\n", character.name.alternative));
                }
            }

            await embed.UpdateEmbed();
        }

        [Command("staff")]
        public async Task SearchStaff([Remainder]string search)
        {
            EmbedHandler embed = new EmbedHandler(Context.User, "Searching for " + search + "...");
            await embed.SendMessage(Context.Channel);

            embed.Title = "No Staff Found";

            DiscordUser globalUser = await DatabaseRequest.GetUserById(Context.User.Id);

            PersonSearchResult result = await Program._jikan.SearchPerson(search);
            IAniStaff staff = await AniStaffQuery.SearchStaff(search);
            bool mal = globalUser.ListPreference == DiscordUser.AnimeList.MAL;
            if (mal && result != null && result.Results.Count > 0)
            {
                List<PersonSearchEntry> results = result.Results.ToList();
                PersonSearchEntry entry = results[0];
                Person malPerson = await Program._jikan.GetPerson(entry.MalId);
                embed.Title = malPerson.Name;
                embed.Url = malPerson.LinkCanonical;
                embed.ThumbnailUrl = malPerson.ImageURL;
                embed.Description = EmbedHandler.SecureEmbedText(malPerson.More.Replace(@"\n", ""));
                if (malPerson.Birthday.HasValue) {
                    CultureInfo en_US = new CultureInfo("en-US");
                    embed.AddFieldSecure("Birthday", malPerson.Birthday.Value.ToString("dddd, dd MMMM yyyy", en_US));
                }
            }else if(!mal && staff != null)
            {
                embed.Title = (string.IsNullOrEmpty(staff.name.last) ? "" : staff.name.last + ", ") + staff.name.first;
                embed.Url = staff.siteUrl;
                embed.ThumbnailUrl = staff.image.large;
                embed.Description = EmbedHandler.SecureEmbedText(staff.description);
            }
            await embed.UpdateEmbed();
        }

        private static async Task SetAnimeMalInfo(AnimeSearchEntry entry, EmbedHandler embed, DiscordUser globalUser, IUser targetUser)
        {
            embed.Title = entry.Title;
            embed.Description = EmbedHandler.SecureEmbedText(entry.Description);
            embed.Url = entry.URL;
            embed.ThumbnailUrl = entry.ImageURL;
            embed.AddFieldSecure(
                "Information",
                    "Type: " + entry.Type +
                    "\nEpisodes: " + (entry.Episodes == 0 ? "Unknown" : entry.Episodes.ToString()) +
                    "\nStatus: " + GetAnimeStatus(entry) +
                    "\nAired: " + GetDate(entry.StartDate.GetValueOrDefault(), entry.EndDate.GetValueOrDefault())
            );

            if (globalUser != null && !string.IsNullOrWhiteSpace(globalUser.malProfile.Username))
            {
                await globalUser.UpdateUserInfo();
                UserListAnimeSearchConfig config = new UserListAnimeSearchConfig();
                config.Query = entry.Title;
                UserAnimeList animeList = await Program._jikan.GetUserAnimeList(globalUser.malProfile.Username, config);
                AnimeListEntry malEntry = animeList?.Anime?.ToList().Find(x => x.MalId == entry.MalId);
                if (malEntry != null)
                {
                    embed.AddFieldSecure(
                        Format.Sanitize(globalUser.malProfile.Username) + " Stats",
                        (malEntry.Days.GetValueOrDefault() == 0 ? "" : "\nDays watched: " + malEntry.Days.GetValueOrDefault()) +
                        "\nEpisodes watched: " + malEntry.WatchedEpisodes +
                        "\nRating: " + malEntry.Score
                    );
                }
                else
                {
                    embed.AddFieldSecure(Format.Sanitize(globalUser.malProfile.Username) + " Stats", "Has not watched this anime.");
                }
            }
            else
            {
                embed.AddFieldSecure(
                        Format.Sanitize(targetUser.Username) + " Stats",
                        "There are no stats available for this user."
                );
            }
        }

        private static async Task SetAnimeAniInfo(IAniMedia media, EmbedHandler embed, DiscordUser globalUser, IUser targetUser)
        {
            embed.Title = media.title.english;
            embed.Description = EmbedHandler.SecureEmbedText(media.description);
            embed.Url = media.siteUrl;
            embed.ThumbnailUrl = media.coverImage.large;
            embed.AddFieldSecure(
                "Information",
                    "Type: " + Enum.GetName(typeof(AniMediaType), media.type) +
                    "\nFormat: " + Enum.GetName(typeof(AniMediaFormat), media.format).Replace("_", " ") +
                    "\nEpisodes: " + (media.episodes.GetValueOrDefault() == 0 ? "Unknown" : media.episodes.GetValueOrDefault().ToString()) +
                    "\nStatus: " + Enum.GetName(typeof(AniMediaStatus), media.status).Replace("_", " ") +
                    "\nAired: " + GetDate(media.startDate, media.endDate)
            );

            if (globalUser != null && !string.IsNullOrWhiteSpace(globalUser.anilistProfile?.name))
            {
                IAniMediaList list = await AniMediaListQuery.GetMediaList(globalUser.anilistProfile.name, media.id.GetValueOrDefault(), AniMediaType.ANIME);
                if (list != null)
                {
                    embed.AddFieldSecure(
                        Format.Sanitize(globalUser.anilistProfile.name) + " Stats",
                        //(malEntry.Days.GetValueOrDefault() == 0 ? "" : "\nDays watched: " + list.day) +
                        "\nStatus: " + Enum.GetName(typeof(AniMediaListStatus), list.status.GetValueOrDefault()) +
                        "\nEpisodes watched: " + list.progress.GetValueOrDefault() +
                        "\nRating: " + list.score
                    );
                }
                else
                {
                    embed.AddFieldSecure(Format.Sanitize(globalUser.anilistProfile.name) + " Stats", "Has not watched this anime.");
                }
            }
            else
            {
                embed.AddFieldSecure(
                        Format.Sanitize(targetUser.Username) + " Stats",
                        "There are no stats available for this user."
                );
            }
        }

        private static async Task SetMangaMalInfo(MangaSearchEntry entry, EmbedHandler embed, DiscordUser globalUser, IUser targetUser)
        {
            embed.Title = entry.Title;
            embed.Description = EmbedHandler.SecureEmbedText(entry.Description);
            embed.Url = entry.URL;
            embed.ThumbnailUrl = entry.ImageURL;
            embed.AddFieldSecure(
                "Information",
                    "Type: " + entry.Type +
                    "\nVolumes: " + (entry.Volumes == 0 ? "Unknown" : entry.Volumes.ToString()) +
                    "\nChapters: " + (entry.Chapters == 0 ? "Unknown" : entry.Chapters.ToString()) +
                    "\nStatus: " + GetMangaStatus(entry) +
                    "\nPublished: " + GetDate(entry.StartDate.GetValueOrDefault(), entry.EndDate.GetValueOrDefault())
            );

            await globalUser.UpdateUserInfo();
            if (globalUser != null && !string.IsNullOrWhiteSpace(globalUser.malProfile.Username))
            {
                UserListMangaSearchConfig config = new UserListMangaSearchConfig();
                config.Query = entry.Title;
                UserMangaList mangaList = await Program._jikan.GetUserMangaList(globalUser.malProfile.Username, config);
                MangaListEntry malEntry = mangaList?.Manga?.ToList().Find(x => x.MalId == entry.MalId);
                if (malEntry != null)
                {
                    embed.AddFieldSecure(
                        Format.Sanitize(globalUser.malProfile.Username) + " Stats",
                        (malEntry.Days.GetValueOrDefault() == 0 ? "" : "\nDays read: " + malEntry.Days.GetValueOrDefault()) +
                        "\nVolumes read: " + malEntry.ReadVolumes +
                        "\nChapters read: " + malEntry.ReadChapters +
                        "\nRating: " + malEntry.Score
                    );
                }
                else
                {
                    embed.AddFieldSecure(Format.Sanitize(globalUser.malProfile.Username) + " Stats", "Has not read this manga.");
                }
            }
            else
            {
                embed.AddFieldSecure(
                        Format.Sanitize(targetUser.Username) + " Stats",
                        "There are no stats available for this user."
                );
            }
        }

        private static async Task SetMangaAniInfo(IAniMedia media, EmbedHandler embed, DiscordUser globalUser, IUser targetUser)
        {
            embed.Title = media.title.english;
            embed.Description = EmbedHandler.SecureEmbedText(media.description);
            embed.Url = media.siteUrl;
            embed.ThumbnailUrl = media.coverImage.large;
            embed.AddFieldSecure(
                "Information",
                    "Type: " + Enum.GetName(typeof(AniMediaType), media.type) +
                    "\nFormat: " + Enum.GetName(typeof(AniMediaFormat), media.format) +
                    "\nVolumes: " + (media.volumes.GetValueOrDefault() == 0 ? "Unknown" : media.volumes.GetValueOrDefault().ToString()) +
                    "\nChapters: " + (media.chapters.GetValueOrDefault() == 0 ? "Unknown" : media.chapters.GetValueOrDefault().ToString()) +
                    "\nStatus: " + Enum.GetName(typeof(AniMediaStatus), media.status).Replace("_", " ") +
                    "\nAired: " + GetDate(media.startDate, media.endDate)
            );

            if (globalUser != null && !string.IsNullOrWhiteSpace(globalUser.anilistProfile?.name))
            {
                IAniMediaList list = await AniMediaListQuery.GetMediaList(globalUser.anilistProfile.name, media.id.GetValueOrDefault(), AniMediaType.MANGA);
                if (list != null)
                {
                    embed.AddFieldSecure(
                        Format.Sanitize(globalUser.anilistProfile.name) + " Stats",
                        //(malEntry.Days.GetValueOrDefault() == 0 ? "" : "\nDays watched: " + list.day) +
                        "\nStatus: " + Enum.GetName(typeof(AniMediaListStatus), list.status.GetValueOrDefault()) +
                        "\nVolumes Read: " + list.progressVolumes +
                        "\nChapters Read: " + list.progress +
                        "\nRating: " + list.score
                    );
                }
                else
                {
                    embed.AddFieldSecure(Format.Sanitize(globalUser.anilistProfile.name) + " Stats", "Has not read this manga.");
                }
            }
            else
            {
                embed.AddFieldSecure(
                        Format.Sanitize(targetUser.Username) + " Stats",
                        "There are no stats available for this user."
                );
            }
        }

        public static string GetAnimeStatus(AnimeSearchEntry entry)
        {
            return entry.Airing ? "Airing" : (entry.StartDate > DateTime.Now ? "Not yet aired" : "Finished Airing");
        }

        public static string GetMangaStatus(MangaSearchEntry entry)
        {
            return entry.Publishing ? "Publishing" :  "Finished";
        }

        public static string GetDate(DateTime startDate, DateTime endDate)
        {
            CultureInfo enCultureInfo = CultureInfo.CreateSpecificCulture("en-US");

            string returnMessage = string.Empty;
            StringBuilder startDateString = new StringBuilder(startDate.ToString("MMM" + (startDate.Day == 0 ? "" : " dd") + ", yyyy", enCultureInfo));
            char firstChar = startDateString[0];
            startDateString[0] = char.ToUpper(firstChar);

            returnMessage += startDateString;

            bool isEndDateDefault = endDate == default(DateTime);
            if (!isEndDateDefault && startDate != endDate)
            {
                StringBuilder endDateString = new StringBuilder(endDate.ToString($"MMM" + (endDate.Day == 0 ? "" : " dd") + ", yyyy", enCultureInfo));
                firstChar = endDateString[0];
                endDateString[0] = char.ToUpper(firstChar);
                returnMessage += " to " + endDateString.ToString();
            }
            else if(startDate != endDate)
            {
                returnMessage += " to ?";
            }
            return returnMessage;
        }

        public static string GetDate(AniFuzzyDate startDate, AniFuzzyDate endDate)
        {
            DateTime startDateTime = default(DateTime);
            DateTime endDateTime = default(DateTime);

            try
            {
                startDateTime = new DateTime(startDate.year.GetValueOrDefault(), startDate.month.GetValueOrDefault(), startDate.day.GetValueOrDefault());
                endDateTime = new DateTime(endDate.year.GetValueOrDefault(), endDate.month.GetValueOrDefault(), endDate.day.GetValueOrDefault());
            }
            catch (Exception)
            {
                // TODO: Exception
            }

            return GetDate(startDateTime, endDateTime);
        }
    }
}
