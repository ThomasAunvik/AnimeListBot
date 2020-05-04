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
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AnimeListBot.Handler
{
    public class DiscordServer
    {
        [NotMapped]
        public static List<ulong> rolesUpdating = new List<ulong>();

        public ulong ServerId { get; set; }
        public string Prefix { get; set; } = Program.botPrefix;
        // OBSULETE

        [Obsolete("Moved to ranks")] public ulong RegisterChannelId { get; set; } = 0;
        [Obsolete("Moved to ranks")] public List<long> AnimeroleId { get; set; } = new List<long>();
        [Obsolete("Moved to ranks")] public List<long> MangaroleId { get; set; } = new List<long>();

        [Obsolete("Moved to ranks")] public List<double> AnimeroleDays { get; set; } = new List<double>();
        [Obsolete("Moved to ranks")] public List<double> MangaroleDays { get; set; } = new List<double>();

        [Obsolete("Moved to ranks")] public List<string> AnimeroleNames { get; set; } = new List<string>();
        [Obsolete("Moved to ranks")] public List<string> MangaroleNames { get; set; } = new List<string>();

        public ServerStatistics server_statistics { get; set; }
        public ServerRanks server_ranks { get; set; }


        public DiscordServer() { }
        public DiscordServer(IGuild guild) { ServerId = guild.Id; }

        public IGuild GetGuild() { return Program._client.GetGuild(ServerId); }
        public async Task<IGuildUser> GetGuildUser(ulong userId) { return await GetGuildUser(GetGuild(), userId); }
        public static async Task<IGuildUser> GetGuildUser(IGuild guild, ulong userId) { return await guild.GetUserAsync(userId); }
   
    }
}