using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using MALBot.Handler;

namespace MALBot.Modules
{
    public class Ranks : ModuleBase<ICommandContext>
    {
        [Command("addrank")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task AddRank(IRole role, decimal days)
        {
            DiscordServer server = DiscordServer.GetServerFromID(Context.Guild.Id);
            server.animeRoles.Add((role.Id, days));
            await UpdateUserRoles(server);
        }

        public static async Task UpdateUserRoles(DiscordServer server)
        {
            server.Users.ForEach(async x => {
                GlobalUser gUser = Program.globalUsers.Find(y => y.UserID == x.userID);
                if (gUser != null)
                {
                    decimal? days = gUser.daysWatchedAnime;
                    ulong roleId = 0;
                    decimal currentDays = 0;
                    server.animeRoles.ForEach(role => { if (role.days > currentDays) { currentDays = role.days; roleId = role.roleId; } });

                    if(roleId != 0)
                    {
                        IGuildUser guildUser = server.Guild.GetUser(x.userID);
                        if (!guildUser.RoleIds.Contains(roleId))
                        {
                            IRole role = server.Guild.GetRole(roleId);
                            await guildUser.AddRoleAsync(role);

                            var deletedRoles = server.animeRoles.ToList();
                            deletedRoles.Remove((roleId, currentDays));

                            var iRoles = deletedRoles.Select(z => server.Guild.GetRole(z.roleId));

                            await guildUser.RemoveRolesAsync(iRoles);
                        }
                    }
                }
            });
        }

        static TimeSpan startTimeSpan = TimeSpan.Zero;
        static TimeSpan periodTimeSpan = TimeSpan.FromSeconds(1);

        public static void SetupTimer()
        {
            var timer = new Timer((e) =>
            {
                if (Program.discordServers != null)
                {
                    Program.discordServers.ForEach(async x =>
                    {
                        await UpdateUserRoles(x);
                    });
                }
            }, null, startTimeSpan, periodTimeSpan);
        }
    }
}
