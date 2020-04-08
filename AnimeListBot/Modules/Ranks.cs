using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using AnimeListBot.Handler;
using Discord.WebSocket;

namespace AnimeListBot.Modules
{
    public class Ranks : ModuleBase<ICommandContext>
    {
        [Command("addrank")]
        [Summary("Adds a rank for how many days you have spent with anime/manga (Options: [anime, manga])")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task AddRank(string option, IRole role, double days)
        {
            DiscordServer server = await DatabaseRequest.GetServerById(Context.Guild.Id);
            EmbedHandler embed = new EmbedHandler(Context.User, "Adding rank " + role.Name + " (" + days +")...");
            await embed.SendMessage(Context.Channel);

            if (option == "anime")
            {
                if (server.animeRoleIds.Contains(role.Id))
                {
                    embed.Title = "Anime Role: " + role.Name + " already exists.";
                    await embed.UpdateEmbed();
                    return;
                }
                server.animeRoleIds.Add(role.Id);
                server.animeRoleDays.Add(days);
                embed.Title = "Added " + role.Name + " as the rank for having " + days + " of watching anime";
            }
            else if(option == "manga")
            {
                if (server.mangaRoleIds.Contains(role.Id))
                {
                    embed.Title = "Manga Role: " + role.Name + " already exists.";
                    await embed.UpdateEmbed();
                    return;
                }
                server.mangaRoleIds.Add(role.Id);
                server.mangaRoleDays.Add(days);
                embed.Title = "Added " + role.Name + " as the rank for having " + days + " of reading manga";
            }
            else
            {
                embed.Title = "Incorrect use of modes, the modes are: `anime` and `manga`";
            }
            await DatabaseRequest.UpdateServer(server);
            await embed.UpdateEmbed();
        }

        [Command("editrank")]
        [Summary("Edits a rank for how many days you have spent with anime/manga (Options: [anime, manga])")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task EditRank(string option, IRole role, double newDays)
        {
            EmbedHandler embed = new EmbedHandler(Context.User, "Editing rank " + role.Name + " to " + newDays + "...");
            await embed.SendMessage(Context.Channel);

            DiscordServer server = await DatabaseRequest.GetServerById(Context.Guild.Id);

            if (option == "anime")
            {
                int animeRoleIndex = server.animeRoleIds.FindIndex(x => x == role.Id);
                if (animeRoleIndex >= 0) {
                    server.animeRoleDays[animeRoleIndex] = newDays;
                    embed.Title = "Anime rank: " + role.Name + " was set to " + newDays + " days.";
                }
                else
                {
                    embed.Title = "Failed to change rank, the role is not set from before.";
                }
            }
            else if (option == "manga")
            {
                int mangaRoleIndex = server.mangaRoleIds.FindIndex(x => x == role.Id);
                if (mangaRoleIndex >= 0)
                {
                    server.mangaRoleDays[mangaRoleIndex] = newDays;
                    embed.Title = "Manga rank: " + role.Name + " was set to " + newDays + " days.";
                }
                else
                {
                    embed.Title = "Failed to change rank, the role is not set from before.";
                }
            }
            else
            {
                embed.Title = "Incorrect use of modes, the modes are: `anime` and `manga`";
            }
            await DatabaseRequest.UpdateServer(server);
            await embed.UpdateEmbed();
        }

        [Command("removerank")]
        [Summary("Removes a rank (Options: [anime, manga])")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task RemoveRank(string option, IRole role)
        {
            EmbedHandler embed = new EmbedHandler(Context.User, "Removing role " + role.Name + " from ranks...");
            await embed.SendMessage(Context.Channel);

            DiscordServer server = await DatabaseRequest.GetServerById(Context.Guild.Id);

            if (option == "anime")
            {
                int animeRoleIndex = server.animeRoleIds.FindIndex(x => x == role.Id);
                if (animeRoleIndex >= 0)
                {
                    server.animeRoleIds.RemoveAt(animeRoleIndex);
                    server.animeRoleDays.RemoveAt(animeRoleIndex);
                    embed.Title = "Anime Role " + role.Name + " was removed from the ranks.";
                }
                else
                {
                    embed.Title = "Failed to remove rank, the role is not set from before.";
                }
            }
            else if (option == "manga")
            {
                int mangaRoleIndex = server.mangaRoleIds.FindIndex(x => x == role.Id);
                if (mangaRoleIndex >= 0)
                {
                    server.mangaRoleIds.RemoveAt(mangaRoleIndex);
                    server.mangaRoleDays.RemoveAt(mangaRoleIndex);
                    embed.Title = "Manga Role " + role.Name + " was removed from the ranks.";
                }
                else
                {
                    embed.Title = "Failed to remove rank, the role is not set from before.";
                }
            }
            else
            {
                embed.Title = "Incorrect use of options, the modes are: `anime` and `manga`";
                return;
            }
            await DatabaseRequest.UpdateServer(server);
            await embed.UpdateEmbed();
        }

        [Command("updateranks")]
        //[RequireBotPermission(GuildPermission.ManageRoles)]
        //[RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task UpdateRanks()
        {
            EmbedHandler embed = new EmbedHandler(Context.User, "Updating user ranks on this server...");

            DiscordServer server = await DatabaseRequest.GetServerById(Context.Guild.Id);
            if (DiscordServer.rolesUpdating.Contains(server.id))
            {
                embed.Title = "You cant update user ranks now, its already updating.";
                await embed.SendMessage(Context.Channel);
                return;
            }
            await embed.SendMessage(Context.Channel);

            new Thread(async () =>
            {
                try
                {
                    DiscordServer.rolesUpdating.Add(server.id);
                    await UpdateUserRoles(server, embed);
                    DiscordServer.rolesUpdating.Remove(server.id);
                }
                catch(Exception e)
                {
                    DiscordServer.rolesUpdating.Remove(server.id);
                    embed.Title = "Failed to update user roles.";
                    await Program._logger.LogError(e);
                }

                embed.Title = "Updated all user ranks.";
                embed.Description = "";
                await embed.UpdateEmbed();
            }).Start();
        }

        [Command("updaterank")]
        public async Task UpdateRank(IGuildUser user = null)
        {
            if (user == null) user = Context.User as IGuildUser;

            EmbedHandler embed = new EmbedHandler(Context.User, "Updating rank for " + user.Username);
            await embed.SendMessage(Context.Channel);

            DiscordServer server = await DatabaseRequest.GetServerById(Context.Guild.Id);
            DiscordUser duser;

            if (!await DatabaseRequest.DoesUserIdExist(user.Id))
                await DatabaseRequest.CreateUser(duser = new DiscordUser(user));
            else duser = await DatabaseRequest.GetUserById(user.Id);

            if (await UpdateUserRole(server, duser, embed))
            {
                embed.Title = "Updated " + user.Username + " rank.";
            }
            else embed.Title = "Failed to update rank.";
            await DatabaseRequest.UpdateUser(duser);
            await embed.UpdateEmbed();
        }
        /*
        public static async Task<bool> UpdateUserRole(IGuildUser user, EmbedBuilder embed)
        {
            DiscordServer server = DiscordServer.GetServerFromID(user.Guild.Id);
            if (server != null && (server.animeRoles.Count > 0 || server.mangaRoles.Count > 0))
            {
                ServerUser sUser = server.GetUserFromId(user.Id);
                if (sUser != null)
                {
                    GlobalUser gUser = Program.globalUsers.Find(x => x.userID == user.Id);
                    if (gUser != null)
                    {
                        await UpdateUserRole(server, sUser, gUser, embed);
                        return true;
                    }
                }
            }
            return false;
        }*/

        public static async Task<bool> UpdateUserRole(DiscordServer server, DiscordUser user, EmbedBuilder embed)
        {
            IGuild guild = server.GetGuild();
            IGuildUser guildUser = await DiscordServer.GetGuildUser(guild, user.userID);

            try
            {
                if (user.malProfile == null && user.anilistProfile == null) return true;

                // CALCULATING USER INFO
                await user.UpdateUserInfo();

                (ulong, double) animeRank = user.GetAnimeServerRank(server);
                (ulong, double) mangaRank = user.GetMangaServerRank(server);
                IRole animeRankRole = null;
                if (animeRank.Item1 != 0) animeRankRole = guild.GetRole(animeRank.Item1);
                IRole mangaRankRole = null;
                if (mangaRank.Item1 != 0) mangaRankRole = guild.GetRole(mangaRank.Item1);

                // DELETING ROLES

                var delAnimeRoles = server.animeRoleIds.ToList();
                if (server.animeRoleIds.Count > 0)
                {
                    if(animeRankRole != null) delAnimeRoles.Remove(animeRankRole.Id);
                    delAnimeRoles.ToList().ForEach(x => {
                        if (!guildUser.RoleIds.Contains(x))
                            delAnimeRoles.Remove(x);
                    });
                }

                var delMangaRoles = server.mangaRoleIds.ToList();
                if (server.mangaRoleIds.Count > 0)
                {
                    if (mangaRankRole != null) delMangaRoles.Remove(mangaRankRole.Id);
                    delMangaRoles.ToList().ForEach(x => {
                        if (!guildUser.RoleIds.Contains(x))
                            delMangaRoles.Remove(x);
                    });
                }

                if (delAnimeRoles.Count > 0 || delMangaRoles.Count > 0)
                {
                    var rolesToDelete = delAnimeRoles.Select(z => guild.GetRole(z)).ToList();
                    delMangaRoles.ForEach(x => { rolesToDelete.Add(guild.GetRole(x)); });

                    rolesToDelete.ForEach(async x => { await Program._logger.Log(guildUser.Username + " lost rank " + x.Name); });
                    await guildUser.RemoveRolesAsync(rolesToDelete);
                }

                // ADDING ROLES

                if (animeRankRole != null)
                {
                    if (!guildUser.RoleIds.Contains(animeRankRole.Id))
                    {
                        IRole role = guild.GetRole(animeRankRole.Id);
                        await guildUser.AddRoleAsync(role);
                        await Program._logger.Log(guildUser.Username + " got anime rank " + role.Name + " in server " + guild.Name);
                    }
                }

                if (mangaRankRole != null)
                {
                    if (!guildUser.RoleIds.Contains(mangaRankRole.Id))
                    {
                        IRole role = guild.GetRole(mangaRankRole.Id);
                        await guildUser.AddRoleAsync(role);
                        await Program._logger.Log(guildUser.Username + " got manga rank " + role.Name + " in server " + guild.Name);
                    }
                }
                return true;
            }catch(Exception e)
            {
                await Program._logger.LogError(e, (IUser)guildUser);
                return false;
            }
        }

        public static async Task UpdateUserRoles(DiscordServer server, EmbedHandler embed)
        {
            IGuild guild = server.GetGuild();

            string updateText = $"Updating ({ guild.Id }) for user roles: Progress ";
            await Program._logger.Log(updateText + "[----------] 0%");
            int currentCursorPos = Console.CursorTop;

            if (embed != null)
            {
                embed.Description = "Progress: 0%";
                await embed.UpdateEmbed();
            }

            float progress = 0;
            
            IReadOnlyCollection<IGuildUser> iro_users = await guild.GetUsersAsync();
            List<IGuildUser> users = iro_users.ToList();
            for(int i = 0; i < users.Count; i++) {
                IGuildUser sUser = users[i];
                DiscordUser user;

                if (!await DatabaseRequest.DoesUserIdExist(sUser.Id))
                    await DatabaseRequest.CreateUser(user = new DiscordUser(sUser));
                else user = await DatabaseRequest.GetUserById(sUser.Id);

                await UpdateUserRole(server, user, embed);

                await DatabaseRequest.UpdateUser(user);

                progress = i / (float)users.Count;
                progress *= 100;
                progress = MathF.Round(progress);

                string newText = "[";

                for (int j = 0; j < 10; j++)
                    if (j <= (progress / 10) - 1) newText += "=";
                    else newText += "-";

                if(currentCursorPos != 0 || (progress % 10 == 0 && progress / 10 != 0)) await Program._logger.ReplaceLine(currentCursorPos - 1, updateText + newText + "] " + progress + "%");

                if (progress % 10 == 0)
                {
                    if (embed != null)
                    {
                        embed.Description = "Progress: " + progress + "%";
                        await embed.UpdateEmbed();
                    }
                }
            }
            await Program._logger.ReplaceLine(currentCursorPos - 1, $"Finished user role update for server {guild.Id}.                                  ");
        }

        [Command("Ranks")]
        public async Task ListRanks()
        {
            EmbedHandler embed = new EmbedHandler(Context.User, "Geting all user ranks...");
            await embed.SendMessage(Context.Channel);
            embed.Title = "List of Ranks";

            DiscordServer server = await DatabaseRequest.GetServerById(Context.Guild.Id);
            if(server.animeRoleIds.Count <= 0 && server.mangaRoleIds.Count <= 0)
            {
                embed.Title = "This server does not have any anime and manga ranks. (For administrators: Use `" + Program.botPrefix + "addrank`";
                await embed.UpdateEmbed();
                return;
            }

            // ANIME

            if (server.animeRoleIds.Count > 0)
            {
                (ulong roleId, double days) animeRank = (server.animeRoleIds[0], server.animeRoleDays[0]);
                IRole animeRole = Context.Guild.GetRole(animeRank.roleId);

                string animeRoleList = animeRole.Name + " for " + animeRank.days + " days";
                for (int i = 1; i < server.animeRoleIds.Count; i++)
                {
                    animeRank = (server.animeRoleIds[i], server.animeRoleDays[0]);
                    animeRole = Context.Guild.GetRole(animeRank.roleId);
                    animeRoleList += "\n" + animeRole.Name + " for " + animeRank.days + " days";
                }
                embed.AddFieldSecure(new EmbedFieldBuilder()
                {
                    Name = "Anime",
                    Value = animeRoleList
                });
            }

            // MANGA

            if (server.mangaRoleIds.Count > 0)
            {
                (ulong roleId, double days) mangaRank = (server.mangaRoleIds[0], server.mangaRoleDays[0]);
                IRole mangaRole = Context.Guild.GetRole(mangaRank.roleId);

                string mangaRoleList = mangaRole.Name + " for " + mangaRank.days + " days";
                for (int i = 1; i < server.mangaRoleIds.Count; i++)
                {
                    mangaRank = (server.mangaRoleIds[i], server.mangaRoleDays[i]);
                    mangaRole = Context.Guild.GetRole(mangaRank.roleId);
                    mangaRoleList += "\n" + mangaRole.Name + " for " + mangaRank.days + " days";
                }
                embed.AddFieldSecure(new EmbedFieldBuilder()
                {
                    Name = "Manga",
                    Value = mangaRoleList
                });
            }

            await embed.UpdateEmbed();
        }
    }
}
