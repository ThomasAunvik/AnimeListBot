using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler
{
    public class Cluster
    {
        public int Id { get; set; }
        public int ShardIdStart { get; set; }
        public int ShardIdEnd { get; set; }

        public int GetShardAmount()
        {
            return (ShardIdEnd - ShardIdStart) + 1;
        }

        public int[] GetShardIds()
        {
            int indexAmount = GetShardAmount();
            int[] shardIds = new int[indexAmount];
            for(int i = 0; i < indexAmount; i++)
            {
                shardIds[i] = ShardIdStart + i;
            }
            return shardIds;
        }
    }
}
