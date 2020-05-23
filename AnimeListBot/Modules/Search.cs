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
using AnimeListBot.Handler.Database;
using System.Xml.Schema;
using JikanDotNet.Exceptions;

namespace AnimeListBot.Modules
{
    public class Search : ModuleBase<ShardedCommandContext>
    {
        private IDatabaseService _db;

        public Search(IDatabaseService db)
        {
            _db = db;
        }

        [Command("anime")]
        [RequireBotPermission(GuildPermission.AddReactions)]
        public async Task SearchAnime(IUser targetUser, [Remainder]string search)
        {
            EmbedHandler embed = new EmbedHandler(targetUser, "Searching for " + search + "...");
            await embed.SendMessage(Context.Channel);
            embed.SetOwner(Context.User);

            DiscordUser user = await _db.GetUserById(targetUser.Id);
            await SearchAnime(embed, user, search);
        }
        
        [Command("anime")]
        [RequireBotPermission(GuildPermission.AddReactions)]
        public async Task SearchAnime([Remainder]string search)
        {
            await SearchAnime(Context.User, search);
        }

        [Command("midanime")]
        public static async Task GetAnime(EmbedHandler embed, DiscordUser targetUser, long id, AnimeListPreference listOverride = AnimeListPreference.None)
        {
            if(id == 0)
            {
                embed.Title = "No Anime Found";
                await embed.UpdateEmbed();
                return;
            }
            embed.Title = "No Anime Found";

            AnimeListPreference preference = targetUser.ListPreference;
            if (listOverride != AnimeListPreference.None) preference = listOverride;

            switch (preference)
            {
                case AnimeListPreference.MAL:
                    Anime malAnime = null;
                    try
                    {
                        malAnime = await Program._jikan.GetAnime(id);
                    }
                    catch (JikanRequestException) { }

                    if (malAnime != null)
                    {
                        AnimeSearchEntry entry = MalClassTransfer.AnimeToSearchEntry(malAnime);
                        await SetAnimeMalInfo(entry, embed, targetUser);
                    }
                    break;

                case AnimeListPreference.Anilist:
                    IAniMedia media = await AniMediaQuery.GetMedia((int)id, AniMediaType.ANIME);
                    if (media != null)
                    {
                        await SetAnimeAniInfo(media, embed, targetUser);
                    }
                    break;
            }
            await embed.UpdateEmbed();
        }

        public static async Task SearchAnime(EmbedHandler embed, DiscordUser targetUser, string search)
        {
            embed.Title = "Searching for " + search + "...";
            await embed.UpdateEmbed();

            AnimeSearchResult searchResult = null;
            try
            {
                searchResult = await Program._jikan.SearchAnime(search);
            }
            catch (JikanRequestException) { }
            IAniMedia media = await AniMediaQuery.SearchMedia(search, AniMediaType.ANIME);

            embed.Title = "No Anime Found";

            bool mal = targetUser.ListPreference == AnimeListPreference.MAL;
            if (mal && (searchResult != null && searchResult.Results.Count > 0))
            {
                List<AnimeSearchEntry> results = searchResult.Results.ToList();
                if(results.Count > 1)
                {
                    await SetMalAnimeSearchList(embed, targetUser, results, 1);
                    return;
                }

                AnimeSearchEntry entry = results[0];
                await SetAnimeMalInfo(entry, embed, targetUser);
            }
            else if (mal && media != null)
            {
                mal = false;
            }

            if (!mal && media != null)
            {
                await SetAnimeAniInfo(media, embed, targetUser);
            }
            else if (!mal && (searchResult != null && searchResult.Results.Count > 0))
            {
                List<AnimeSearchEntry> results = searchResult.Results.ToList();
                if (results.Count > 1)
                {
                    await SetMalAnimeSearchList(embed, targetUser, results, 1);
                    return;
                }

                AnimeSearchEntry entry = results[0];
                await SetAnimeMalInfo(entry, embed, targetUser);
            }
            await embed.UpdateEmbed();
        }

