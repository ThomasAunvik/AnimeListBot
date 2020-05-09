using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnimeListBot.Handler
{
    public class GuildUserInfo
    {
        [JsonProperty("server_id")]
        public ulong ServerId { get; set; }
        [JsonProperty("roles")]
        public List<ulong> roles;

        public GuildUserInfo() { }

        public GuildUserInfo(IGuildUser guildUser)
        {
            ServerId = guildUser.GuildId;
            roles = guildUser.RoleIds.ToList();
        }
    }
}
