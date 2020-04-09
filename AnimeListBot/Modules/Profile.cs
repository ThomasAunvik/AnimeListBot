using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

using AnimeListBot.Handler;
using Discord;
using JikanDotNet;
using AnimeListBot.Handler.Anilist;
using System.Globalization;
using Discord.WebSocket;

namespace AnimeListBot.Modules
{
    public class Profile : ModuleBase<ICommandContext>
    {
        [Command("setup"), Summary(
            "Registers your discord account with MAL. Possible options: [mal, anilist]\n" +
            "MAL = 0\n" +
            "Anilist = 1"
        )]
        public async Task SetupProfile(DiscordUser.AnimeList animeList, string username)
        {
            EmbedHandler embed = new EmbedHandler(Context.User, "Setting up profile...");
            await embed.SendMessage(Context.Channel);

            DiscordServer server = await DatabaseRequest.GetServerById(Context.Guild.Id);
            DiscordUser user;
            if (!await DatabaseRequest.DoesUserIdExist(Context.User.Id))
                await DatabaseRequest.CreateUser(user = new DiscordUser(Context.User));
            else user = await DatabaseRequest.GetUserById(Context.User.Id);
            Uri link = null;
            bool isValidLink = Uri.TryCreate(username, UriKind.Absolute, out link);

            if(animeList == DiscordUser.AnimeList.MAL)
            {
                if (isValidLink && username.Contains("myanimelist.net"))
                {
                    string usernamePart = link.Segments[link.Segments.Length - 1];
                    username = usernamePart;
                }

                if (!await user.UpdateMALInfo(username))
                {
                    embed.Title = "Invalid Username.";
                    await embed.UpdateEmbed();
                    return;
                }
                embed.Title = "";
                embed.Description = "";
                embed.AddFieldSecure(new EmbedFieldBuilder()
                {
                    Name = "MAL Account Setup",
                    Value = "User registered " + username
                });

                user.animeList = DiscordUser.AnimeList.MAL;
            }
            else if(animeList == DiscordUser.AnimeList.Anilist)
            {
                if (isValidLink && username.Contains("anilist.co"))
                {
                    string usernamePart = link.Segments[link.Segments.Length - 1];
                    username = usernamePart;
                }

                if (!await user.UpdateAnilistInfo(username))
                {
                    embed.Title = "Invalid Username.";
                    await embed.UpdateEmbed();
                    return;
                }
                embed.Title = "";
                embed.Description = "";
                embed.AddFieldSecure(new EmbedFieldBuilder()
                {
                    Name = "Anilist Account Setup",
                    Value = "User registered: " + username
                });
                user.animeList = DiscordUser.AnimeList.Anilist;
            }
            else
            {
                embed.Title = "Incorrect mode (Must be MAL or Anilist)";
                await embed.UpdateEmbed();
            }

            await DatabaseRequest.UpdateUser(user);
            await Ranks.UpdateUserRole(server, user, embed);
        }

        [Command("setlist")]
        [Summary(
            "Chooses which anime/manga list you want to use.\n" +
            "MAL = 0\n" +
            "Anilist = 1"
        )]
        public async Task SetList(DiscordUser.AnimeList animeList)
        {
            if (!await DatabaseRequest.DoesUserIdExist(Context.User.Id)) return;
            DiscordUser user = await DatabaseRequest.GetUserById(Context.User.Id);
            DiscordServer server = await DatabaseRequest.GetServerById(Context.Guild.Id);

            EmbedHandler embed = new EmbedHandler(Context.User, "Setting List...");
            await embed.SendMessage(Context.Channel);

            await user.UpdateUserInfo();
            if (animeList == DiscordUser.AnimeList.MAL)
            {
                if (user.malProfile == null)
                {
                    embed.Title = "There is no MAL profile set";
                    await embed.UpdateEmbed();
                    return;
                }

                user.animeList = DiscordUser.AnimeList.MAL;
                embed.Title = "List set to MAL";
            }
            else if(animeList == DiscordUser.AnimeList.Anilist)
            {
                if (user.anilistProfile == null)
                {
                    embed.Title = "There is no Anilist profile set";
                    await embed.UpdateEmbed();
                    return;
                }
                user.animeList = DiscordUser.AnimeList.Anilist;
                embed.Title = "List set to Anilist";
            }
            else
            {
                await ReplyAsync("Incorrect mode, only MAL and Anilist");
                embed.Title = "Incorrect mode, only MAL and Anilist";
                await embed.UpdateEmbed();
                return;
            }
            await embed.UpdateEmbed();

            await Ranks.UpdateUserRole(server, user, embed);
        }

