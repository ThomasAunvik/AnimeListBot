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
        public string ServerId { get; set; }
        [JsonProperty("roles")]
        public List<ulong> Roles { get; set; }

        [JsonProperty("admin")]
        public bool Admin { get; set; }
        [JsonProperty("manageroles")]
        public bool ManageRoles { get; set; }

        public GuildUserInfo() { }

        public GuildUserInfo(IGuildUser guildUser)
        {
            ServerId = guildUser.GuildId.ToString();
            Roles = guildUser.RoleIds.ToList();

            Admin = guildUser.GuildPermissions.Administrator;
            ManageRoles = guildUser.GuildPermissions.ManageRoles;
        }

        public void UpdateUserInfo(IGuildUser guildUser)
        {
            Roles = guildUser.RoleIds.ToList();

            Admin = guildUser.GuildPermissions.Administrator;
            ManageRoles = guildUser.GuildPermissions.ManageRoles;
        }
    }
}
