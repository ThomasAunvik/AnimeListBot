﻿using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using AnimeListBot.Handler;

namespace AnimeListBot.Modules
{
    public class Ranks : ModuleBase<ICommandContext>
    {
        [Command("addrank")]
        [Summary("Adds a rank for how many days you have spent with anime/manga (Options: [anime, manga])")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task AddRank(string option, IRole role, decimal days)
        {
            DiscordServer server = DiscordServer.GetServerFromID(Context.Guild.Id);
            EmbedHandler embed = new EmbedHandler(Context.User, "Adding rank " + role.Name + " (" + days +")...");
            await embed.SendMessage(Context.Channel);

            if (option == "anime")
            {
                if (server.animeRoles.Contains((role.Id, days)))
                {
                    embed.Title = "Already added animerole " + role.Name + " with " + days + " days.";
                    await embed.UpdateEmbed();
                    return;
                }
                server.animeRoles.Add((role.Id, days));
                embed.Title = "Added " + role.Name + " as the rank for having " + days + " of watching anime";
            }
            else if(option == "manga")
            {
                if (server.mangaRoles.Contains((role.Id, days)))
                {
                    embed.Title = "Already added mangarole " + role.Name + " with " + days + " days.";
                    await embed.UpdateEmbed();
                    return;
                }
                server.mangaRoles.Add((role.Id, days));
                embed.Title = "Added " + role.Name + " as the rank for having " + days + " of reading manga";
            }
            else
            {
                embed.Title = "Incorrect use of modes, the modes are: `anime` and `manga`";
            }
            await embed.UpdateEmbed();
            server.SaveData();
        }

        [Command("editrank")]
        [Summary("Edits a rank for how many days you have spent with anime/manga (Options: [anime, manga])")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task EditRank(string option, IRole role, decimal newDays)
        {
            EmbedHandler embed = new EmbedHandler(Context.User, "Editing rank " + role.Name + " to " + newDays + "...");
            await embed.SendMessage(Context.Channel);

            DiscordServer server = DiscordServer.GetServerFromID(Context.Guild.Id);

            if (option == "anime")
            {
                int animeRoleIndex = server.animeRoles.FindIndex(x => x.roleId == role.Id);
                if (animeRoleIndex >= 0) {
                    server.animeRoles[animeRoleIndex] = (role.Id, newDays);
                    embed.Title = "Anime rank: " + role.Name + " was set to " + newDays + " days.";
                }
                else
                {
                    embed.Title = "Failed to change rank, the role is not set from before.";
                }
            }
            else if (option == "manga")
            {
                int mangaRoleIndex = server.mangaRoles.FindIndex(x => x.roleId == role.Id);
                if (mangaRoleIndex >= 0)
                {
                    server.mangaRoles[mangaRoleIndex] = (role.Id, newDays);
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
            await embed.UpdateEmbed();
            server.SaveData();
        }

        [Command("removerank")]
        [Summary("Removes a rank (Options: [anime, manga])")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task RemoveRank(string option, IRole role)
        {
            EmbedHandler embed = new EmbedHandler(Context.User, "Removing role " + role.Name + " from ranks...");
            await embed.SendMessage(Context.Channel);

            DiscordServer server = DiscordServer.GetServerFromID(Context.Guild.Id);

            if (option == "anime")
            {
                int animeRoleIndex = server.animeRoles.FindIndex(x => x.roleId == role.Id);
                if (animeRoleIndex >= 0)
                {
                    server.animeRoles.RemoveAt(animeRoleIndex);
                    embed.Title = "Anime Role " + role.Name + " was removed from the ranks.";
                }
                else
                {
                    embed.Title = "Failed to remove rank, the role is not set from before.";
                }
            }
            else if (option == "manga")
            {
                int mangaRoleIndex = server.mangaRoles.FindIndex(x => x.roleId == role.Id);
                if (mangaRoleIndex >= 0)
                {
                    server.mangaRoles.RemoveAt(mangaRoleIndex);
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
            await embed.UpdateEmbed();
            server.SaveData();
        }

        [Command("updateranks")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task UpdateRanks()
        {
            EmbedHandler embed = new EmbedHandler(Context.User, "Updating user ranks on this server...");

            DiscordServer server = DiscordServer.GetServerFromID(Context.Guild.Id);
            if (server.isUpdatingRoles)
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
                    server.isUpdatingRoles = true;
                    await UpdateUserRoles(server, embed);
                    server.isUpdatingRoles = false;
                }
                catch(Exception e)
                {
                    server.isUpdatingRoles = false;
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

            if (await UpdateUserRole(user, embed))
            {
                embed.Title = "Updated " + user.Username + " rank.";
            }
            else embed.Title = "Failed to update rank.";
            await embed.UpdateEmbed();
        }

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
        }

        public static async Task UpdateUserRole(DiscordServer server, ServerUser sUser, GlobalUser gUser, EmbedBuilder embed)
        {
            try
            {
                // CALCULATING USER INFO

                await gUser.UpdateCurrentAnimelist();

                // ANIME
                decimal? animeDays = gUser.GetAnimeWatchDays();
                ulong animeRoleId = 0;
                decimal currentAnimeDays = 0;
                server.animeRoles.ForEach(role => { if (role.days <= animeDays) { currentAnimeDays = role.days; animeRoleId = role.roleId; } });

                // MANGA
                decimal? mangaDays = gUser.GetMangaReadDays();
                ulong mangaRoleId = 0;
                decimal currentMangaDays = 0;
                server.mangaRoles.ForEach(role => { if (role.days <= mangaDays) { currentMangaDays = role.days; mangaRoleId = role.roleId; } });

                // GETTING USER INFO

                // await Program._logger.Log("Getting userid: " + gUser.userID + " from: " + gUser.Username);
                IGuildUser guildUser = server.Guild.GetUser(gUser.userID);

                // DELETING ROLES

                var delAnimeRoles = server.animeRoles.ToList();
                if (server.animeRoles != null && server.animeRoles.Count > 0)
                {
                    delAnimeRoles.Remove((animeRoleId, currentAnimeDays));
                    delAnimeRoles.ToList().ForEach(x => { if (!guildUser.RoleIds.Contains(x.roleId)) delAnimeRoles.Remove(x); });
                }

                var delMangaRoles = server.mangaRoles.ToList();
                if (server.mangaRoles != null && server.mangaRoles.Count > 0)
                {
                    delMangaRoles.Remove((mangaRoleId, currentMangaDays));
                    delMangaRoles.ToList().ForEach(x => { if (!guildUser.RoleIds.Contains(x.roleId)) delMangaRoles.Remove(x); });
                }

                if (delAnimeRoles.Count > 0 || delMangaRoles.Count > 0)
                {
                    var rolesToDelete = delAnimeRoles.Select(z => server.Guild.GetRole(z.roleId)).ToList();
                    delMangaRoles.ForEach(x => { rolesToDelete.Add(server.Guild.GetRole(x.roleId)); });

                    rolesToDelete.ForEach(async x => { await Program._logger.Log(gUser.Username + " lost rank " + x.Name); });
                    await guildUser.RemoveRolesAsync(rolesToDelete);
                }

                // ADDING ROLES

                if (animeRoleId != 0)
                {
                    if (!guildUser.RoleIds.Contains(animeRoleId))
                    {
                        IRole role = server.Guild.GetRole(animeRoleId);
                        await guildUser.AddRoleAsync(role);
                        await Program._logger.Log(gUser.Username + " got anime rank " + role.Name + " in server " + server.Guild.Name);
                    }
                    sUser.currentAnimeRankId = animeRoleId;
                }

                if (mangaRoleId != 0)
                {
                    if (!guildUser.RoleIds.Contains(mangaRoleId))
                    {
                        IRole role = server.Guild.GetRole(mangaRoleId);
                        await guildUser.AddRoleAsync(role);
                        await Program._logger.Log(gUser.Username + " got manga rank " + role.Name + " in server " + server.Guild.Name);
                    }
                    sUser.currentMangaRankId = mangaRoleId;
                }
                gUser.SaveData();
            }catch(Exception e)
            {
                IUser user = Program._client.GetUser(gUser.userID);
                await Program._logger.LogError(e, user);
            }
        }

        public static async Task UpdateUserRoles(DiscordServer server, EmbedHandler embed)
        {
            string updateText = $"Updating ({ server.Guild.Id }) for user roles: Progress ";
            await Program._logger.Log(updateText + "[----------] 0%");
            int currentCursorPos = Console.CursorTop;

            if (embed != null)
            {
                embed.Description = "Progress: 0%";
                await embed.UpdateEmbed();
            }

            float progress = 0;
            
            for(int i = 0; i < server.Users.Count; i++) {
                ServerUser sUser = server.Users[i];
                GlobalUser gUser = Program.globalUsers.Find(x => x.userID == sUser.userID);
                if (gUser != null)
                {
                    await UpdateUserRole(server, sUser, gUser, embed);
                }

                progress = (float)i / server.Users.Count;
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
            await Program._logger.ReplaceLine(currentCursorPos - 1, $"Finished user role update for server {server.Guild.Id}.                                  ");
        }

        [Command("Ranks")]
        public async Task ListRanks()
        {
            EmbedHandler embed = new EmbedHandler(Context.User, "Geting all user ranks...");
            await embed.SendMessage(Context.Channel);
            embed.Title = "List of Ranks";

            DiscordServer server = DiscordServer.GetServerFromID(Context.Guild.Id);
            if(server.animeRoles.Count <= 0 && server.mangaRoles.Count <= 0)
            {
                embed.Title = "This server does not have any anime and manga ranks. (For administrators: Use `" + Program.botPrefix + "addrank`";
                await embed.UpdateEmbed();
                return;
            }

            // ANIME

            if (server.animeRoles.Count > 0)
            {
                (ulong roleId, decimal days) animeRank = server.animeRoles[0];
                IRole animeRole = Context.Guild.GetRole(animeRank.roleId);

                string animeRoleList = animeRole.Name + " for " + animeRank.days + " days";
                for (int i = 1; i < server.animeRoles.Count; i++)
                {
                    animeRank = server.animeRoles[i];
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

            if (server.mangaRoles.Count > 0)
            {
                (ulong roleId, decimal days) mangaRank = server.mangaRoles[0];
                IRole mangaRole = Context.Guild.GetRole(mangaRank.roleId);

                string mangaRoleList = mangaRole.Name + " for " + mangaRank.days + " days";
                for (int i = 1; i < server.mangaRoles.Count; i++)
                {
                    mangaRank = server.mangaRoles[i];
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
