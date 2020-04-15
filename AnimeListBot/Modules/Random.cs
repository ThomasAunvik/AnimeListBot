using AnimeListBot.Handler;
using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AnimeListBot.Modules
{
    public class Random : ModuleBase<ShardedCommandContext>
    {
        class ListRequest
        {
            public List<long> sfw = new List<long>();
        }

        const string AnimeCache = "https://raw.githubusercontent.com/seanbreckenridge/mal-id-cache/master/cache/anime_cache.json";
        const string MangaCache = "https://raw.githubusercontent.com/seanbreckenridge/mal-id-cache/master/cache/manga_cache.json";

        [Command("randomanime")]
        public async Task RandomAnime()
        {
            EmbedHandler embed = new EmbedHandler(Context.User, "Getting Random Anime...");
            await embed.SendMessage(Context.Channel);

            HttpClient client = new HttpClient();
            string json = await client.GetStringAsync(AnimeCache);
            ListRequest ids = JsonConvert.DeserializeObject<ListRequest>(json);

            System.Random rnd = new System.Random();
            int rndNumber = rnd.Next(0, ids.sfw.Count);

            await Search.GetAnime(embed, Context.User, ids.sfw[rndNumber]);
        }

        [Command("randommanga")]
        public async Task RandomManga()
        {
            EmbedHandler embed = new EmbedHandler(Context.User, "Getting Random Manga...");
            await embed.SendMessage(Context.Channel);

            HttpClient client = new HttpClient();
            string json = await client.GetStringAsync(MangaCache);
            ListRequest ids = JsonConvert.DeserializeObject<ListRequest>(json);

            System.Random rnd = new System.Random();
            int rndNumber = rnd.Next(0, ids.sfw.Count);

            await Search.GetManga(embed, Context.User, ids.sfw[rndNumber]);
        }
    }
}
