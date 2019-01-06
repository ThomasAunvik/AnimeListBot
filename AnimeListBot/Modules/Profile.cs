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

namespace AnimeListBot.Modules
{
    public class Profile : ModuleBase<ICommandContext>
    {
        [Command("setup"), Summary(
            "Registers your discord account with MAL. Possible options: [mal, anilist]\n" +
            "MAL = 0\n" +
            "Anilist = 1"
        )]
        public async Task SetupProfile(GlobalUser.AnimeList animeList, string username)
        {
            EmbedHandler embed = new EmbedHandler(Context.User, "Setting up profile...");
            await embed.SendMessage(Context.Channel);

            Uri link = null;
            bool isValidLink = Uri.TryCreate(username, UriKind.Absolute, out link);

            if(animeList == GlobalUser.AnimeList.MAL)
            {
                if (isValidLink && username.Contains("myanimelist.net"))
                {
                    string usernamePart = link.Segments[link.Segments.Length - 1];
                    username = usernamePart;
                }

                UserProfile profile = await Program._jikan.GetUserProfile(username);
                if (profile == null)
                {
                    embed.Title = "Invalid Username.";
                    await embed.UpdateEmbed();
                    return;
                }

                await SetupMAL(profile, embed);
            }
            else if(animeList == GlobalUser.AnimeList.Anilist)
            {
                if (isValidLink && username.Contains("anilist.co"))
                {
                    string usernamePart = link.Segments[link.Segments.Length - 1];
                    username = usernamePart;
                }

                IAniUser profile = await AniUserQuery.GetUser(username);
                if (profile == null)
                {
                    embed.Title = "Invalid Username.";
                    await embed.UpdateEmbed();
                    return;
                }

                await SetupAnilist(profile, embed);
            }
            else
            {
                embed.Title = "Incorrect mode (Must be MAL or Anilist)";
                await embed.UpdateEmbed();
            }

            await Ranks.UpdateUserRole((IGuildUser)Context.User, embed);
        }

        public async Task SetupMAL(UserProfile profile, EmbedHandler embed)
        {
            GlobalUser user = Program.globalUsers.Find(x => x.userID == Context.User.Id);
            if (user == null)
            {
                user = new GlobalUser(Context.User);
                user.MAL_Username = profile.Username;
                Program.globalUsers.Add(user);

                embed.Title = "";
                embed.Description = "";
                embed.AddFieldSecure(new EmbedFieldBuilder()
                {
                    Name = "MAL Account Setup",
                    Value = "User registered " + profile.Username
                });
            }
            else
            {
                user.MAL_Username = profile.Username;

                embed.Title = "";
                embed.Description = "";
                embed.AddFieldSecure(new EmbedFieldBuilder()
                {
                    Name = "MAL Profile Updated",
                    Value = "Username updated to: " + profile.Username
                });
            }
            await embed.UpdateEmbed();
            await user.UpdateMALInfo();
            user.animeList = GlobalUser.AnimeList.MAL;
            user.SaveData();
        }

        public async Task SetupAnilist(IAniUser profile, EmbedHandler embed)
        {
            GlobalUser user = Program.globalUsers.Find(x => x.userID == Context.User.Id);
            if (user == null)
            {
                user = new GlobalUser(Context.User);
                user.Anilist_Username = profile.name;

                Program.globalUsers.Add(user);

                embed.Title = "";
                embed.Description = "";
                embed.AddFieldSecure(new EmbedFieldBuilder()
                {
                    Name = "Anilist Account Setup",
                    Value = "User registered: " + profile.name
                });
            }
            else
            {
                user.Anilist_Username = profile.name;

                embed.Title = "";
                embed.Description = "";
                embed.AddFieldSecure(new EmbedFieldBuilder()
                {
                    Name = "Anilist Profile Updated",
                    Value = "Username updated to: " + profile.name
                });
            }
            await embed.UpdateEmbed();
            await user.UpdateAnilistInfo();
            user.animeList = GlobalUser.AnimeList.Anilist;
            user.SaveData();
        }

