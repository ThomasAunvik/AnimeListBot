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
        [Command("setup"), Summary("Registers your discord account with MAL. Possible options: [mal, anilist]")]
        public async Task SetupProfile(string option, string username)
        {
            option = option.ToLower();

            Uri link = null;
            bool isValidLink = Uri.TryCreate(username, UriKind.Absolute, out link);

            if(option == "mal" || option == "myanimelist")
            {
                if (isValidLink && username.Contains("myanimelist.net"))
                {
                    string usernamePart = link.Segments[link.Segments.Length - 1];
                    username = usernamePart;
                }

                UserProfile profile = await Program._jikan.GetUserProfile(username);
                if (profile == null)
                {
                    await ReplyAsync("Invalid Username.");
                    return;
                }

                await SetupMAL(profile);
            }else if(option == "ani" || option == "anilist")
            {
                if (isValidLink && username.Contains("anilist.co"))
                {
                    string usernamePart = link.Segments[link.Segments.Length - 1];
                    username = usernamePart;
                }

                IAnilistUser profile = await UserQuery.GetUser(username);
                if (profile == null)
                {
                    await ReplyAsync("Invalid Username.");
                    return;
                }

                await SetupAnilist(profile);
            }
            else
            {
                await ReplyAsync("Incorrect mode (Must be MAL)");
            }

            await Ranks.UpdateUserRole((IGuildUser)Context.User);
        }

        public async Task SetupMAL(UserProfile profile)
        {
            GlobalUser user = Program.globalUsers.Find(x => x.userID == Context.User.Id);
            if (user == null)
            {
                user = new GlobalUser(Context.User);
                user.MAL_Username = profile.Username;

                Program.globalUsers.Add(user);

                EmbedBuilder embed = new EmbedBuilder()
                {
                    Title = "",
                    Description = "",
                    Fields = new List<EmbedFieldBuilder>()
                        {
                            new EmbedFieldBuilder()
                            {
                                Name = "MAL Account Setup",
                                Value = "User registered " + profile.Username
                            }
                        },

                    Color = Program.embedColor
                };
                await ReplyAsync("", false, embed.Build());
            }
            else
            {
                user.MAL_Username = profile.Username;

                EmbedBuilder embed = new EmbedBuilder()
                {
                    Title = "",
                    Description = "",
                    Fields = new List<EmbedFieldBuilder>()
                        {
                            new EmbedFieldBuilder()
                            {
                                Name = "MAL Profile Updated",
                                Value = "Username updated to: " + profile.Username
                            }
                        },

                    Color = Program.embedColor
                };
                await ReplyAsync("", false, embed.Build());
            }
            await user.UpdateMALInfo();
            user.toggleAnilist = false;
            user.SaveData();
        }

        public async Task SetupAnilist(IAnilistUser profile)
        {
            GlobalUser user = Program.globalUsers.Find(x => x.userID == Context.User.Id);
            if (user == null)
            {
                user = new GlobalUser(Context.User);
                user.Anilist_Username = profile.name;

                Program.globalUsers.Add(user);

                EmbedBuilder embed = new EmbedBuilder()
                {
                    Title = "",
                    Description = "",
                    Fields = new List<EmbedFieldBuilder>()
                        {
                            new EmbedFieldBuilder()
                            {
                                Name = "Anilist Account Setup",
                                Value = "User registered " + profile.name
                            }
                        },

                    Color = Program.embedColor
                };
                await ReplyAsync("", false, embed.Build());
            }
            else
            {
                user.Anilist_Username = profile.name;

                EmbedBuilder embed = new EmbedBuilder()
                {
                    Title = "",
                    Description = "",
                    Fields = new List<EmbedFieldBuilder>()
                        {
                            new EmbedFieldBuilder()
                            {
                                Name = "Anilist Profile Updated",
                                Value = "Username updated to: " + profile.name
                            }
                        },

                    Color = Program.embedColor
                };
                await ReplyAsync("", false, embed.Build());
            }
            await user.UpdateAnilistInfo();
            user.toggleAnilist = true;
            user.SaveData();
        }

        [Command("setlist")]
        [Summary("Chooses which anime/manga list you want to use.")]
        public async Task SetList(string option)
        {
            GlobalUser user = Program.globalUsers.Find(x => x.userID == Context.User.Id);
            if (user == null) return;

            option = option.ToLower();
 
            if (option == "mal" || option == "myanimelist")
            {
                user.toggleAnilist = false;
                await ReplyAsync("List set to MAL");
            }else if(option == "ani" || option == "anilist")
            {
                user.toggleAnilist = true;
                await ReplyAsync("List set to Anilist");
            }
            else
            {
                await ReplyAsync("Incorrect mode, only MAL and Anilist");
                return;
            }
            await Ranks.UpdateUserRole((IGuildUser)Context.User);
            user.SaveData();
        }

        [Command("profile")]
        [Summary("Gets user profile, options are anime and manga")]
        public async Task GetProfile(IUser user, string option = "")
        {
            if (user == null) user = Context.User;

            GlobalUser gUser = Program.globalUsers.Find(x => x.userID == user.Id);

            DiscordServer server = DiscordServer.GetServerFromID(Context.Guild.Id);
            ServerUser sUser = server.GetUserFromId(user.Id);

            option = option.ToLower();

            if (gUser != null && !string.IsNullOrWhiteSpace(gUser.GetAnimelistUsername()))
            {
                if(!gUser.toggleAnilist) await gUser.UpdateMALInfo();
                else await gUser.UpdateAnilistInfo();

                EmbedBuilder embed = new EmbedBuilder()
                {
                    ThumbnailUrl = gUser.GetAnimelistThumbnail(),
                    Url = gUser.GetAnimelistLink(),

                    Author = new EmbedAuthorBuilder() { Name = user.Username, IconUrl = user.GetAvatarUrl() },
                    Color = Program.embedColor,
                };

                string mangaRank = Context.Guild.GetRole(sUser.currentMangaRankId)?.Name;
                string animeRank = Context.Guild.GetRole(sUser.currentAnimeRankId)?.Name;

                if (option.ToLower() == "anime")
                {
                    embed.Title = gUser.GetAnimelistUsername() + " Anime Statistics";
                    embed.Fields.Clear();
                    
                    if (!string.IsNullOrWhiteSpace(animeRank)) embed.AddField("Rank", animeRank, false);

                    embed.AddField("Days", gUser.GetAnimeWatchDays(), true);
                    embed.AddField("Mean Score", gUser.GetAnimeMeanScore(), true);
                    embed.AddField("Total Entries", gUser.GetAnimeTotalEntries(), true);
                    embed.AddField("Episodes", gUser.GetAnimeEpisodesWatched(), true);
                    embed.AddField("Rewatched", gUser.GetAnimeRewatched(), false);

                    embed.AddField("Watching", gUser.GetAnimeWatching(), true);
                    embed.AddField("Completed", gUser.GetAnimeCompleted(), true);
                    embed.AddField("On-Hold", gUser.GetAnimeOnHold(), true);
                    embed.AddField("Dropped", gUser.GetAnimeDropped(), true);
                    embed.AddField("Plan to Watch", gUser.GetAnimePlanToWatch(), true);
                }
                else if(option.ToLower() == "manga")
                {
                    embed.Title = gUser.GetAnimelistUsername() + " Manga Statistics";
                    embed.Fields.Clear();
                    
                    if (!string.IsNullOrWhiteSpace(mangaRank)) embed.AddField("Rank", mangaRank, false);

                    embed.AddField("Days", gUser.GetMangaReadDays(), true);
                    embed.AddField("Mean Score", gUser.GetMangaMeanScore(), true);
                    embed.AddField("Total Entries", gUser.GetMangaTotalEntries(), true);
                    embed.AddField("Chapters", gUser.GetMangaChaptersRead(), true);
                    embed.AddField("Volumes", gUser.GetMangaVolumesRead(), true);
                    embed.AddField("Reread", gUser.GetMangaReread(), true);

                    embed.AddField("Reading", gUser.GetMangaReading(), true);
                    embed.AddField("Completed", gUser.GetMangaCompleted(), true);
                    embed.AddField("On-Hold", gUser.GetMangaOnHold(), true);
                    embed.AddField("Dropped", gUser.GetMangaDropped(), true);
                    embed.AddField("Plan to Read", gUser.GetMangaPlanToRead(), true);
                }
                else
                {
                    embed.Title = gUser.GetAnimelistUsername() + " Profile";

                    embed.AddField("Anime:", (string.IsNullOrWhiteSpace(animeRank) ? "" : "**Rank:** " + animeRank) +
                                             "\n**Days:** " + gUser.GetAnimeWatchDays());
                    embed.AddField("Manga:", (string.IsNullOrWhiteSpace(mangaRank) ? "" : "**Rank:** " + mangaRank) +
                                             "\n**Days:** " + gUser.GetMangaReadDays());
                }

                await ReplyAsync("", false, embed.Build());
            }
            else
            {
                string username = user == null ? Context.User.Username : user.Username;
                await ReplyAsync(username + " has not registered a MAL or Anilist account to his discord user.");
            }
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

            EmbedBuilder embed = new EmbedBuilder();
            embed.Title = "Leaderboard";

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
                embed.AddField(animeBoardField);
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
                embed.AddField(mangaBoardField);
            }
            await ReplyAsync("", false, embed.Build());
        }
    }
}
