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
        public static List<ulong> rolesUpdating = new List<ulong>();

        public ulong ServerId { get; set; }
        public string Prefix { get; set; } = Program.botPrefix;
        [NotMapped]

        // OBSULETE

        public ulong RegisterChannelId { get; set; } = 0;
        public long[] AnimeroleId { get; set; } = new long[0];
        public long[] MangaroleId { get; set; } = new long[0];

        public double[] AnimeroleDays { get; set; } = new double[0];
        public double[] MangaroleDays { get; set; } = new double[0];

        public DiscordServer() { }
        public DiscordServer(IGuild guild) { ServerId = guild.Id; }

        public IGuild GetGuild() { return Program._client.GetGuild(ServerId); }
        public async Task<IGuildUser> GetGuildUser(ulong userId) { return await GetGuildUser(GetGuild(), userId); }
        public static async Task<IGuildUser> GetGuildUser(IGuild guild, ulong userId) { return await guild.GetUserAsync(userId); }
        public async Task UpdateDatabase() { await DatabaseRequest.UpdateServer(this); }

        public void OverrideData(DiscordServer server)
        {
            Prefix = server.Prefix;
            RegisterChannelId = server.RegisterChannelId;
            AnimeroleId = server.AnimeroleId;
            MangaroleId = server.MangaroleId;

            AnimeroleDays = server.AnimeroleDays;
            MangaroleDays = server.MangaroleDays;
        }
    }
}