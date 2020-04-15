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

        public ulong ServerId { get; set; }
        public string Prefix { get; set; } = Program.botPrefix;
        public ulong RegisterChannelId { get; set; } = 0;

        public long[] AnimeroleId { get; set; }
        public long[] MangaroleId { get; set; }

        public double[] AnimeroleDays { get; set; }
        public double[] MangaroleDays { get; set; }

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