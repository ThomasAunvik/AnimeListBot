using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using AnimeListBot.Handler;
using JikanDotNet;
using AnimeListBot.Handler.Anilist;

namespace AnimeListBot.Modules
{
    public class AutoAdder : ModuleBase<ICommandContext>
    {
        [Command("automal")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task SetChannel(ITextChannel channel)
        {
            DiscordServer server = DiscordServer.GetServerFromID(Context.Guild.Id);
            server.animeListChannelId = channel.Id;
            await ReplyAsync("Added " + channel.Mention + " to automal.");
            server.SaveData();
            await Update();
        }

        [Command("automalupdate")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
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

        [Command("automalchannel")]
        public async Task GetAutoMalChannel()
        {
            DiscordServer server = DiscordServer.GetServerFromID(Context.Guild.Id);
            if (server.animeListChannelId != 0)
            {
                await ReplyAsync("AutoMAL is set to channel: <#" + server.animeListChannelId + ">");
            }
            else
            {
                await ReplyAsync("AutoMAL channel is not set.");
            }
        }

        public static async Task AddUser(IMessage message)
        {
            if (string.IsNullOrWhiteSpace(message.Content)) return;

            try
            {
                GlobalUser user = Program.globalUsers.Find(y => y.userID == message.Author.Id);
                if(user == null)
                {
                    user = new GlobalUser(message.Author);
                    Program.globalUsers.Add(user);
                }

                string currentLink = message.Content;

                Uri link = null;
                bool isValidLink = Uri.TryCreate(currentLink, UriKind.Absolute, out link);
                if (!isValidLink)
                {
                    Match rxMatch = Regex.Match(message.Content, @"#\bhttps?://[^,\s()<>]+(?:\([\w\d]+\)|([^,[:punct:]\s]|/))#");
                    if (rxMatch.Success && rxMatch.Length > 0)
                    {
                        currentLink = rxMatch.Captures[0].Value;
                        isValidLink = Uri.TryCreate(currentLink, UriKind.Absolute, out link);
                    }
                }

                bool containsMAL = false;
                bool containsAnilist = false;

                if (isValidLink)
                {
                    containsMAL = link.AbsolutePath.Contains("myanimelist.net");
                    containsAnilist = link.AbsolutePath.Contains("anilist.co");
                }

                string usernamePart = string.Empty;
                if(link != null)
                {
                    usernamePart = link.Segments[link.Segments.Length - 1];
                    if (usernamePart[usernamePart.Length - 1] == '/' || usernamePart[usernamePart.Length - 1] == '\\')
                    {
                        usernamePart = usernamePart.Substring(0, usernamePart.Length - 1);
                    }
                }
                else
                {
                    await Program._logger.LogError("Invalid Link: \n    Message: " + message.Content + "\n    Link: " + currentLink);
                    return;
                }

                if (isValidLink && containsMAL && string.IsNullOrWhiteSpace(user.MAL_Username))
                {
                    UserProfile profile = await Program._jikan.GetUserProfile(usernamePart);
                    if (profile != null)
                    {
                        user.MAL_Username = usernamePart;
                        user.toggleAnilist = false;
                        await user.UpdateMALInfo();
                    }
                }
                else if (isValidLink && containsAnilist && string.IsNullOrWhiteSpace(user?.Anilist_Username))
                {
                    IAnilistUser profile = await UserQuery.GetUser(usernamePart);
                    if (profile != null)
                    {
                        user.Anilist_Username = usernamePart;
                        user.toggleAnilist = true;
                        await user.UpdateAnilistInfo();
                    }
                }
            }catch(Exception e)
            {
                await Program._logger.LogError("Message: " + message.Content + "\n" + e);
            }
        }
    }
}
