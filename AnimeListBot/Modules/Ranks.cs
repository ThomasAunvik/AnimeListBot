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
    public class Ranks : ModuleBase<ShardedCommandContext>
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
                if (server.AnimeroleId.Contains((long)role.Id))
                {
                    embed.Title = "Anime Role: " + role.Name + " already exists.";
                    await embed.UpdateEmbed();
                    return;
                }

                var roleid = server.AnimeroleId.ToList();
                roleid.Add((long)role.Id);
                server.AnimeroleId = roleid;

                var roledays = server.AnimeroleDays.ToList();
                roledays.Add(days);
                server.AnimeroleDays = roledays;

                await DatabaseConnection.db.SaveChangesAsync();
                embed.Title = "Added " + role.Name + " as the rank for having " + days + " of watching anime";
            }
            else if(option == "manga")
            {
                if (server.MangaroleId.Contains((long)role.Id))
                {
                    embed.Title = "Manga Role: " + role.Name + " already exists.";
                    await embed.UpdateEmbed();
                    return;
                }
                var roleid = server.MangaroleId.ToList();
                roleid.Add((long)role.Id);
                server.MangaroleId = roleid;

                var roledays = server.MangaroleDays.ToList();
                roledays.Add(days);
                server.MangaroleDays = roledays;

                await DatabaseConnection.db.SaveChangesAsync();
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
                int animeRoleIndex = server.AnimeroleId.ToList().FindIndex(x => x == (long)role.Id);
                if (animeRoleIndex >= 0) {
                    server.AnimeroleDays[animeRoleIndex] = newDays;
                    embed.Title = "Anime rank: " + role.Name + " was set to " + newDays + " days.";
                }
                else
                {
                    embed.Title = "Failed to change rank, the role is not set from before.";
                }
            }
            else if (option == "manga")
            {
                int mangaRoleIndex = server.MangaroleId.ToList().FindIndex(x => x == (long)role.Id);
                if (mangaRoleIndex >= 0)
                {
                    server.MangaroleDays[mangaRoleIndex] = newDays;
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
                var roleid = server.AnimeroleId.ToList();
                int animeRoleIndex = roleid.FindIndex(x => x == (long)role.Id);
                if (animeRoleIndex >= 0)
                {
                    roleid.RemoveAt(animeRoleIndex);
                    server.AnimeroleId = roleid;

                    var roledays = server.AnimeroleDays.ToList();
                    roledays.RemoveAt(animeRoleIndex);
                    server.AnimeroleDays = roledays;

                    await DatabaseConnection.db.SaveChangesAsync();
                    embed.Title = "Anime Role " + role.Name + " was removed from the ranks.";
                }
                else
                {
                    embed.Title = "Failed to remove rank, the role is not set from before.";
                }
            }
            else if (option == "manga")
            {
                var roleid = server.MangaroleId.ToList();
                int mangaRoleIndex = roleid.FindIndex(x => x == (long)role.Id);
                if (mangaRoleIndex >= 0)
                {
                    roleid.RemoveAt(mangaRoleIndex);
                    server.MangaroleId = roleid;

                    var roledays = server.MangaroleDays.ToList();
                    roledays.RemoveAt(mangaRoleIndex);
                    server.MangaroleDays = roledays;

                    await DatabaseConnection.db.SaveChangesAsync();
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
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task UpdateRanks()
        {
            EmbedHandler embed = new EmbedHandler(Context.User, "Updating user ranks on this server...");

            DiscordServer server = await DatabaseRequest.GetServerById(Context.Guild.Id);
            if (DiscordServer.rolesUpdating.Contains(server.ServerId))
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
                    DiscordServer.rolesUpdating.Add(server.ServerId);
                    await UpdateUserRoles(server, embed);
                    DiscordServer.rolesUpdating.Remove(server.ServerId);
                }
                catch(Exception e)
                {
                    DiscordServer.rolesUpdating.Remove(server.ServerId);
                    embed.Title = "Failed to update user roles.";
                    await Program._logger.LogError(e, Context.User, (IGuildChannel)Context.Channel);
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

            if (!DatabaseRequest.DoesUserIdExist(user.Id))
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
            IGuildUser guildUser = await DiscordServer.GetGuildUser(guild, user.UserId);
            if (guildUser == null) return false;

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

                List<long> delAnimeRoles = new List<long>();
                if (server.AnimeroleId != null && server.AnimeroleId.Count > 0)
                {
                    delAnimeRoles = server.AnimeroleId.ToList();
                    if (animeRankRole != null) delAnimeRoles.Remove((long)animeRankRole.Id);
                    delAnimeRoles.ToList().ForEach(x => {
                        if (!guildUser.RoleIds.Contains((ulong)x))
                            delAnimeRoles.Remove(x);
                    });
                }

                List<long> delMangaRoles = new List<long>();
                if (server.MangaroleId != null &&  server.MangaroleId.Count > 0)
                {
                    delMangaRoles = server.MangaroleId.ToList();
                    if (mangaRankRole != null) delMangaRoles.Remove((long)mangaRankRole.Id);
                    delMangaRoles.ToList().ForEach(x => {
                        if (!guildUser.RoleIds.Contains((ulong)x))
                            delMangaRoles.Remove(x);
                    });
                }

                if (delAnimeRoles.Count > 0 || delMangaRoles.Count > 0)
                {
                    var rolesToDelete = delAnimeRoles.Select(z => guild.GetRole((ulong)z)).ToList();
                    delMangaRoles.ForEach(x => { rolesToDelete.Add(guild.GetRole((ulong)x)); });

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
                await Program._logger.LogError(e, guildUser);
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

                if (!DatabaseRequest.DoesUserIdExist(sUser.Id))
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
            if((server.AnimeroleId == null && server.AnimeroleDays == null) || (server.AnimeroleId.Count <= 0 && server.MangaroleId.Count <= 0))
            {
                embed.Title = "This server does not have any anime and manga ranks.";
                embed.Description = "For administrators: Use `" + server.Prefix + "addrank`";
                await embed.UpdateEmbed();
                return;
            }

            // ANIME

            if (server.AnimeroleId != null && server.AnimeroleId.Count > 0)
            {
                (ulong roleId, double days) animeRank = ((ulong)server.AnimeroleId[0], server.AnimeroleDays[0]);
                IRole animeRole = Context.Guild.GetRole(animeRank.roleId);

                string animeRoleList = animeRole.Name + " for " + animeRank.days + " days";
                for (int i = 1; i < server.AnimeroleId.Count; i++)
                {
                    animeRank = ((ulong)server.AnimeroleId[i], server.AnimeroleDays[0]);
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

            if (server.MangaroleId != null && server.MangaroleId.Count > 0)
            {
                (ulong roleId, double days) mangaRank = ((ulong)server.MangaroleId[0], server.MangaroleDays[0]);
                IRole mangaRole = Context.Guild.GetRole(mangaRank.roleId);

                string mangaRoleList = mangaRole.Name + " for " + mangaRank.days + " days";
                for (int i = 1; i < server.MangaroleId.Count; i++)
                {
                    mangaRank = ((ulong)server.MangaroleId[i], server.MangaroleDays[i]);
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
