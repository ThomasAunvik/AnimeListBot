﻿using Discord;
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
        [Command("autolist")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task SetChannel(ITextChannel channel)
        {
            DiscordServer server = DiscordServer.GetServerFromID(Context.Guild.Id);
            server.animeListChannelId = channel.Id;

            EmbedHandler embed = new EmbedHandler(Context.User, "Set auto anime list channel to <# " + channel.Id + ">...");
            await embed.SendMessage(Context.Channel);
            await Update();
        }

        [Command("autolistupdate")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task Update()
        {
            DiscordServer server = DiscordServer.GetServerFromID(Context.Guild.Id);
            ITextChannel channel = await Context.Guild.GetChannelAsync(server.animeListChannelId) as ITextChannel;

            EmbedHandler embed = new EmbedHandler(Context.User, "Updating Anime Lists from <# " + channel.Id + ">...");
            await embed.SendMessage(Context.Channel);

            IEnumerable<IMessage> messages = await channel.GetMessagesAsync().FlattenAsync();
            List<IMessage> listMessages = messages.ToList();

            listMessages.ForEach(async x =>
            {
                await AddUser(x);
            });

            embed.Title = "Anime Lists Updated.";
            await embed.UpdateEmbed();
            server.SaveData();
        }

        [Command("autolistchannel")]
        public async Task GetAutoMalChannel()
        {
            DiscordServer server = DiscordServer.GetServerFromID(Context.Guild.Id);
            EmbedHandler embed = new EmbedHandler(Context.User);
            if (server.animeListChannelId != 0)
            {
                embed.Title = "Auto AnimeList is set to channel: <#" + server.animeListChannelId + ">";
            }
            else
            {
                embed.Title = "AutoMAL channel is not set.";
            }
            await embed.SendMessage(Context.Channel);
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
                    Console.WriteLine("Invalid Link: " + message.Content);
                    return;
                }

                if (isValidLink && containsMAL && string.IsNullOrWhiteSpace(user.MAL_Username))
                {
                    UserProfile profile = await Program._jikan.GetUserProfile(usernamePart);
                    if (profile != null)
                    {
                        user.MAL_Username = usernamePart;
                        user.animeList = GlobalUser.AnimeList.MAL;
                        await user.UpdateMALInfo();
                    }
                }
                else if (isValidLink && containsAnilist && string.IsNullOrWhiteSpace(user?.Anilist_Username))
                {
                    IAniUser profile = await AniUserQuery.GetUser(usernamePart);
                    if (profile != null)
                    {
                        user.Anilist_Username = usernamePart;
                        user.animeList = GlobalUser.AnimeList.Anilist;
                        await user.UpdateAnilistInfo();
                    }
                }
            }catch(Exception e)
            {
                await Program._logger.LogError(e);
            }
        }
    }
}
