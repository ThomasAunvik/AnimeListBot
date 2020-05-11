using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler.API.SauceNAO
{
    public class SauceHeaderIndex
    {
        [JsonProperty("status")]
        public int Status { get; set; }
        [JsonProperty("parent_id")]
        public int ParentId { get; set; }
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("results")]
        public int Results { get; set; }
    }
}
