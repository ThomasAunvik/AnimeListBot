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

namespace AnimeListBot.Modules
{
    public class Trace : ModuleBase<ShardedCommandContext>
    {
        [Command("trace")]
        [Summary("Traces an image by link")]
        public async Task TraceImage(string url)
        {
            EmbedHandler embed = new EmbedHandler(Context.User, "Tracing Image...");

            Uri imgLink = null;
            if (!Uri.TryCreate(url, UriKind.Absolute, out imgLink))
            {
                embed.Title = "Invalid URL";
                await embed.SendMessage(Context.Channel);
                return;
            }

            embed.ThumbnailUrl = imgLink.AbsoluteUri;
            await embed.SendMessage(Context.Channel);

            ITraceResult response = await TraceMoe.Search(imgLink);
            if (!response.failed)
            {
                ITraceImage trace = response.trace;
                List<TraceDoc> validDocs = trace.docs.Where(x => x.similarity > (decimal)0.8).ToList();

                if(validDocs.Count > 0)
                {
                    TraceDoc doc = validDocs[0];

                    double atValue = Math.Round((double)doc.at.GetValueOrDefault());
                    double toValue = Math.Round((double)doc.to.GetValueOrDefault());

                    TimeSpan atTime = TimeSpan.FromSeconds(atValue);
                    TimeSpan toTime = TimeSpan.FromSeconds(toValue);

                    embed.Title = "";
                    embed.AddFieldSecure(
                        doc.title_english,

                        "Native Title: " + doc.title_native + "\n" +

                        "Episode: " + doc.episode + "\n" +
                        "At: " + GetTraceTime(atTime) + "\n" +
                        (atTime.TotalSeconds == toTime.TotalSeconds ? "" :("To: " + GetTraceTime(atTime) + "\n")) +
                        "Similarity: " + (doc.similarity.GetValueOrDefault() * 100).ToString("N2") + "%\n" +
                        
                        "MAL Id: " + doc.mal_id + "\n" +
                        "Anilist Id: " + doc.anilist_id + "\n"
                    );
                }
                else
                {
                    embed.Title = "None Found";
                }
            }
            else
            {
                embed.Title = response.errorMessage;
                embed.Description = response.errorDescription;
            }
            await embed.UpdateEmbed();
            await PermissionWrapper.DeleteMessage(Context.Message);
        }

        [Command("trace")]
        [Summary("Traces an image by uploading an image as an attachment while you do this command.")]
        public async Task TraceImage()
        {
            var attachements = Context.Message.Attachments;
            if(attachements.Count > 0)
            {
                IAttachment attachment = attachements.ElementAt(0);
                await TraceImage(attachment.Url);
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
