using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler
{
    public class ServerRanks
    {
        public ulong RegisterChannelId { get; set; } = 0;
        public List<long> AnimeroleId { get; set; } = new List<long>();
        public List<long> MangaroleId { get; set; } = new List<long>();

        public List<double> AnimeroleDays { get; set; } = new List<double>();
        public List<double> MangaroleDays { get; set; } = new List<double>();

        public List<string> AnimeroleNames { get; set; } = new List<string>();
        public List<string> MangaroleNames { get; set; } = new List<string>();
    }
}
