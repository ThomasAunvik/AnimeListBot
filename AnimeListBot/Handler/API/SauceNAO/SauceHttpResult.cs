using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler.API.SauceNAO
{
    public class SauceHttpResult
    {
        [JsonProperty("header")]
        public SauceHeader Header { get; set; }
        [JsonProperty("results")]
        public SauceResult[] Results { get; set; }
    }
}
