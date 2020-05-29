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
using Discord.WebSocket;
using AnimeListBot.Handler.Database;
using JikanDotNet.Exceptions;

namespace AnimeListBot.Modules
{
    public class AutoAdder : ModuleBase<ShardedCommandContext>
    {
        private IDatabaseService _db;

        public AutoAdder(IDatabaseService db)
        {
            _db = db;
        }

        [Command("autolist")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task SetChannel(ITextChannel channel)
        {
            DiscordServer server = await _db.GetServerById(Context.Guild.Id);
            server.ranks.RegisterChannelId = channel.Id.ToString();
            await _db.SaveChangesAsync();

            EmbedHandler embed = new EmbedHandler(Context.User, "Set auto anime list channel to:", "<#" + channel.Id + ">...");
            await embed.SendMessage(Context.Channel);
        }

        [Command("autolistremove")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task RemoveChannel()
        {
            DiscordServer server = await _db.GetServerById(Context.Guild.Id);
            EmbedHandler embed = new EmbedHandler(Context.User, "Removed auto anime list channel from:", "<#" + server.ranks.RegisterChannelId + ">...");

            if (server.ranks.RegisterChannelId == "")
            {
                embed.Title = "There is no set channel for auto-list.";
            }

            server.ranks.RegisterChannelId = "";
            await _db.SaveChangesAsync();

            await embed.SendMessage(Context.Channel);
        }

        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Command("autolistupdate")]
        public async Task AutoListUpdate()
        {
            DiscordServer server = await _db.GetServerById(Context.Guild.Id);
            ulong channelId;
            if(!ulong.TryParse(server.ranks.RegisterChannelId, out channelId)) return;

            ITextChannel channel = Context.Guild.GetTextChannel(channelId) as ITextChannel;
            if (channel == null)
            {
                EmbedHandler errorEmbed = new EmbedHandler(Context.User, "No Auto List Channel Set, use command `autolist` first.");
                await errorEmbed.SendMessage(Context.Channel);
                return;
            }

            EmbedHandler embed = new EmbedHandler(Context.User, "Updating Anime Lists from channel", "<#" + channel.Id + ">");
            await embed.SendMessage(Context.Channel);

            IEnumerable<IMessage> messages = await channel.GetMessagesAsync().FlattenAsync();
            List<IMessage> listMessages = messages.ToList();
            if (listMessages.Count > 0)
            {
                int messageCount = listMessages.Count;
                int fieldIndex = embed.AddFieldSecure("Progress", "0%");
                await embed.UpdateEmbed();
                for (int messageIndex = 0; messageIndex < messageCount; messageIndex++)
                {
                    DiscordUser user = await _db.GetUserById(listMessages[messageIndex].Author.Id);
                    await AddUser(listMessages[messageIndex], user, server, true);
                    int progressValue = (int)((messageIndex / (float)messageCount) * 100);
                    embed.Fields[fieldIndex].Value = progressValue.ToString() + "%";

                    if(progressValue % 5 == 0)
                    {
                        await embed.UpdateEmbed();
                    }
                }

                embed.Fields[fieldIndex].Value = "100%";
            }

            embed.Title = "Anime Lists Updated.";
            await embed.UpdateEmbed();
        }

        [RequireOwner]
        [Command("autolistupdate")]
        public async Task AutoListUpdateOwner()
        {
            await AutoListUpdate();
        }

        [Command("autolistchannel")]
        public async Task GetAutoMalChannel()
        {
            DiscordServer server = await _db.GetServerById(Context.Guild.Id);
            EmbedHandler embed = new EmbedHandler(Context.User);
            if (server.ranks.RegisterChannelId != "")
            {
                embed.Title = "Auto AnimeList is set to channel";
                embed.Description = "<#" + server.ranks.RegisterChannelId + ">";
            }
            else
            {
                embed.Title = "AutoMAL channel is not set.";
            }
            await embed.SendMessage(Context.Channel);
        }

        public static async Task AddUser(IMessage message, DiscordUser user, DiscordServer server, bool ignoreMessage = false)
        {
            if (string.IsNullOrWhiteSpace(message.Content)) return;

            try
            {
                if (!(message is SocketUserMessage))
                    return;
                if (message.Source != MessageSource.User)
                    return;

                user.RefreshMutualGuilds();

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
                else if(!ignoreMessage)
                {
                    IDMChannel dmChannel = await message.Author.GetOrCreateDMChannelAsync();
                    string returnMessage = "Invalid Profile Link: " + message.Content;
                    await dmChannel.SendMessageAsync(returnMessage +
                        "\nChannel: <#" + message.Channel.Id + ">" +
                        "\nPlease use the following link formats:\n" +
                        "MAL: `https://myanimelist.net/profile/[username]` \n" +
                        "Anilist: `https://anilist.co/user/[username]`");
                    await message.DeleteAsync();
                    return;
                }

                if (isValidLink && containsMAL && string.IsNullOrWhiteSpace(user?.malProfile?.Username))
                {
                    try
                    {
                        UserProfile profile = await Program._jikan.GetUserProfile(usernamePart);
                        if (profile != null)
                        {
                            user.ListPreference = AnimeListPreference.MAL;
                            await user.UpdateMALInfo(usernamePart);
                        }
                    }
                    catch (JikanRequestException) { }
                }
                else if (isValidLink && containsAnilist && string.IsNullOrWhiteSpace(user?.anilistProfile?.name))
                {
                    IAniUser profile = await AniUserQuery.GetUser(usernamePart);
                    if (profile != null)
                    {
                        user.ListPreference = AnimeListPreference.Anilist;
                        await user.UpdateAnilistInfo(usernamePart);
                    }
                }

                await Ranks.UpdateUserRole(server, user, null);
            }
            catch(Exception e)
            {
                await Program._logger.LogError(e, message.Author, (IGuildChannel)message.Channel);
            }
        }
    }
}
