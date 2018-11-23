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
            UserProfile profile = await Program._jikan.GetUserProfile(username);
            if(profile == null)
            {
                await ReplyAsync("Invalid Username.");
                return;
            }

            if(option == "mal" || option == "myanimelist")
            {
                GlobalUser user = Program.globalUsers.Find(x => x.UserID == Context.User.Id);
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
                user.SaveData();
            }
        }

        [Command("profile")]
        public async Task GetProfile(IUser user = null)
        {
            GlobalUser gUser = null;
            ulong targetId = user == null ? Context.User.Id : user.Id;
            gUser = Program.globalUsers.Find(x => x.UserID == targetId);

            if (gUser != null && !string.IsNullOrWhiteSpace(gUser.MAL_Username))
            {
                await gUser.UpdateMALInfo();

                EmbedBuilder embed = new EmbedBuilder()
                {
                    Title = "MAL Profile",
                    Description = "",
                    ImageUrl = gUser.imageURL,

                    Fields = new List<EmbedFieldBuilder>()
                    {
                        new EmbedFieldBuilder()
                        {
                            Name = "Anime",
                            Value = "Days of anime watched: " + gUser.daysWatchedAnime
                        }
                    },

                    Color = Program.embedColor
                };
                await ReplyAsync("", false, embed.Build());
            }
            else
            {
                string username = user == null ? Context.User.Username : user.Username;
                await ReplyAsync(username + " has not registered a MAL account to his account.");
            }
        }
    }
}
