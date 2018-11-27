using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using AnimeListBot.Handler;
using JikanDotNet;
using AnimeListBot.Handler.Anilist;

namespace AnimeListBot.Modules
{
    public class AutoAdder : ModuleBase<ICommandContext>
    {
        [Command("automal")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetChannel(ITextChannel channel)
        {
            DiscordServer server = DiscordServer.GetServerFromID(Context.Guild.Id);
            server.animeListChannelId = channel.Id;
            await ReplyAsync("Added " + channel.Mention + " to automal.");

            await Update();
        }

        [Command("automalupdate")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Update()
        {
            IUserMessage message = await ReplyAsync("Now updating all users ranks.");

            DiscordServer server = DiscordServer.GetServerFromID(Context.Guild.Id);
            ITextChannel channel = await Context.Guild.GetChannelAsync(server.animeListChannelId) as ITextChannel;

            IEnumerable<IMessage> messages = await channel.GetMessagesAsync(100).FlattenAsync();
            List<IMessage> listMessages = messages.ToList();

            listMessages.ForEach(async x =>
            {
                await AddUser(x);
            });

            await message.DeleteAsync();
        }

        public static async Task AddUser(IMessage message)
        {
            Uri link = null;
            bool isValidLink = Uri.TryCreate(message.Content, UriKind.Absolute, out link);
            bool containsMAL = message.Content.Contains("myanimelist.net");
            bool containsAnilist = message.Content.Contains("anilist.co");
            if (isValidLink && containsMAL)
            {
                string usernamePart = link.Segments[link.Segments.Length - 1];
                UserProfile profile = await Program._jikan.GetUserProfile(usernamePart);
                if (profile != null)
                {
                    GlobalUser user = Program.globalUsers.Find(y => y.userID == message.Author.Id);
                    if (user == null)
                    {
                        user = new GlobalUser(message.Author);
                        user.MAL_Username = usernamePart;
                        Program.globalUsers.Add(user);
                        user.toggleAnilist = false;
                        await user.UpdateMALInfo();
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(user.MAL_Username))
                        {
                            user.MAL_Username = usernamePart;
                            user.toggleAnilist = false;
                            await user.UpdateMALInfo();
                        }
                    }
                }
            }else if(isValidLink && containsAnilist)
            {
                string usernamePart = link.Segments[link.Segments.Length - 1];
                IAnilistUser profile = await UserQuery.GetUser(usernamePart);
                if (profile != null)
                {
                    GlobalUser user = Program.globalUsers.Find(y => y.userID == message.Author.Id);
                    if (user == null)
                    {
                        user = new GlobalUser(message.Author);
                        user.Anilist_Username = usernamePart;
                        user.toggleAnilist = true;
                        Program.globalUsers.Add(user);
                        await user.UpdateAnilistInfo();
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(user.Anilist_Username))
                        {
                            user.Anilist_Username = usernamePart;
                            user.toggleAnilist = true;
                            await user.UpdateAnilistInfo();
                        }
                    }
                }
            }
        }
    }
}