        [Command("profile")]
        [Summary("Gets user profile, options are anime and manga")]
        public async Task GetProfile(IUser targetUser, string option = "")
        {
            if (targetUser == null) targetUser = Context.User;

            if (!await DatabaseRequest.DoesUserIdExist(targetUser.Id)) return;
            DiscordUser user = await DatabaseRequest.GetUserById(targetUser.Id);
            DiscordServer server = await DatabaseRequest.GetServerById(Context.Guild.Id);

            EmbedHandler embed = new EmbedHandler(targetUser, "Loading Profile Info...");
            await embed.SendMessage(Context.Channel);

            option = option.ToLower();

            if (user != null && !string.IsNullOrWhiteSpace(user.GetAnimelistUsername()))
            {
                embed.ThumbnailUrl = user.GetAnimelistThumbnail();
                embed.Url = user.GetAnimelistLink();

                if (option == "anime")
                {
                    embed.Title = user.GetAnimelistUsername() + " Anime Statistics";
                    embed.Fields.Clear();

                    (ulong, double) animeRank = user.GetAnimeServerRank(server);
                    IRole animeRankRole = null;
                    if (animeRank.Item1 != 0) animeRankRole = Context.Guild.GetRole(animeRank.Item1);
                    if (animeRankRole != null) embed.AddFieldSecure("Rank", animeRankRole.Name, false);

                    embed.AddFieldSecure("Days", user.GetAnimeWatchDays(), true);
                    embed.AddFieldSecure("Mean Score", user.GetAnimeMeanScore(), true);
                    embed.AddFieldSecure("Total Entries", user.GetAnimeTotalEntries(), true);
                    embed.AddFieldSecure("Episodes", user.GetAnimeEpisodesWatched(), true);
                    embed.AddFieldSecure("Rewatched", user.GetAnimeRewatched(), false);

                    embed.AddFieldSecure("Watching", user.GetAnimeWatching(), true);
                    embed.AddFieldSecure("Completed", user.GetAnimeCompleted(), true);
                    embed.AddFieldSecure("On-Hold", user.GetAnimeOnHold(), true);
                    embed.AddFieldSecure("Dropped", user.GetAnimeDropped(), true);
                    embed.AddFieldSecure("Plan to Watch", user.GetAnimePlanToWatch(), true);
                }
                else if(option == "manga")
                {
                    embed.Title = user.GetAnimelistUsername() + " Manga Statistics";
                    embed.Fields.Clear();

                    (ulong, double) mangaRank = user.GetMangaServerRank(server);
                    IRole mangaRankRole = null;
                    if (mangaRank.Item1 != 0) mangaRankRole = Context.Guild.GetRole(mangaRank.Item1);
                    if (mangaRankRole != null) embed.AddFieldSecure("Rank", mangaRankRole.Name, false);

                    embed.AddFieldSecure("Days", user.GetMangaReadDays(), true);
                    embed.AddFieldSecure("Mean Score", user.GetMangaMeanScore(), true);
                    embed.AddFieldSecure("Total Entries", user.GetMangaTotalEntries(), true);
                    embed.AddFieldSecure("Chapters", user.GetMangaChaptersRead(), true);
                    embed.AddFieldSecure("Volumes", user.GetMangaVolumesRead(), true);
                    embed.AddFieldSecure("Reread", user.GetMangaReread(), true);

                    embed.AddFieldSecure("Reading", user.GetMangaReading(), true);
                    embed.AddFieldSecure("Completed", user.GetMangaCompleted(), true);
                    embed.AddFieldSecure("On-Hold", user.GetMangaOnHold(), true);
                    embed.AddFieldSecure("Dropped", user.GetMangaDropped(), true);
                    embed.AddFieldSecure("Plan to Read", user.GetMangaPlanToRead(), true);
                }
                else if(option == "cache")
                {
                    if (user.animeList == DiscordUser.AnimeList.MAL)
                    {
                        CultureInfo en_US = new CultureInfo("en-US");

                        embed.Title = user.GetAnimelistUsername() + " Profile Cache";
                        embed.AddFieldSecure(
                            "Cache",
                            "Is Cached: " + user.malProfile.RequestCached.ToString() + (
                            user.malProfile.RequestCached ?
                            "\nCache Expiry In: " + new DateTime().AddSeconds(user.malProfile.RequestCacheExpiry).ToString("mm:ss", en_US) : "")
                        );
                    }
                    else if(user.animeList == DiscordUser.AnimeList.Anilist)
                    {
                        embed.Title = user.GetAnimelistUsername() + " Profile Cache";
                        embed.Description = "There is no cache option yet for Anilist";
                    }
                }
                else
                {
                    embed.Title = user.GetAnimelistUsername() + " Profile";

                    (ulong, double) animeRank = user.GetAnimeServerRank(server);
                    (ulong, double) mangaRank = user.GetMangaServerRank(server);
                    IRole animeRankRole = null;
                    if (animeRank.Item1 != 0) animeRankRole = Context.Guild.GetRole(animeRank.Item1);
                    IRole mangaRankRole = null;
                    if (mangaRank.Item1 != 0) mangaRankRole = Context.Guild.GetRole(mangaRank.Item1);

                    embed.AddFieldSecure("Anime:", (animeRankRole == null ? "No Rank" : "**Rank:** " + animeRankRole.Name) +
                                             "\n**Days:** " + user.GetAnimeWatchDays());
                    embed.AddFieldSecure("Manga:", (mangaRankRole == null ? "No Rank" : "**Rank:** " + mangaRankRole.Name) +
                                             "\n**Days:** " + user.GetMangaReadDays());
                            
                }

                await Ranks.UpdateUserRole(server, user, null);
            }
            else
            {
                string username = targetUser == null ? Context.User.Username : targetUser.Username;
                embed.Title = username + " has not registered a MAL or Anilist account to his discord user.";
            }
            await embed.UpdateEmbed();
        }

