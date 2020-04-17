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
    public class AutoAdder : ModuleBase<ShardedCommandContext>
    {
        [Command("autolist")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task SetChannel(ITextChannel channel)
        {
            DiscordServer server = DatabaseRequest.GetServerById(Context.Guild.Id);
            server.RegisterChannelId = channel.Id;
            await server.UpdateDatabase();

            EmbedHandler embed = new EmbedHandler(Context.User, "Set auto anime list channel to:", "<#" + channel.Id + ">...");
            await embed.SendMessage(Context.Channel);
            await Update();
        }

        [Command("autolistupdate")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task Update()
        {
            DiscordServer server = DatabaseRequest.GetServerById(Context.Guild.Id);
            ITextChannel channel = Context.Guild.GetTextChannel(server.RegisterChannelId) as ITextChannel;

            EmbedHandler embed = new EmbedHandler(Context.User, "Updating Anime Lists from channel", "<#" + channel.Id + ">");
            await embed.SendMessage(Context.Channel);

            IEnumerable<IMessage> messages = await channel.GetMessagesAsync().FlattenAsync();
            List<IMessage> listMessages = messages.ToList();
            if (listMessages.Count > 0)
            {
                listMessages.ForEach(async x =>
                {
                    await AddUser(x, server);
                });
            }

            embed.Title = "Anime Lists Updated.";
            await embed.UpdateEmbed();
        }

        [Command("autolistchannel")]
        public async Task GetAutoMalChannel()
        {
            DiscordServer server = DatabaseRequest.GetServerById(Context.Guild.Id);
            EmbedHandler embed = new EmbedHandler(Context.User);
            if (server.RegisterChannelId != 0)
            {
                embed.Title = "Auto AnimeList is set to channel";
                embed.Description = "<#" + server.RegisterChannelId + ">";
            }
            else
            {
                embed.Title = "AutoMAL channel is not set.";
            }
            await embed.SendMessage(Context.Channel);
        }

        public static async Task AddUser(IMessage message, DiscordServer server)
        {
            if (string.IsNullOrWhiteSpace(message.Content)) return;

            try
            {
                DiscordUser user = await DatabaseRequest.GetUserById(message.Author.Id);
                if(user == null)
                {
                    user = new DiscordUser(message.Author);
                    await user.CreateUserDatabase();
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
                    containsMAL = link.ToString().Contains("myanimelist.net");
                    containsAnilist = link.ToString().Contains("anilist.co");
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
                    string returnMessage = "Invalid Profile Link: " + message.Content;
                    Console.WriteLine(returnMessage);
                    await message.Author.SendMessageAsync(returnMessage +
                        "Channel: <#" + message.Channel.Id + ">" +
                        "\nPlease use the following link formats:\n" +
                        "MAL: `https://myanimelist.net/profile/[username]` \n" +
                        "Anilist: `https://anilist.co/user/[username]`");
                    await message.DeleteAsync();
                    return;
                }

                if (isValidLink && containsMAL && string.IsNullOrWhiteSpace(user?.malProfile?.Username))
                {
                    UserProfile profile = await Program._jikan.GetUserProfile(usernamePart);
                    if (profile != null)
                    {
                        user.ListPreference = DiscordUser.AnimeList.MAL;
                        await user.UpdateMALInfo(usernamePart);
                    }
                }
                else if (isValidLink && containsAnilist && string.IsNullOrWhiteSpace(user?.anilistProfile?.name))
                {
                    IAniUser profile = await AniUserQuery.GetUser(usernamePart);
                    if (profile != null)
                    {
                        user.ListPreference = DiscordUser.AnimeList.Anilist;
                        await user.UpdateAnilistInfo(usernamePart);
                    }
                }

                await user.UpdateDatabase();

                await Ranks.UpdateUserRole(server, user, null);
            }
            catch(Exception e)
            {
                await Program._logger.LogError(e, message.Author, (IGuildChannel)message.Channel);
            }
        }
    }
}
