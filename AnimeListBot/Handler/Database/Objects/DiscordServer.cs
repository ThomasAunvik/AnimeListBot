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
using System.Linq;
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
        public string name { get; set; }
        public string icon { get; set; }

        public string Prefix { get; set; } = Program.botPrefix;

        public ServerStatistics stats { get; set; } = new ServerStatistics();
        public ServerRanks ranks { get; set; } = new ServerRanks();

        public DiscordServer() { }
        public DiscordServer(IGuild guild) { 
            ServerId = guild.Id;
            UpdateGuildInfo(guild);
        }

        public void UpdateGuildInfo(IGuild guild)
        {
            name = guild.Name;
            icon = guild.IconUrl;
        }

        public void UpdateGuildRoles()
        {
            IGuild guild = GetGuild();
            List<RoleRank> roles = guild.Roles.Select(x => new RoleRank() { Id = x.Id.ToString(), Name = x.Name, RawGuildPermissionsValue = x.Permissions.RawValue }).ToList();
            roles.RemoveAll(x =>
                ranks.AnimeRanks.Find(y => x.Id == y.Id) != null ||
                ranks.MangaRanks.Find(y => x.Id == y.Id) != null
                );
            roles.RemoveAll(x => x.Id == guild.EveryoneRole.Id.ToString());
            ranks.NotSetRanks = roles;
        }

        public IGuild GetGuild() { 
            IGuild guild = Program._client.GetGuild(ServerId);
            UpdateGuildInfo(guild);
            return guild;
        }
        public async Task<IGuildUser> GetGuildUser(ulong userId) { return await GetGuildUser(GetGuild(), userId); }
        public static async Task<IGuildUser> GetGuildUser(IGuild guild, ulong userId) { return await guild.GetUserAsync(userId); }
   
    }
}