        public static async Task SetMalAnimeSearchList(EmbedHandler embed, DiscordUser targetUser, List<AnimeSearchEntry> searchList, int page)
        {
            bool nextPage = true;
            int maxIndex = 5 * page;
            int startPage = 5 * (page - 1);
            bool previousPage = startPage > 0;
            if (searchList.Count <= maxIndex)
            {
                maxIndex = searchList.Count;
                nextPage = false;
            }

            if (previousPage)
            {
                int currentPage = page;
                Emoji emote = new Emoji(Emotes.PREVIOUS_PAGE);
                embed.AddEmojiAction(emote, async () =>
                {
                    embed.Fields.Clear();
                    await embed.RemoveAllEmojiActions();
                    await SetMalAnimeSearchList(embed, targetUser, searchList, currentPage - 1);
                });
            }

            string favListMessage = string.Empty;
            for (int resultIndex = startPage; resultIndex < maxIndex; resultIndex++)
            {
                AnimeSearchEntry indexEntry = searchList[resultIndex];
                Emoji emote = new Emoji(Emotes.NUMBERS_EMOTES[resultIndex - startPage + 1]);
                favListMessage += emote + " " + indexEntry.Title + "\n";
                embed.AddEmojiAction(emote, async () =>
                {
                    embed.Title = "Searching for " + indexEntry.Title + "...";
                    embed.Fields.Clear();
                    await embed.RemoveAllEmojiActions();
                    await embed.UpdateEmbed();
                    await SetAnimeMalInfo(indexEntry, embed, targetUser);
                    await embed.UpdateEmbed();
                });
            }

            if (nextPage)
            {
                int currentPage = page;
                Emoji emote = new Emoji(Emotes.NEXT_PAGE);
                embed.AddEmojiAction(emote, async () =>
                {
                    embed.Fields.Clear();
                    await embed.RemoveAllEmojiActions();
                    await SetMalAnimeSearchList(embed, targetUser, searchList, currentPage + 1);
                });
            }
            embed.Title = "Select Anime";
            embed.AddField("Page " + page, favListMessage);
            await embed.UpdateEmbed();
        }

        [Command("manga")]
        [RequireBotPermission(GuildPermission.AddReactions)]
        public async Task SearchManga(IUser targetUser, [Remainder]string search)
        {
            EmbedHandler embed = new EmbedHandler(targetUser, "Searching for " + search + "...");
            await embed.SendMessage(Context.Channel);
            embed.SetOwner(Context.User);

            DiscordUser user = await _db.GetUserById(targetUser.Id);
            await SearchManga(embed, user, search);
        }

        [Command("manga")]
        [RequireBotPermission(GuildPermission.AddReactions)]
        public async Task SearchManga([Remainder]string search)
        {
            await SearchManga(Context.User, search);
        }

        public static async Task SearchManga(EmbedHandler embed, DiscordUser targetUser, [Remainder]string search)
        {
            MangaSearchResult searchResult = null;
            try
            {
                searchResult = await Program._jikan.SearchManga(search);
            }
            catch (JikanRequestException) { }
            IAniMedia media = await AniMediaQuery.SearchMedia(search, AniMediaType.MANGA);

            embed.Title = "No Manga Found";

            bool mal = targetUser.ListPreference == AnimeListPreference.MAL;
            if (mal && (searchResult != null && searchResult.Results.Count > 0))
            {
                List<MangaSearchEntry> results = searchResult.Results.ToList();
                if (results.Count > 1)
                {
                    await SetMalMangaSearchList(embed, targetUser, results, 1);
                    return;
                }

                MangaSearchEntry entry = results[0];
                await SetMangaMalInfo(entry, embed, targetUser);
            }
            else if (mal && media != null)
            {
                mal = false;
            }

            if (!mal && media != null)
            {
                await SetMangaAniInfo(media, embed, targetUser);
            }
            else if (!mal && (searchResult != null && searchResult.Results.Count > 0))
            {
                List<MangaSearchEntry> results = searchResult.Results.ToList();
                if (results.Count > 1)
                {
                    await SetMalMangaSearchList(embed, targetUser, results, 1);
                    return;
                }
                MangaSearchEntry entry = results[0];
                await SetMangaMalInfo(entry, embed, targetUser);
            }

            await embed.UpdateEmbed();
        }

