using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler.trace.moe.Objects
{
    [Serializable]
    public class TraceDoc
    {
        public decimal? from { get; set; }
        public decimal? to { get; set; }
        public int? anilist_id { get; set; }
        public decimal? at { get; set; }
        public string season { get; set; }
        public string anime { get; set; }
        public string filename { get; set; }
        public string episode { get; set; }
        public string tokenthumb { get; set; }
        public decimal? similarity { get; set; }
        public string title { get; set; }
        public string title_native { get; set; }
        public string title_chinese { get; set; }
        public string title_english { get; set; }
        public string title_romanji { get; set; }
        public string mal_id { get; set; }
        public List<string> synonyms { get; set; }
        public List<string> synonyms_chinese { get; set; }
        public bool? is_adult { get; set; }
    }
}