        [Command("profile")]
        public async Task GetProfile(string nameOrOption = "", string option = "")
        {
            IUser targetUser = null;
            if (nameOrOption != "anime" && nameOrOption != "manga" && nameOrOption != string.Empty)
            {
                List<SocketGuildUser> searchUsers = Program._client.GetGuild(Context.Guild.Id).Users.Where(x =>
                    x.Username.ToLower().Contains(nameOrOption.ToLower())
                ).ToList();

                if (searchUsers.Count > 0)
                {
                    targetUser = searchUsers[0];
                }
            }

            await GetProfile(targetUser, targetUser == null ? nameOrOption : option);
        }

        [Command("animeprofile")]
        public async Task GetAnimeProfile(IUser user = null)
        {
            await GetProfile(user, "anime");
        }

        [Command("mangaprofile")]
        public async Task GetMangaProfile(IUser user = null)
        {
            await GetProfile(user, "manga");
        }

        // TODO: Temporary Disablement of Leaderboard till days fixed.
        [Command("Leaderboard")]
        public async Task Leaderboard(int page = 1)
        {
            EmbedHandler embed = new EmbedHandler(Context.User, "Getting Leaderboard...");
            await embed.SendMessage(Context.Channel);

            DiscordUser gUser = await DatabaseRequest.GetUserById(Context.User.Id);
            DiscordServer server = await DatabaseRequest.GetServerById(Context.Guild.Id);


            IReadOnlyCollection<IGuildUser> guildUsers = await Context.Guild.GetUsersAsync();
            List<DiscordUser> globalServerUsers = guildUsers.Select(async x => await DatabaseRequest.GetUserById(x.Id, false))
                                                            .Select(t => t.Result)
                                                            .Where(i => i != null)
                                                            .ToList();

            List<DiscordUser> animeLeaderboard = globalServerUsers.OrderByDescending(x => x.GetAnimeWatchDays()).ToList();
            animeLeaderboard.RemoveAll(x => x.GetAnimeWatchDays() == 0);

            List<DiscordUser> mangaLeaderboard = globalServerUsers.OrderByDescending(x => x.GetMangaReadDays()).ToList();
            mangaLeaderboard.RemoveAll(x => x.GetMangaReadDays() == 0);

            if (animeLeaderboard.Count <= 0 && mangaLeaderboard.Count <= 0)
            {
                embed.Title = "There is no lead to view.";
                await embed.UpdateEmbed();
                return;
            }

            float animePages = MathF.Ceiling(animeLeaderboard.Count / 10f);
            float mangaPages = MathF.Ceiling(mangaLeaderboard.Count / 10f);
            float maxPages = MathF.Round((animePages > mangaPages ? animePages : mangaPages));
            if (maxPages > 1)
            {
                embed.Description = "Page: " + page + "/" + maxPages;
            }
            
            if(page > maxPages)
            {
                embed.Title = "Page value is too high. Max Page: " + maxPages;
                embed.Description = "";
                await embed.UpdateEmbed();
                return;
            }
            
            embed.Title = "Leaderboard";
            if (animeLeaderboard.Count > 0 && animeLeaderboard.Count >= ((page - 1) * 10))
            {
                DiscordUser animeLeadUser = animeLeaderboard[0];

                EmbedFieldBuilder animeBoardField = new EmbedFieldBuilder();
                animeBoardField.Name = "Anime Leaderboard";
                animeBoardField.IsInline = true;
                
                int startIndex = 0 + ((page - 1) * 10);
                for (int i = startIndex; i < (animeLeaderboard.Count > startIndex + 10 ? startIndex + 10 : animeLeaderboard.Count); i++)
                {
                    animeLeadUser = animeLeaderboard[i];

                    if (i != startIndex)
                    {
                        animeBoardField.Value += "\n";
                    }
                    IUser leadDiscordUser = Program._client.GetUser(animeLeadUser.userID);
                    animeBoardField.Value += "#" + (i + 1) + ": " + Format.Sanitize(animeLeadUser.GetAnimelistUsername()) + " (" + leadDiscordUser.Mention + ") - " + animeLeadUser.GetAnimeWatchDays() + " days";
                }

                // YOURS
                if (animeLeaderboard.Exists(x => x.GetAnimeWatchDays() == gUser.GetAnimeWatchDays()))
                {
                    int yourIndex = animeLeaderboard.FindIndex(x => x.GetAnimeWatchDays() == gUser.GetAnimeWatchDays());
                    animeBoardField.Value += "\n\n**Your Position: #" + (yourIndex + 1) + ":** " + Format.Sanitize(gUser.GetAnimelistUsername()) + " - " + gUser.GetAnimeWatchDays() + " days.";
                }
                embed.AddFieldSecure(animeBoardField);
            }

            if(mangaLeaderboard.Count > 0 && mangaLeaderboard.Count > ((page - 1) * 10))
            {
                DiscordUser mangaLeadUser = mangaLeaderboard[0];

                EmbedFieldBuilder mangaBoardField = new EmbedFieldBuilder();
                mangaBoardField.Name = "Manga Leaderboard";
                mangaBoardField.IsInline = true;
                
                int startIndex = 0 + ((page - 1) * 10);
                for (int i = startIndex; i < (mangaLeaderboard.Count >= startIndex + 10 ? startIndex + 10 : mangaLeaderboard.Count); i++)
                {
                    mangaLeadUser = mangaLeaderboard[i];

                    if (i != startIndex)
                    {
                        mangaBoardField.Value += "\n";
                    }

                    IUser leadDiscordUser = Program._client.GetUser(mangaLeadUser.userID);
                    mangaBoardField.Value += "#" + (i + 1) + ": " + Format.Sanitize(mangaLeadUser.GetAnimelistUsername()) + " (" + leadDiscordUser.Mention + ") - " + mangaLeadUser.GetMangaReadDays() + " days";
                }

                // YOURS
                if (mangaLeaderboard.Exists(x => x.GetMangaReadDays() == gUser.GetMangaReadDays()))
                {
                    int yourIndex = mangaLeaderboard.FindIndex(x => x.GetMangaReadDays() == gUser.GetMangaReadDays());
                    mangaBoardField.Value += "\n\n**Your Position: #" + (yourIndex + 1) + ":** " + Format.Sanitize(gUser.GetAnimelistUsername()) + " - " + gUser.GetMangaReadDays() + " days.";
                }
                embed.AddFieldSecure(mangaBoardField);
            }
            
            await embed.UpdateEmbed();
        }

        [Command("resetuser")]
        public async Task RemoveUser(string confirm = "n")
        {
            DiscordUser user = await DatabaseRequest.GetUserById(Context.User.Id);

            EmbedHandler embed = new EmbedHandler(Context.User);
            if(user == null)
            {
                embed.Title = "User does not exist in the database.";
                await embed.SendMessage(Context.Channel);
                return;
            }

            if(confirm == "Y")
            {
                embed.Title = "Your user info is removed from the bot.";
                await DatabaseRequest.RemoveUser(user);
            }
            else
            {
                embed.Title = "Confirm your user info deletion from bot with the argument 'Y'";
            }
            await embed.SendMessage(Context.Channel);
        }
    }
}
