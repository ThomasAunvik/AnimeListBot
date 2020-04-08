using System;
using System.Collections.Generic;
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

        public ulong id;
        public ulong animeListChannelId = 0;

        public List<ulong> animeRoleIds = new List<ulong>();
        public List<ulong> mangaRoleIds = new List<ulong>();

        public List<double> animeRoleDays = new List<double>();
        public List<double> mangaRoleDays = new List<double>();

        public DiscordServer() { }
        public DiscordServer(IGuild guild) { id = guild.Id; }

        public IGuild GetGuild() { return Program._client.GetGuild(id); }
        public async Task<IGuildUser> GetGuildUser(ulong userId) { return await GetGuildUser(GetGuild(), userId); }
        public static async Task<IGuildUser> GetGuildUser(IGuild guild, ulong userId) { return await guild.GetUserAsync(userId); }
        public async Task UpdateDatabase() { await DatabaseRequest.UpdateServer(this); }
    }
}