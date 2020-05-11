using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AnimeListBot.Handler.API.SauceNAO
{
    public class SauceResult
    {
        [JsonProperty("header")]
        public SauceResultHeader Header { get; set; }
        [JsonProperty("data")]
        public SauceMedia Media { get; set; }

        public async Task<SauceSourceRating> GetRating(){
            return await SauceNao.GetSourceRating(this);
        }
    }
}
