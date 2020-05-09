using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler
{
    public class RoleRank
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public double Days { get; set; }

        public ulong RawGuildPermissionsValue { get; set; }

        public void UpdateRank(IGuild guild)
        {
            IRole role = guild.GetRole(Id);
            Name = role.Name;
            RawGuildPermissionsValue = role.Permissions.RawValue;
        }
    }
}
