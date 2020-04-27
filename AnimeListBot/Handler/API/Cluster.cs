/*
 * This file is part of AnimeList Bot
 *
 * AnimeList Bot is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * AnimeList Bot is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with AnimeList Bot.  If not, see <https://www.gnu.org/licenses/>
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace AnimeListBot.Handler
{
    public class Cluster
    {
        public int Id { get; set; }
        public int ShardIdStart { get; set; }
        public int ShardIdEnd { get; set; }

        public static async Task<int> GetTotalShards()
        {
            if (Config.cached.override_shard_amount > 0) return Config.cached.override_shard_amount;

            int total = 0;
            await DatabaseConnection.db.Cluster.ForEachAsync(x => total += x.GetShardCount());
            return total;
        }

        public int GetShardCount()
        {
            if (Config.cached.override_shard_amount > 0) return Config.cached.override_shard_amount;
            return ShardIdEnd - ShardIdStart;
        }

        public int[] GetShardIds()
        {
            int indexAmount = GetShardCount();
            int[] shardIds = new int[indexAmount];
            for(int i = 0; i < indexAmount; i++)
            {
                shardIds[i] = ShardIdStart + i;
            }
            return shardIds;
        }
    }
}
