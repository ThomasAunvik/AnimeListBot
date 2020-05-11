using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler.API.SauceNAO
{
    public class SauceHeader
    {
        [JsonProperty("user_id")]
        public int UserId { get; set; }
        [JsonProperty("account_type")]
        public int AccountType { get; set; }
        [JsonProperty("short_limit")]
        public int ShortLimit { get; set; }
        [JsonProperty("long_limit")]
        public int LongLimit { get; set; }
        [JsonProperty("long_remaining")]
        public int LongRemaining { get; set; }
        [JsonProperty("short_remaining")]
        public int ShortRemaining { get; set; }
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("index")]
        public Dictionary<SauceSiteIndex, SauceHeaderIndex> Index { get; set; }

        [JsonProperty("search_depth")]
        public int SearchDepth { get; set; }
        [JsonProperty("minimum_similarity")]
        public double MinimumSimilarity { get; set; }
        [JsonProperty("query_image_display")]
        public string QueryImageDisplay { get; set; }
        [JsonProperty("query_image")]
        public string QueryImage { get; set; }
        [JsonProperty("results_returned")]
        public int ResultsReturned { get; set; }
    }
}
