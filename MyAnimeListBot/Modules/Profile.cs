using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using MALBot.Handler;
using Discord;
using JikanDotNet;

namespace MALBot.Modules
{
    public class Profile : ModuleBase<ICommandContext>
    {
        [Command("setup"), Summary("Registers your discord account with MAL. Possible options: [mal]")]
        public async Task SetupProfile(string option, string username)
        {
            Uri link = null;
            bool isValidLink = Uri.TryCreate(username, UriKind.Absolute, out link);
            bool containsMAL = username.Contains("myanimelist");
            if (isValidLink && containsMAL)
            {
                string usernamePart = link.Segments[link.Segments.Length - 1];
                username = usernamePart;
            }

            UserProfile profile = await Program._jikan.GetUserProfile(username);
            if(profile == null)
            {
                await ReplyAsync("Invalid Username.");
                return;
            }

            if(option == "mal" || option == "myanimelist")
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

                await Ranks.UpdateUserRole((IGuildUser)Context.User);

                user.SaveData();
            }
        }

        [Command("profile")]
        [Summary("Gets user profile, options are anime and manga")]
        public async Task GetProfile(IUser user, string option = "")
        {
            if (user == null) user = Context.User;

            GlobalUser gUser = Program.globalUsers.Find(x => x.userID == user.Id);

            DiscordServer server = DiscordServer.GetServerFromID(Context.Guild.Id);
            ServerUser sUser = server.GetUserFromId(user.Id);


            if (gUser != null && !string.IsNullOrWhiteSpace(gUser.MAL_Username))
            {
                await gUser.UpdateMALInfo();
                

                EmbedBuilder embed = new EmbedBuilder()
                {
                    ThumbnailUrl = gUser.imageURL,
                    Url = gUser.profile.URL,

                    Author = new EmbedAuthorBuilder() { Name = user.Username, IconUrl = user.GetAvatarUrl() },
                    Color = Program.embedColor,
                };

                string mangaRank = Context.Guild.GetRole(sUser.currentMangaRankId)?.Name;
                string animeRank = Context.Guild.GetRole(sUser.currentAnimeRankId)?.Name;

                if (option.ToLower() == "anime")
                {
                    embed.Title = gUser.MAL_Username + " Anime Statistics";
                    embed.Fields.Clear();
                    
                    if (!string.IsNullOrWhiteSpace(animeRank)) embed.AddField("Rank", animeRank, false);

                    embed.AddField("Days", gUser.daysWatchedAnime.ToString(), true);
                    embed.AddField("Mean Score", gUser.profile.AnimeStatistics.MeanScore, true);
                    embed.AddField("Total Entries", gUser.profile.AnimeStatistics.TotalEntries, true);
                    embed.AddField("Episodes", gUser.profile.AnimeStatistics.EpisodesWatched, true);
                    embed.AddField("Rewatched", gUser.profile.AnimeStatistics.Rewatched, false);

                    embed.AddField("Watching", gUser.profile.AnimeStatistics.Watching, true);
                    embed.AddField("Completed", gUser.profile.AnimeStatistics.Completed, true);
                    embed.AddField("On-Hold", gUser.profile.AnimeStatistics.OnHold, true);
                    embed.AddField("Dropped", gUser.profile.AnimeStatistics.Dropped, true);
                    embed.AddField("Plan to Watch", gUser.profile.AnimeStatistics.PlanToWatch, true);
                }
                else if(option.ToLower() == "manga")
                {
                    embed.Title = gUser.MAL_Username + " Manga Statistics";
                    embed.Fields.Clear();
                    
                    if (!string.IsNullOrWhiteSpace(mangaRank)) embed.AddField("Rank", mangaRank, false);

                    embed.AddField("Days", gUser.daysReadManga.ToString(), true);
                    embed.AddField("Mean Score", gUser.profile.MangaStatistics.MeanScore, true);
                    embed.AddField("Total Entries", gUser.profile.MangaStatistics.TotalEntries, true);
                    embed.AddField("Chapters", gUser.profile.MangaStatistics.ChaptersRead, true);
                    embed.AddField("Volumes", gUser.profile.MangaStatistics.VolumesRead, true);
                    embed.AddField("Reread", gUser.profile.MangaStatistics.Reread, true);

                    embed.AddField("Reading", gUser.profile.MangaStatistics.Reading, true);
                    embed.AddField("Completed", gUser.profile.MangaStatistics.Completed, true);
                    embed.AddField("On-Hold", gUser.profile.MangaStatistics.OnHold, true);
                    embed.AddField("Dropped", gUser.profile.MangaStatistics.Dropped, true);
                    embed.AddField("Plan to Read", gUser.profile.MangaStatistics.PlanToRead, true);
                }
                else
                {
                    embed.Title = gUser.MAL_Username + " Profile";

                    embed.AddField("Anime:", (string.IsNullOrWhiteSpace(animeRank) ? "" : "**Rank:** " + animeRank) +
                                             "\n**Days:** " + gUser.daysWatchedAnime);
                    embed.AddField("Manga:", (string.IsNullOrWhiteSpace(mangaRank) ? "" : "**Rank:** " + mangaRank) +
                                             "\n**Days:** " + gUser.daysReadManga);
                }

                await ReplyAsync("", false, embed.Build());
            }
            else
            {
                string username = user == null ? Context.User.Username : user.Username;
                await ReplyAsync(username + " has not registered a MAL account to his account.");
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
    }
}
