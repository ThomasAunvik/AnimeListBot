using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler
{
    public class RoleRank
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double Days { get; set; }

        public ulong RawGuildPermissionsValue { get; set; }

        public void UpdateRank(IGuild guild)
        {
            ulong idResult;
            if (ulong.TryParse(Id, out idResult))
            {
                IRole role = guild.GetRole(idResult);
                if (role == null) return;

                Name = role.Name;
                RawGuildPermissionsValue = role.Permissions.RawValue;
            }
        }

        public ulong GetRoleID()
        {
            ulong idResult;
            if (ulong.TryParse(Id, out idResult))
            {
                return idResult;
            }
            return 0;
        }
    }
}
