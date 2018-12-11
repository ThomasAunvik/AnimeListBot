using AnimeListBot.Handler;
using AnimeListBot.Handler.trace.moe;
using AnimeListBot.Handler.trace.moe.Objects;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AnimeListBot.Modules
{
    public class Trace : ModuleBase<ICommandContext>
    {
        [Command("trace")]
        public async Task TraceImage(string url)
        {
            EmbedHandler embed = new EmbedHandler(Context.User, "Tracing Image...");

            Uri imgLink = null;
            if (!Uri.TryCreate(url, UriKind.Absolute, out imgLink))
            {
                embed.Title = "URL is invalid.";
                await embed.SendMessage(Context.Channel);
                return;
            }

            embed.ThumbnailUrl = imgLink.AbsoluteUri;
            await embed.SendMessage(Context.Channel);

            await PermissionWrapper.DeleteMessage(Context.Message);

            (bool, string, ITraceResult) response = await TraceMoe.Search(imgLink);
            if (response.Item1)
            {
                ITraceResult result = response.Item3;
                if(result == null)
                {
                    embed.Title = "Failed to load.";
                    await embed.UpdateEmbed();
                    return;
                }

                if(result.docs.Count > 0)
                {
                    TraceDoc doc = result.docs[0];

                    decimal atValue = doc.at.GetValueOrDefault();
                    string atString = string.Empty;
                    if(atValue != 0)
                    {
                        atString = decimal.Round(atValue / (decimal)60, 2).ToString().Replace(",", ":");
                    }

                    decimal toValue = doc.to.GetValueOrDefault();
                    string toString = string.Empty;
                    if (atValue != 0)
                    {
                        toString = decimal.Round(toValue / (decimal)60, 2).ToString().Replace(",", ":");
                    }

                    embed.Title = "";
                    embed.AddField(
                        doc.title_english,

                        "Native Title: " + doc.title_native + "\n" +

                        "Episode: " + doc.episode + "\n" +
                        "At: " + atString + "\n" +
                        (atString == toString ? "" :("To: " + toString + "\n")) +
                        "Similarity: " + doc.similarity + "\n" +
                        
                        "MAL Id: " + doc.mal_id + "\n" +
                        "Anilist Id: " + doc.anilist_id + "\n"
                    );
                }
            }
            else
            {
                embed.Title = response.Item2;
            }
            await embed.UpdateEmbed();
        }
    }
}