        [Command("midmanga")]
        public static async Task GetManga(EmbedHandler embed, DiscordUser targetUser, long id, AnimeListPreference listOverride = AnimeListPreference.None)
        {
            if (id == 0)
            {
                embed.Title = "No Manga Found";
                await embed.UpdateEmbed();
                return;
            }

            embed.Title = "No Manga Found";

            AnimeListPreference preference = targetUser.ListPreference;
            if (listOverride != AnimeListPreference.None) preference = listOverride;

            switch (preference)
            {
                case AnimeListPreference.MAL:
                    Manga malManga = null;
                    try
                    {
                        malManga = await Program._jikan.GetManga(id);
                    }
                    catch (JikanRequestException) { }

                    if (malManga != null)
                    {
                        MangaSearchEntry entry = MalClassTransfer.MangaToSearchEntry(malManga);
                        await SetMangaMalInfo(entry, embed, targetUser);
                    }
                    break;

                case AnimeListPreference.Anilist:
                    IAniMedia media = await AniMediaQuery.GetMedia((int)id, AniMediaType.MANGA);
                    if (media != null)
                    {
                        await SetMangaAniInfo(media, embed, targetUser);
                    }
                    break;
            }
            await embed.UpdateEmbed();
        }

        [Command("character")]
        public async Task SearchCharacter([Remainder]string search)
        {
            EmbedHandler embed = new EmbedHandler(Context.User, "Searching for " + search + "...");
            await embed.SendMessage(Context.Channel);

            embed.Title = "No Character Found";

            DiscordUser globalUser = await _db.GetUserById(Context.User.Id);

            CharacterSearchResult result = null;
            try
            {
                result = await Program._jikan.SearchCharacter(search);
            }
            catch (JikanRequestException) { }
            IAniCharacter character = await AniCharacterQuery.SearchCharacter(search);

            bool mal = globalUser.ListPreference == AnimeListPreference.MAL;
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

            DiscordUser globalUser = await _db.GetUserById(Context.User.Id);

            PersonSearchResult result = null;
            try
            {
                result = await Program._jikan.SearchPerson(search);
            }
            catch (JikanRequestException) { }
            IAniStaff staff = await AniStaffQuery.SearchStaff(search);
            bool mal = globalUser.ListPreference == AnimeListPreference.MAL;
            if (mal && result != null && result.Results.Count > 0)
            {
                List<PersonSearchEntry> results = result.Results.ToList();
                PersonSearchEntry entry = results[0];
                Person malPerson = await Program._jikan.GetPerson(entry.MalId);
                embed.Title = malPerson.Name;
                embed.Url = malPerson.LinkCanonical;
                embed.ThumbnailUrl = malPerson.ImageURL;
                embed.Description = EmbedHandler.SecureEmbedText(malPerson.More);
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

        public static async Task SetAnimeMalInfo(AnimeSearchEntry entry, EmbedHandler embed, DiscordUser globalUser)
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

            IUser user = globalUser.GetUser();
            if (globalUser.malProfile != null)
            {
                if (!string.IsNullOrWhiteSpace(globalUser.malProfile.Username))
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
                            Format.Sanitize(user.Username) + " Stats",
                            "There are no stats available for this user."
                    );
                }
            }
            else
            {
                embed.AddFieldSecure(
                        Format.Sanitize(user.Username) + " Stats",
                        "There are no stats available for this user."
                );
            }
        }

