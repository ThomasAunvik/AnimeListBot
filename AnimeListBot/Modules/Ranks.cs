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
using JikanDotNet;
using System.Security.Cryptography.X509Certificates;
using AnimeListBot.Handler.Database;

namespace AnimeListBot.Modules
{
    public class Ranks : ModuleBase<ShardedCommandContext>
    {
        private IDatabaseService _db;

        public Ranks(IDatabaseService db)
        {
            _db = db;
        }

        [Command("addrank")]
        [Summary("Adds a rank for how many days you have spent with anime/manga (Options: [anime, manga]), Requires ManageRoles on both User and Bot")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task AddRank(RankOption option, IRole role, double days)
        {
            EmbedHandler embed = new EmbedHandler(Context.User, "Adding rank " + role.Name + " (" + days +")...");
            DiscordServer server = await _db.GetServerById(Context.Guild.Id);
            ServerRanks ranks = server.ranks;

            List<RoleRank> optionRanks;
            switch (option)
            {
                case RankOption.ANIME:
                    optionRanks = ranks.AnimeRanks;
                    break;
                case RankOption.MANGA:
                    optionRanks = ranks.MangaRanks;
                    break;
                default:
                    embed.Title = "Incorrect use of options, the modes are: `anime` and `manga`";
                    await embed.SendMessage(Context.Channel);
                    return;
            }

            if (role.Id == Context.Guild.EveryoneRole.Id)
            {
                embed.Title = "You cannot set @everyone as a rank.";
                await embed.SendMessage(Context.Channel);
                return;
            }

            await embed.SendMessage(Context.Channel);

            if (optionRanks.Find(x => x.Id == role.Id.ToString()) != null)
            {
                embed.Title = "Role: " + role.Name + " already exists for " + Enum.GetName(typeof(RankOption), option);
                await embed.UpdateEmbed();
                return;
            }

            RoleRank newRank = new RoleRank();
            newRank.Id = role.Id.ToString();
            newRank.Name = role.Name;
            newRank.Days = days;
            newRank.RawGuildPermissionsValue = role.Permissions.RawValue;
            optionRanks.Add(newRank);
            embed.Title = "Added " + role.Name + " as the rank for having " + days + " days of watching " + Enum.GetName(typeof(RankOption), option).ToLower();

            await embed.UpdateEmbed();
            server.UpdateGuildRoles();
            await _db.SaveChangesAsync();
        }

        [Command("editrank")]
        [Summary("Edits a rank for how many days you have spent with anime/manga (Options: [anime, manga]), Requires ManageRoles on both User and Bot")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task EditRank(RankOption option, IRole role, double newDays)
        {
            EmbedHandler embed = new EmbedHandler(Context.User, "Editing rank " + role.Name + " to " + newDays + "...");
            await embed.SendMessage(Context.Channel);

            DiscordServer server = await _db.GetServerById(Context.Guild.Id);
            ServerRanks ranks = server.ranks;

            List<RoleRank> optionRanks;
            switch (option)
            {
                case RankOption.ANIME:
                    optionRanks = ranks.AnimeRanks;
                    break;
                case RankOption.MANGA:
                    optionRanks = ranks.MangaRanks;
                    break;
                default:
                    embed.Title = "Incorrect use of options, the modes are: `anime` and `manga`";
                    await embed.UpdateEmbed();
                    return;
            }

            RoleRank animeRole = optionRanks.Find(x => x.Id == role.Id.ToString());
            if (animeRole != null)
            {
                animeRole.Days = newDays;
                animeRole.Name = role.Name;
                embed.Title = "Anime rank: " + role.Name + " was set to " + newDays + " days.";
            }
            else
            {
                embed.Title = "Failed to change rank, the role is not set from before.";
            }
            await embed.UpdateEmbed();
            server.UpdateGuildRoles();
            await _db.SaveChangesAsync();
        }

        [Command("removerank")]
        [Summary("Removes a rank (Options: [anime, manga]), Requires ManageRoles on both User and Bot")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task RemoveRank(RankOption option, IRole role)
        {
            EmbedHandler embed = new EmbedHandler(Context.User, "Removing role " + role.Name + " from ranks...");
            await embed.SendMessage(Context.Channel);

            DiscordServer server = await _db.GetServerById(Context.Guild.Id);
            ServerRanks ranks = server.ranks;

            List<RoleRank> optionRanks;
            switch (option)
            {
                case RankOption.ANIME:
                    optionRanks = ranks.AnimeRanks;
                    break;
                case RankOption.MANGA:
                    optionRanks = ranks.MangaRanks;
                    break;
                default:
                    embed.Title = "Incorrect use of options, the modes are: `anime` and `manga`";
                    await embed.UpdateEmbed();
                    return;
            }

            int ranksRemoved = ranks.AnimeRanks.RemoveAll(x => x.Id == role.Id.ToString());
            if (ranksRemoved > 0)
                embed.Title = "Role (" + Enum.GetName(typeof(RankOption), option) + ") " + role.Name + " was removed from the ranks.";
            else
                embed.Title = "Failed to remove rank, the role is not set from before.";

            await embed.UpdateEmbed();
            server.UpdateGuildRoles();
            await _db.SaveChangesAsync();
        }

        [Command("updateranks")]
        [Summary("Requires ManageRoles on both User and Bot")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task UpdateRanks()
        {
            EmbedHandler embed = new EmbedHandler(Context.User, "Updating user ranks on this server...");

            DiscordServer server = await _db.GetServerById(Context.Guild.Id);
            if (DiscordServer.rolesUpdating.Contains(server.ServerId))
            {
                embed.Title = "You cant update user ranks now, its already updating.";
                await embed.SendMessage(Context.Channel);
                return;
            }
            await embed.SendMessage(Context.Channel);

            try
            {
                DiscordServer.rolesUpdating.Add(server.ServerId);

                IGuild guild = server.GetGuild();

                List<DiscordUser> users = _db.GetAllUsers();
                List<DiscordUser> serverUsers = users.FindAll(x => x.Servers?.Find(x => x.ServerId == server.ServerId.ToString()) != null);

                await UpdateUserRoles(server, serverUsers, embed);
                DiscordServer.rolesUpdating.Remove(server.ServerId);
            }
            catch (Exception e)
            {
                DiscordServer.rolesUpdating.Remove(server.ServerId);
                embed.Title = "Failed to update user roles.";
                await Program._logger.LogError(e, Context.User, (IGuildChannel)Context.Channel);
            }

            embed.Title = "Updated all user ranks.";
            embed.Description = "";
            await embed.UpdateEmbed();
        }

        [RequireValidAnimelist]
        [Command("updaterank")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task UpdateRank(IGuildUser user = null)
        {
            if (user == null) user = Context.User as IGuildUser;

            EmbedHandler embed = new EmbedHandler(Context.User, "Updating rank for " + user.Username);
            await embed.SendMessage(Context.Channel);

            DiscordServer server = await _db.GetServerById(Context.Guild.Id);
            DiscordUser duser = await _db.GetUserById(user.Id);

            if (await UpdateUserRole(server, duser, embed))
            {
                embed.Title = "Updated " + user.Username + " rank.";
            }
            else embed.Title = "Failed to update rank.";
            await embed.UpdateEmbed();
            await _db.SaveChangesAsync();
        }

        public static async Task<bool> UpdateUserRole(DiscordServer server, DiscordUser user, EmbedBuilder embed)
        {
            IGuild guild = server.GetGuild();
            IGuildUser botUser = await guild.GetUserAsync(Program._client.CurrentUser.Id);

            if (!botUser.GuildPermissions.ManageRoles)
            {
                if (embed != null && (server.ranks.AnimeRanks.Count > 0 || server.ranks.MangaRanks.Count > 0))
                {
                    embed.AddField("Warning: Unable to update user roles", "Bot requires ManageRoles Permissions!\nTo remove this message, either add the permissions, or remove all ranks.");
                }
                return false;
            }

            IGuildUser guildUser = await DiscordServer.GetGuildUser(guild, user.UserId);
            if (guildUser == null) return false;

            try
            {
                if (!user.HasValidAnimelist()) return false;

                // CALCULATING USER INFO
                await user.UpdateUserInfo();

                RoleRank animeRank = user.GetAnimeServerRank(server);
                RoleRank mangaRank = user.GetMangaServerRank(server);

                // DELETING ROLES

                List<RoleRank> delAnimeRoles = server.ranks.AnimeRanks.ToList();
                if(mangaRank != null) delAnimeRoles.RemoveAll(x => x.Id == animeRank.Id);
                delAnimeRoles.RemoveAll(x => !guildUser.RoleIds.Contains(x.GetRoleID()));

                List<RoleRank> delMangaRoles = server.ranks.MangaRanks.ToList();
                if(mangaRank != null) delMangaRoles.RemoveAll(x => x.Id == mangaRank.Id);
                delMangaRoles.RemoveAll(x => !guildUser.RoleIds.Contains(x.GetRoleID()));

                if (delAnimeRoles.Count > 0 || delMangaRoles.Count > 0)
                {
                    var animeRoles = delAnimeRoles.Select(z => guild.GetRole(z.GetRoleID())).ToArray();
                    var mangaRoles = delMangaRoles.Select(z => guild.GetRole(z.GetRoleID())).ToArray();
                    if(animeRoles.Length > 0)
                    {
                        var animeString = string.Join(", ", animeRoles.Select(x => x.Name));
                        await Program._logger.Log("User: " + guildUser.Username + " lost these anime ranks: " + animeString);
                        await guildUser.RemoveRolesAsync(animeRoles);
                    }

                    if (mangaRoles.Length > 0)
                    {
                        var mangaString = string.Join(", ", mangaRoles.Select(x => x.Name));
                        await Program._logger.Log("User: " + guildUser.Username + " lost these manga ranks: " + mangaString);
                        await guildUser.RemoveRolesAsync(mangaRoles);
                    }
                }

                // ADDING ROLES

                if (animeRank != null)
                {
                    if (!guildUser.RoleIds.Contains(animeRank.GetRoleID()))
                    {
                        IRole role = guild.GetRole(animeRank.GetRoleID());
                        if (role != null)
                        {
                            await guildUser.AddRoleAsync(role);
                            await Program._logger.Log(guildUser.Username + " got anime rank " + role.Name + " in server " + guild.Name);
                        }
                    }
                }

                if (mangaRank != null)
                {
                    if (!guildUser.RoleIds.Contains(mangaRank.GetRoleID()))
                    {
                        IRole role = guild.GetRole(mangaRank.GetRoleID());
                        if (role != null)
                        {
                            await guildUser.AddRoleAsync(role);
                            await Program._logger.Log(guildUser.Username + " got manga rank " + role.Name + " in server " + guild.Name);
                        }
                    }
                }
                return true;
            }catch(Exception e)
            {
                await Program._logger.LogError(e, guildUser);
                return false;
            }
        }

        public static async Task UpdateUserRoles(DiscordServer server, List<DiscordUser> users, EmbedHandler embed)
        {
            string updateText = $"Updating ({ server.ServerId }) for user roles: Progress ";
            await Program._logger.Log(updateText + "[----------] 0%");
            int currentCursorPos = Console.CursorTop;

            if (embed != null)
            {
                embed.Description = "Progress: 0%";
                await embed.UpdateEmbed();
            }

            //IReadOnlyCollection<IGuildUser> iro_users = await guild.GetUsersAsync();
            //List<IGuildUser> users = iro_users.ToList();
            for (int i = 0; i < users.Count; i++) {
                DiscordUser user = users[i];
                await UpdateUserRole(server, user, embed);

                float progress = i / (float)users.Count;
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
            await Program._logger.ReplaceLine(currentCursorPos - 1, $"Finished user role update for server {server.ServerId}.                                  ");
        }

        [Command("Ranks")]
        public async Task ListRanks()
        {
            EmbedHandler embed = new EmbedHandler(Context.User, "Geting all user ranks...");
            await embed.SendMessage(Context.Channel);
            embed.Title = "List of Ranks";

            DiscordServer server = await _db.GetServerById(Context.Guild.Id);
            ServerRanks ranks = server.ranks;
            if(ranks.AnimeRanks.Count <= 0 && ranks.MangaRanks.Count <= 0)
            {
                embed.Title = "This server does not have any anime and manga ranks.";
                embed.Description = "For administrators: Use `" + server.Prefix + "addrank`";
                await embed.UpdateEmbed();
                return;
            }

            // ANIME

            if (ranks.AnimeRanks.Count > 0)
            {
                var animeRanks = ranks.AnimeRanks.OrderBy(o => -o.Days).ToList();

                RoleRank animeRank = animeRanks[0];
                IRole animeRole = Context.Guild.GetRole(animeRank.GetRoleID());

                string animeRoleList = animeRole.Name + " for " + animeRank.Days + " days";
                for (int i = 1; i < animeRanks.Count; i++)
                {
                    animeRank = animeRanks[i];
                    animeRole = Context.Guild.GetRole(animeRank.GetRoleID());
                    animeRoleList += "\n" + animeRole.Name + " for " + animeRank.Days + " days";
                }
                embed.AddFieldSecure(new EmbedFieldBuilder()
                {
                    Name = "Anime",
                    Value = animeRoleList
                });
            }

            // MANGA

            if (ranks.MangaRanks.Count > 0)
            {
                var mangaRanks = ranks.MangaRanks.OrderBy(o => -o.Days).ToList();

                RoleRank mangaRank = mangaRanks[0];
                IRole mangaRole = Context.Guild.GetRole(mangaRank.GetRoleID());

                string mangaRoleList = mangaRole.Name + " for " + mangaRank.Days + " days";
                for (int i = 1; i < mangaRanks.Count; i++)
                {
                    mangaRank = mangaRanks[i];
                    mangaRole = Context.Guild.GetRole(mangaRank.GetRoleID());
                    mangaRoleList += "\n" + mangaRole.Name + " for " + mangaRank.Days + " days";
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