        [Command("setlist")]
        [Summary(
            "Chooses which anime/manga list you want to use.\n" +
            "MAL = 0\n" +
            "Anilist = 1"
        )]
        public async Task SetList(GlobalUser.AnimeList animeList)
        {
            GlobalUser user = Program.globalUsers.Find(x => x.userID == Context.User.Id);
            if (user == null) return;

            EmbedHandler embed = new EmbedHandler(Context.User, "Setting List...");
            await embed.SendMessage(Context.Channel);
            
            if (animeList == GlobalUser.AnimeList.MAL)
            {
                await user.UpdateMALInfo();
                if (user.malProfile == null)
                {
                    embed.Title = "There is no MAL profile set";
                    await embed.UpdateEmbed();
                    return;
                }

                user.animeList = GlobalUser.AnimeList.MAL;
                embed.Title = "List set to MAL";
            }
            else if(animeList == GlobalUser.AnimeList.Anilist)
            {
                await user.UpdateAnilistInfo();
                if (user.anilistProfile == null)
                {
                    embed.Title = "There is no Anilist profile set";
                    await embed.UpdateEmbed();
                    return;
                }
                user.animeList = GlobalUser.AnimeList.Anilist;
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
            await Ranks.UpdateUserRole((IGuildUser)Context.User, embed);
            user.SaveData();
        }

        [Command("profile")]
        [Summary("Gets user profile, options are anime and manga")]
        public async Task GetProfile(IUser user, string option = "")
        {
            if (user == null) user = Context.User;
            EmbedHandler embed = new EmbedHandler(user, "Loading Profile Info...");
            await embed.SendMessage(Context.Channel);

            GlobalUser gUser = Program.globalUsers.Find(x => x.userID == user.Id);

            DiscordServer server = DiscordServer.GetServerFromID(Context.Guild.Id);
            ServerUser sUser = server.GetUserFromId(user.Id);

            option = option.ToLower();

            if (gUser != null && !string.IsNullOrWhiteSpace(gUser.GetAnimelistUsername()))
            {
                await gUser.UpdateCurrentAnimelist();

                embed.ThumbnailUrl = gUser.GetAnimelistThumbnail();
                embed.Url = gUser.GetAnimelistLink();

                string mangaRank = Context.Guild.GetRole(sUser.currentMangaRankId)?.Name;
                string animeRank = Context.Guild.GetRole(sUser.currentAnimeRankId)?.Name;

                if (option.ToLower() == "anime")
                {
                    embed.Title = gUser.GetAnimelistUsername() + " Anime Statistics";
                    embed.Fields.Clear();
                    
                    if (!string.IsNullOrWhiteSpace(animeRank)) embed.AddFieldSecure("Rank", animeRank, false);

                    embed.AddFieldSecure("Days", gUser.GetAnimeWatchDays(), true);
                    embed.AddFieldSecure("Mean Score", gUser.GetAnimeMeanScore(), true);
                    embed.AddFieldSecure("Total Entries", gUser.GetAnimeTotalEntries(), true);
                    embed.AddFieldSecure("Episodes", gUser.GetAnimeEpisodesWatched(), true);
                    embed.AddFieldSecure("Rewatched", gUser.GetAnimeRewatched(), false);

                    embed.AddFieldSecure("Watching", gUser.GetAnimeWatching(), true);
                    embed.AddFieldSecure("Completed", gUser.GetAnimeCompleted(), true);
                    embed.AddFieldSecure("On-Hold", gUser.GetAnimeOnHold(), true);
                    embed.AddFieldSecure("Dropped", gUser.GetAnimeDropped(), true);
                    embed.AddFieldSecure("Plan to Watch", gUser.GetAnimePlanToWatch(), true);
                }
                else if(option.ToLower() == "manga")
                {
                    embed.Title = gUser.GetAnimelistUsername() + " Manga Statistics";
                    embed.Fields.Clear();
                    
                    if (!string.IsNullOrWhiteSpace(mangaRank)) embed.AddFieldSecure("Rank", mangaRank, false);

                    embed.AddFieldSecure("Days", gUser.GetMangaReadDays(), true);
                    embed.AddFieldSecure("Mean Score", gUser.GetMangaMeanScore(), true);
                    embed.AddFieldSecure("Total Entries", gUser.GetMangaTotalEntries(), true);
                    embed.AddFieldSecure("Chapters", gUser.GetMangaChaptersRead(), true);
                    embed.AddFieldSecure("Volumes", gUser.GetMangaVolumesRead(), true);
                    embed.AddFieldSecure("Reread", gUser.GetMangaReread(), true);

                    embed.AddFieldSecure("Reading", gUser.GetMangaReading(), true);
                    embed.AddFieldSecure("Completed", gUser.GetMangaCompleted(), true);
                    embed.AddFieldSecure("On-Hold", gUser.GetMangaOnHold(), true);
                    embed.AddFieldSecure("Dropped", gUser.GetMangaDropped(), true);
                    embed.AddFieldSecure("Plan to Read", gUser.GetMangaPlanToRead(), true);
                }
                else
                {
                    embed.Title = gUser.GetAnimelistUsername() + " Profile";

                    embed.AddFieldSecure("Anime:", (string.IsNullOrWhiteSpace(animeRank) ? "" : "**Rank:** " + animeRank) +
                                             "\n**Days:** " + gUser.GetAnimeWatchDays());
                    embed.AddFieldSecure("Manga:", (string.IsNullOrWhiteSpace(mangaRank) ? "" : "**Rank:** " + mangaRank) +
                                             "\n**Days:** " + gUser.GetMangaReadDays());
                }
            }
            else
            {
                string username = user == null ? Context.User.Username : user.Username;
                embed.Title = username + " has not registered a MAL or Anilist account to his discord user.";
            }
            await embed.UpdateEmbed();
        }

        [Command("profile")]
        public async Task GetProfile(string option = "")
        {
            await GetProfile(null, option);
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

        [Command("Leaderboard")]
        public async Task Leaderboard()
        {
            EmbedHandler embed = new EmbedHandler(Context.User, "Getting Leaderboard...");
            await embed.SendMessage(Context.Channel);
            embed.Title = "Leaderboard";

            GlobalUser gUser = Program.globalUsers.Find(x => x.userID == Context.User.Id);
            DiscordServer server = DiscordServer.GetServerFromID(Context.Guild.Id);

            List<GlobalUser> globalServerUsers = Program.globalUsers.FindAll(x => server.Users.Exists(y => x.userID == y.userID));
            
            List<GlobalUser> animeLeaderboard = globalServerUsers.OrderByDescending(x => x.GetAnimeWatchDays()).ToList();
            animeLeaderboard.RemoveAll(x => x.GetAnimeWatchDays() == 0);

            List<GlobalUser> mangaLeaderboard = globalServerUsers.OrderByDescending(x => x.GetMangaReadDays()).ToList();
            mangaLeaderboard.RemoveAll(x => x.GetMangaReadDays() == 0);

            if (animeLeaderboard.Count <= 0 && mangaLeaderboard.Count <= 0)
            {
                await ReplyAsync("There is no lead to view.");
                return;
            }

            if (animeLeaderboard.Count > 0)
            {
                GlobalUser animeLeadUser = animeLeaderboard[0];

                EmbedFieldBuilder animeBoardField = new EmbedFieldBuilder();
                animeBoardField.Name = "Anime Leaderboard";
                animeBoardField.IsInline = true;

                // TOP
                animeBoardField.Value = "#1: " + Format.Sanitize(animeLeadUser.GetAnimelistUsername()) + " - " + animeLeadUser.GetAnimeWatchDays() + " days";

                // THE REST
                for (int i = 1; i < (animeLeaderboard.Count >= 10 ? 10 : animeLeaderboard.Count); i++)
                {
                    animeLeadUser = animeLeaderboard[i];
                    animeBoardField.Value += "\n#" + (i + 1) + ": " + Format.Sanitize(animeLeadUser.GetAnimelistUsername()) + " - " + animeLeadUser.GetAnimeWatchDays() + " days";
                }

                // YOURS
                if (animeLeaderboard.Exists(x => x.GetAnimeWatchDays() == gUser.GetAnimeWatchDays()))
                {
                    int yourIndex = animeLeaderboard.FindIndex(x => x.GetAnimeWatchDays() == gUser.GetAnimeWatchDays());
                    animeBoardField.Value += "\n\n**Your Position: #" + (yourIndex + 1) + ":** " + Format.Sanitize(gUser.GetAnimelistUsername()) + " - " + gUser.GetAnimeWatchDays() + " days.";
                }
                embed.AddFieldSecure(animeBoardField);
            }

            if(mangaLeaderboard.Count > 0)
            {
                GlobalUser mangaLeadUser = mangaLeaderboard[0];

                EmbedFieldBuilder mangaBoardField = new EmbedFieldBuilder();
                mangaBoardField.Name = "Manga Leaderboard";
                mangaBoardField.IsInline = true;

                // TOP
                mangaBoardField.Value = "#1: " + Format.Sanitize(mangaLeadUser.GetAnimelistUsername()) + " - " + mangaLeadUser.GetMangaReadDays() + " days";

                // THE REST
                for (int i = 1; i < (mangaLeaderboard.Count >= 10 ? 10 : mangaLeaderboard.Count); i++)
                {
                    mangaLeadUser = mangaLeaderboard[i];
                    mangaBoardField.Value += "\n#" + (i + 1) + ": " + Format.Sanitize(mangaLeadUser.GetAnimelistUsername()) + " - " + mangaLeadUser.GetMangaReadDays() + " days";
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
    }
}