        public static async Task SetAnimeAniInfo(IAniMedia media, EmbedHandler embed, DiscordUser globalUser)
        {
            if (media == null) return;
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

            IUser user = globalUser.GetUser();
            if (globalUser.anilistProfile != null)
            {
                if (!string.IsNullOrWhiteSpace(globalUser.anilistProfile?.name))
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
                            Format.Sanitize(user.Username) + " Stats",
                            "There are no stats available for this user."
                    );
                }
            }
            else
            {
                embed.AddFieldSecure(
                        Format.Sanitize(user.Username) + " Stats",
                        "There are no stats available for this user."
                );
            }
        }

        public static async Task SetMangaMalInfo(MangaSearchEntry entry, EmbedHandler embed, DiscordUser globalUser)
        {
            if (entry == null) return;
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

            IUser user = globalUser.GetUser();
            if (globalUser.malProfile != null)
            {
                if (!string.IsNullOrWhiteSpace(globalUser.malProfile.Username))
                {
                    await globalUser.UpdateUserInfo();
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
                            Format.Sanitize(user.Username) + " Stats",
                            "There are no stats available for this user."
                    );
                }
            }
            else
            {
                embed.AddFieldSecure(
                        Format.Sanitize(user.Username) + " Stats",
                        "There are no stats available for this user."
                );
            }
        }

        public static async Task SetMangaAniInfo(IAniMedia media, EmbedHandler embed, DiscordUser globalUser)
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

            IUser user = globalUser.GetUser();
            if (globalUser.anilistProfile != null)
            {
                if (!string.IsNullOrWhiteSpace(globalUser.anilistProfile?.name))
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
                            Format.Sanitize(user.Username) + " Stats",
                            "There are no stats available for this user."
                    );
                }
            }
            else
            {
                embed.AddFieldSecure(
                        Format.Sanitize(user.Username) + " Stats",
                        "There are no stats available for this user."
                );
            }
        }

        public static async Task SetMalMangaSearchList(EmbedHandler embed, DiscordUser targetUser, List<MangaSearchEntry> searchList, int page)
        {
            bool nextPage = true;
            int maxIndex = 5 * page;
            int startPage = 5 * (page - 1);
            bool previousPage = startPage > 0;
            if (searchList.Count <= maxIndex)
            {
                maxIndex = searchList.Count;
                nextPage = false;
            }

            if (previousPage)
            {
                int currentPage = page;
                Emoji emote = new Emoji(Emotes.PREVIOUS_PAGE);
                embed.AddEmojiAction(emote, async () =>
                {
                    embed.Fields.Clear();
                    await embed.RemoveAllEmojiActions();
                    await SetMalMangaSearchList(embed, targetUser, searchList, currentPage - 1);
                });
            }

            string favListMessage = string.Empty;
            for (int resultIndex = startPage; resultIndex < maxIndex; resultIndex++)
            {
                MangaSearchEntry indexEntry = searchList[resultIndex];
                Emoji emote = new Emoji(Emotes.NUMBERS_EMOTES[resultIndex - startPage + 1]);
                favListMessage += emote + " (" + indexEntry.Type + ") " + indexEntry.Title + "\n";
                embed.AddEmojiAction(emote, async () =>
                {
                    embed.Title = "Searching for " + indexEntry.Title + "...";
                    embed.Fields.Clear();
                    await embed.RemoveAllEmojiActions();
                    await embed.UpdateEmbed();
                    await SetMangaMalInfo(indexEntry, embed, targetUser);
                    await embed.UpdateEmbed();
                });
            }

            if (nextPage)
            {
                int currentPage = page;
                Emoji emote = new Emoji(Emotes.NEXT_PAGE);
                embed.AddEmojiAction(emote, async () =>
                {
                    embed.Fields.Clear();
                    await embed.RemoveAllEmojiActions();
                    await SetMalMangaSearchList(embed, targetUser, searchList, currentPage + 1);
                });
            }
            embed.Title = "Select Manga";
            embed.AddField("Page " + page, favListMessage);
            await embed.UpdateEmbed();
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
