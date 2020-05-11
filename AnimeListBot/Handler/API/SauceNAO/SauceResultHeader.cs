using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler.API.SauceNAO
{
    public class SauceResultHeader
    {
        [JsonProperty("similarity")]
        public double Similarity { get; set; }
        [JsonProperty("thumbnail")]
        public string Thumbnail { get; set; }
        [JsonProperty("index_id")]
        public SauceSiteIndex IndexId { get; set; }
        [JsonProperty("index_name")]
        public string IndexName { get; set; }
    }
}
