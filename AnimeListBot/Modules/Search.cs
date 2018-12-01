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

namespace AnimeListBot.Modules
{
    public class Search : ModuleBase<ICommandContext>
    {
        [Command("anime")]
        public async Task SearchAnime(IUser targetUser, [Remainder]string search)
        {
            EmbedHandler embed = new EmbedHandler(targetUser, "Searching for " + search + "...");
            await embed.SendMessage(Context.Channel);

            GlobalUser globalUser = Program.globalUsers.Find(x => x.userID == targetUser.Id);

            if (!(globalUser?.toggleAnilist).GetValueOrDefault())
            {
                AnimeSearchResult searchResult = await Program._jikan.SearchAnime(search);
                if (searchResult?.Results.Count > 0)
                {
                    List<AnimeSearchEntry> results = searchResult.Results.ToList();
                    AnimeSearchEntry entry = results[0];
                    embed.Title = entry.Title;
                    embed.Description = entry.Description;
                    embed.Url = entry.URL;
                    embed.ThumbnailUrl = entry.ImageURL;
                    embed.AddField(
                        "Information",
                            "Type: " + entry.Type +
                            "\nEpisodes: " + (entry.Episodes == 0 ? "Unknown" : entry.Episodes.ToString()) +
                            "\nStatus: " + GetAnimeStatus(entry) +
                            "\nAired: " + GetDate(entry.StartDate.GetValueOrDefault(), entry.EndDate.GetValueOrDefault())
                    );

                    if (globalUser != null)
                    {
                        await globalUser.UpdateMALInfo();
                        UserAnimeList animeList = await Program._jikan.GetUserAnimeList(globalUser.MAL_Username);
                        AnimeListEntry malEntry = animeList.Anime.ToList().Find(x => x.MalId == entry.MalId);
                        if (malEntry != null)
                        {
                            embed.AddField(
                                Format.Sanitize(globalUser.MAL_Username) + " Stats",
                                (malEntry.Days.GetValueOrDefault() == 0 ? "" : "\nDays watched: " + malEntry.Days.GetValueOrDefault()) +
                                "\nEpisodes watched: " + malEntry.WatchedEpisodes +
                                "\nRating: " + malEntry.Score
                            );
                        }
                    }
                    else
                    {
                        embed.AddField(
                                Format.Sanitize(targetUser.Username) + " Stats",
                                "There are no stats available for this user."
                        );
                    }
                }
                else
                {
                    embed.Title = "No Anime Found";
                }
            }
            else
            {
                IAniMedia media = await AniMediaQuery.SearchMedia(search, AniMediaType.ANIME);
                if (media != null)
                {
                    embed.Title = media.title.english;
                    embed.Description = media.description;
                    embed.Url = media.siteUrl;
                    embed.ThumbnailUrl = media.coverImage.large;
                    embed.AddField(
                        "Information",
                            "Type: " + Enum.GetName(typeof(AniMediaType), media.type) +
                            "\nEpisodes: " + (media.episodes.GetValueOrDefault() == 0 ? "Unknown" : media.episodes.GetValueOrDefault().ToString()) +
                            "\nStatus: " + Enum.GetName(typeof(AniMediaStatus), media.status).Replace("_", " ") +
                            "\nAired: " + GetDate(media.startDate, media.endDate)
                    );

                    if (globalUser != null)
                    {
                        IAniMediaList list = await AniMediaListQuery.GetMediaList(globalUser.Anilist_Username, media.id.GetValueOrDefault(), AniMediaType.ANIME);
                        if(list != null)
                        {
                            embed.AddField(
                                Format.Sanitize(globalUser.Anilist_Username) + " Stats",
                                //(malEntry.Days.GetValueOrDefault() == 0 ? "" : "\nDays watched: " + list.day) +
                                "\nStatus: " + Enum.GetName(typeof(AniMediaListStatus), list.status.GetValueOrDefault()) +
                                "\nEpisodes watched: " + list.progress.GetValueOrDefault() +
                                "\nRating: " + list.score
                            );
                        }
                    }
                    else
                    {
                        embed.AddField(
                                Format.Sanitize(targetUser.Username) + " Stats",
                                "There are no stats available for this user."
                        );
                    }
                }
                else
                {
                    embed.Title = "No Anime Found";
                }
            }
            await embed.UpdateEmbed();
        }

        [Command("anime")]
        public async Task SearchAnime([Remainder]string search)
        {
            await SearchAnime(Context.User, search);
        }

