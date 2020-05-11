using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler.API.SauceNAO
{
    public class SauceMedia
    {
        [JsonProperty("ext_urls")]
        public string[] ExternalUrl { get; set; }

        [JsonProperty("source")]
        public string Title { get; set; }

        [JsonProperty("anidb_aid")]
        public int? AnidbId { get; set; }
        [JsonProperty("pixiv_id")]
        public int? PixivId { get; set; }
        [JsonProperty("md_id")]
        public int? MangaDexId { get; set; }
        [JsonProperty("mu_id")]
        public int? MangaUpdatesId { get; set; }
        [JsonProperty("mal_id")]
        public int? MyAnimeListId { get; set; }

        [JsonProperty("part")]
        public string Part { get; set; }
        [JsonProperty("year")]
        public string Year{ get; set; }
        [JsonProperty("est_time")]
        public string EstTime { get; set; }

        [JsonProperty("artist")]
        public string Artist { get; set; }
        [JsonProperty("author")]
        public string Author { get; set; }
    }
}
