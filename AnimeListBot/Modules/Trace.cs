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
using AnimeListBot.Handler;
using AnimeListBot.Handler.trace.moe;
using AnimeListBot.Handler.trace.moe.Objects;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using AnimeListBot.Handler.Database;
using Discord.WebSocket;
using AnimeListBot.Handler.API.SauceNAO;
using JikanDotNet;
using AnimeListBot.Handler.Misc;

namespace AnimeListBot.Modules
{
    public class Trace : ModuleBase<ShardedCommandContext>
    {
        private IDatabaseService _db;

        public Trace(IDatabaseService db)
        {
            _db = db;
        }

        [Command("trace")]
        [Summary("Traces an image by link")]
        [RequireBotPermission(ChannelPermission.AddReactions)]
        [RequireBotPermission(ChannelPermission.UseExternalEmojis)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task TraceImage(string url, bool override_trace = false)
        {
            EmbedHandler embed = new EmbedHandler(Context.User, "Tracing Image...");
            embed.SetOwner(Context.User);

            Uri imgLink = null;
            if (!Uri.TryCreate(url, UriKind.Absolute, out imgLink))
            {
                embed.Title = "Invalid URL";
                await embed.SendMessage(Context.Channel);
                return;
            }

            embed.ThumbnailUrl = imgLink.AbsoluteUri;
            await embed.SendMessage(Context.Channel);

            SauceNao wrapper = Program._sauceNao;
            SauceHttpResult result = await wrapper.Trace(url);
           
            if(result == null)
            {
                embed.Title = "Trace failed";
                await embed.UpdateEmbed();
                await PermissionWrapper.DeleteMessage(Context.Message);
                return;
            }

            if(result.Results == null || result.Results.Length <= 0)
            {
                embed.Title = "No Traces found";
                await embed.UpdateEmbed();
                await PermissionWrapper.DeleteMessage(Context.Message);
                return;
            }

            SauceResult sauce = result.Results[0];
            SauceSourceRating rating = await sauce.GetRating();

            SocketTextChannel textChannel = (SocketTextChannel)Context.Channel;
            if (rating == SauceSourceRating.Nsfw && !textChannel.IsNsfw)
            {
                embed.Title = "The most similar media is NSFW, please use the command with the image on an NSFW channel.";
                if(sauce.Header.Similarity < 50) embed.Description = "Similarity Level is under 50% (at " + sauce.Header.Similarity + "%)., it might not be the result you want.";
                embed.ThumbnailUrl = "";
                await embed.UpdateEmbed();
                await PermissionWrapper.DeleteMessage(Context.Message);
                return;
            }

            if(sauce.Header.Similarity < 50 && !override_trace)
            {
                embed.Title = "Similarity level too low (" + sauce.Header.Similarity + "%). It must be above 50%";
                embed.Description = "Override?, Add 'true' as the last parameter.";
                await embed.UpdateEmbed();
                await PermissionWrapper.DeleteMessage(Context.Message);
                return;
            }
            string siteIndex = Enum.GetName(typeof(SauceSiteIndex), sauce.Header.IndexId);
            
            embed.Title = "";
            string fieldTitle = string.IsNullOrEmpty(sauce.Media.Title) ? "No Name" : sauce.Media.Title;
            embed.AddFieldSecure(fieldTitle,
                "Title: " + sauce.Media.Title + (string.IsNullOrEmpty(sauce.Media.Part) ? "" : " " + sauce.Media.Part) +
                "\n" + (string.IsNullOrEmpty(sauce.Media.Author) ? "" : "Author: " + sauce.Media.Author) +
                "\nSimilarity: " + sauce.Header.Similarity
            );

            List<string> mediaUrls = sauce.Media.ExternalUrl.ToList();
            embed.AddFieldSecure("Sources", string.Join("\n", sauce.Media.ExternalUrl));

            DiscordUser user = await _db.GetUserById(Context.User.Id);

            bool foundMalMangaURL = mediaUrls.Find(x => x.Contains("myanimelist.net/manga/")) != null;
            bool foundMalAnimeURL = mediaUrls.Find(x => x.Contains("myanimelist.net/anime/")) != null;

            if (foundMalMangaURL)
            {
                IEmote emote = Emote.Parse("<:mal_icon:709458138356514897>");
                embed.AddEmojiAction(emote, async () =>
                {
                    await embed.RemoveAllEmojiActions();
                    await embed.RemoveAllEmotes();
                    embed.Fields.Clear();
                    embed.Title = "Getting manga: " + sauce.Media.Title;
                    await embed.UpdateEmbed();
                    await Search.GetManga(embed, user, (long)sauce.Media.MyAnimeListId);
                });
            }
            else if (foundMalAnimeURL)
            {
                IEmote emote = Emote.Parse("<:mal_icon:709458138356514897>");
                embed.AddEmojiAction(emote, async () =>
                {
                    await embed.RemoveAllEmojiActions();
                    await embed.RemoveAllEmotes();
                    embed.Fields.Clear();
                    embed.Title = "Getting anime: " + sauce.Media.Title;
                    await embed.UpdateEmbed();
                    await Search.GetAnime(embed, user, (long)sauce.Media.MyAnimeListId);
                });
            }

            if (!string.IsNullOrEmpty(sauce.Media.Title))
            {
                IEmote tvEmote = new Emoji("📺");
                embed.AddEmojiAction(tvEmote, async () =>
                {
                    embed.Title = "Searching for " + sauce.Media.Title;
                    embed.Fields.Clear();
                    await embed.RemoveAllEmojiActions();
                    await embed.RemoveAllEmotes();
                    await embed.UpdateEmbed();
                    await Search.SearchAnime(embed, user, sauce.Media.Title);
                });

                IEmote bookEmote = new Emoji("📖");
                embed.AddEmojiAction(bookEmote, async () =>
                {
                    embed.Title = "Searching for " + sauce.Media.Title;
                    embed.Fields.Clear();
                    await embed.RemoveAllEmojiActions();
                    await embed.RemoveAllEmotes();
                    await embed.UpdateEmbed();
                    await Search.SearchManga(embed, user, sauce.Media.Title);
                });
                await PermissionWrapper.DeleteMessage(Context.Message);
            }
            await embed.UpdateEmbed();
        }

        [Command("trace")]
        [Summary("Traces an image by uploading an image as an attachment while you do this command.")]
        [RequireBotPermission(ChannelPermission.AddReactions)]
        [RequireBotPermission(ChannelPermission.UseExternalEmojis)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task TraceImage(bool override_trace = false)
        {
            var attachements = Context.Message.Attachments;
            if(attachements.Count > 0)
            {
                IAttachment attachment = attachements.ElementAt(0);
                await TraceImage(attachment.Url, override_trace);
            }
            else
            {
                EmbedHandler embed = HelpModule.GetCommandHelp("trace", Context);
                await embed.SendMessage(Context.Channel);
            }
        }

        public static string GetTraceTime(TimeSpan time)
        {
            return (time.Hours == 0 ? "" : time.Hours + ":") + time.Minutes + ":" + time.Seconds;
        }
    }
}