        [Command("manga")]
        public async Task SearchManga(IUser targetUser, [Remainder]string search)
        {
            EmbedHandler embed = new EmbedHandler(targetUser, "Searching for " + search + "...");
            await embed.SendMessage(Context.Channel);

            GlobalUser globalUser = Program.globalUsers.Find(x => x.userID == targetUser.Id);

            if (!(globalUser?.toggleAnilist).GetValueOrDefault())
            {
                MangaSearchResult searchResult = await Program._jikan.SearchManga(search);
                if (searchResult?.Results.Count > 0)
                {
                    List<MangaSearchEntry> results = searchResult.Results.ToList();
                    MangaSearchEntry entry = results[0];
                    embed.Title = entry.Title;
                    embed.Description = entry.Description;
                    embed.Url = entry.URL;
                    embed.ThumbnailUrl = entry.ImageURL;
                    embed.AddField(
                        "Information",
                            "Type: " + entry.Type +
                            "\nVolumes: " + (entry.Volumes == 0 ? "Unknown" : entry.Volumes.ToString()) +
                            "\nChapters: " + (entry.Chapters == 0 ? "Unknown" : entry.Chapters.ToString()) +
                            "\nStatus: " + GetMangaStatus(entry) +
                            "\nPublished: " + GetDate(entry.StartDate.GetValueOrDefault(), entry.EndDate.GetValueOrDefault())
                    );

                    if (globalUser != null)
                    {
                        await globalUser.UpdateMALInfo();
                        UserMangaList mangaList = await Program._jikan.GetUserMangaList(globalUser.MAL_Username);
                        MangaListEntry malEntry = mangaList.Manga.ToList().Find(x => x.MalId == entry.MalId);
                        if (malEntry != null)
                        {
                            embed.AddField(
                                Format.Sanitize(globalUser.MAL_Username) + " Stats",
                                (malEntry.Days.GetValueOrDefault() == 0 ? "" : "\nDays read: " + malEntry.Days.GetValueOrDefault()) +
                                "\nVolumes read: " + malEntry.ReadVolumes +
                                "\nChapters read: " + malEntry.ReadChapters +
                                "\nRating: " + malEntry.Score
                            );
                        }
                    }
                    else
                    {
                        embed.AddField(
                                Format.Sanitize(targetUser.Username) + " Stats",
                                "There are no stats available for this user."
                        );
                    }
                }
                else
                {
                    embed.Title = "No Manga Found";
                }
            }
            else
            {
                IAniMedia media = await AniMediaQuery.SearchMedia(search, AniMediaType.MANGA);
                if (media != null)
                {
                    embed.Title = media.title.english;
                    embed.Description = media.description;
                    embed.Url = media.siteUrl;
                    embed.ThumbnailUrl = media.coverImage.large;
                    embed.AddField(
                        "Information",
                            "Type: " + Enum.GetName(typeof(AniMediaType), media.type) +
                            "\nVolumes: " + (media.volumes.GetValueOrDefault() == 0 ? "Unknown" : media.volumes.GetValueOrDefault().ToString()) +
                            "\nChapters: " + (media.chapters.GetValueOrDefault() == 0 ? "Unknown" : media.chapters.GetValueOrDefault().ToString()) +
                            "\nStatus: " + Enum.GetName(typeof(AniMediaStatus), media.status).Replace("_", " ") +
                            "\nAired: " + GetDate(media.startDate, media.endDate)
                    );

                    if (globalUser != null)
                    {
                        IAniMediaList list = await AniMediaListQuery.GetMediaList(globalUser.Anilist_Username, media.id.GetValueOrDefault(), AniMediaType.MANGA);
                        if (list != null)
                        {
                            embed.AddField(
                                Format.Sanitize(globalUser.Anilist_Username) + " Stats",
                                //(malEntry.Days.GetValueOrDefault() == 0 ? "" : "\nDays watched: " + list.day) +
                                "\nStatus: " + Enum.GetName(typeof(AniMediaListStatus), list.status.GetValueOrDefault()) +
                                "\nVolumes Read: " + list.progressVolumes +
                                "\nChapters Read: " + list.progress +
                                "\nRating: " + list.score
                            );
                        }
                    }
                    else
                    {
                        embed.AddField(
                                Format.Sanitize(targetUser.Username) + " Stats",
                                "There are no stats available for this user."
                        );
                    }
                }
                else
                {
                    embed.Title = "No Manga Found";
                }
            }
            await embed.UpdateEmbed();
        }

        [Command("manga")]
        public async Task SearchManga([Remainder]string search)
        {
            await SearchManga(Context.User, search);
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
            catch (Exception) { }

            return GetDate(startDateTime, endDateTime);
        }
    }
}
