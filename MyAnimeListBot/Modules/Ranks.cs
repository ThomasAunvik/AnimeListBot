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
            
            if(server.animeRoles.Contains((role.Id, days))) {
                await ReplyAsync("Already added " + role.Name + " with " + days + " days.");
                return;
            }
            server.animeRoles.Add((role.Id, days));
            server.SaveData();

            await ReplyAsync("Added " + role.Name + " as the rank for having " + days + " of watching/reading anime/manga");
        }

        [Command("updateranks")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task UpdateRanks()
        {
            DiscordServer server = DiscordServer.GetServerFromID(Context.Guild.Id);
            await UpdateUserRoles(server);
            await ReplyAsync("Updated all ranks.");
        }

        [Command("updaterank")]
        public async Task UpdateRank(IGuildUser user = null)
        {
            if (user == null) user = Context.User as IGuildUser;

            if (await UpdateUserRole(user))
            {
                await ReplyAsync("Updated " + user.Username + " rank.");
            }
            else await ReplyAsync("Failed to update rank.");
        }

        public static async Task<bool> UpdateUserRole(IGuildUser user)
        {
            DiscordServer server = DiscordServer.GetServerFromID(user.Guild.Id);
            if (server != null)
            {
                ServerUser sUser = server.GetUserFromId(user.Id);
                GlobalUser gUser = Program.globalUsers.Find(x => x.userID == user.Id);
                if (sUser != null && gUser != null)
                {
                    await UpdateUserRole(server, sUser, gUser);
                    return true;
                }
            }
            return false;
        }

        public static async Task UpdateUserRole(DiscordServer server, ServerUser sUser, GlobalUser gUser)
        {
            await gUser.UpdateMALInfo();

            decimal? days = gUser.daysWatchedAnime;
            ulong roleId = 0;
            decimal currentDays = 0;
            server.animeRoles.ForEach(role => { if (role.days < days) { currentDays = role.days; roleId = role.roleId; } });

            Console.WriteLine("Getting userid: " + gUser.userID + " from: " + gUser.Username);
            IGuildUser guildUser = server.Guild.GetUser(gUser.userID);

            var deletedRoles = server.animeRoles.ToList();
            deletedRoles.Remove((roleId, currentDays));
            deletedRoles.ToList().ForEach(x => { if (!guildUser.RoleIds.Contains(x.roleId)) deletedRoles.Remove(x); });

            var iRoles = deletedRoles.Select(z => server.Guild.GetRole(z.roleId)).ToList();
            iRoles.ForEach(x => { Console.WriteLine(gUser.Username + " lost rank " + x.Name); });
            await guildUser.RemoveRolesAsync(iRoles);

            if (roleId != 0 && roleId != sUser.currentRankId)
            {
                
                if (!guildUser.RoleIds.Contains(roleId))
                {
                    IRole role = server.Guild.GetRole(roleId);
                    await guildUser.AddRoleAsync(role);
                    Console.WriteLine(gUser.Username + " got rank " + role.Name);
                }
                sUser.currentRankId = roleId;
            }
            gUser.SaveData();
        }

        public static async Task UpdateUserRoles(DiscordServer server)
        {
            Console.WriteLine("Updating...");
            foreach(ServerUser sUser in server.Users) {
                GlobalUser gUser = Program.globalUsers.Find(x => x.userID == sUser.userID);
                if (gUser != null)
                {
                    await UpdateUserRole(server, sUser, gUser);
                }
            }
        }

        [Command("Ranks")]
        public async Task ListRanks()
        {
            DiscordServer server = DiscordServer.GetServerFromID(Context.Guild.Id);

            EmbedBuilder embed = new EmbedBuilder() {
                Color = Program.embedColor
            };

            (ulong roleId, decimal days) rank = server.animeRoles[0];
            IRole role = Context.Guild.GetRole(rank.roleId);

            string listString = role.Name + " for " + rank.days + " days";
            for(int i = 1; i < server.animeRoles.Count; i++)
            {
                rank = server.animeRoles[i];
                role = Context.Guild.GetRole(rank.roleId);
                listString += "\n" + role.Name + " for " + rank.days + " days";
            }
            embed.AddField(new EmbedFieldBuilder()
            {
                Name = "List of Ranks",
                Value = listString
            });

            await ReplyAsync("", false, embed.Build());
        }
    }
}
