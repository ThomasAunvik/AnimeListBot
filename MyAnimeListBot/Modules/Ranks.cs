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
        [Summary("Adds a rank for how many days you have spent with anime/manga (Options: [anime, manga])")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task AddRank(string option, IRole role, decimal days)
        {
            DiscordServer server = DiscordServer.GetServerFromID(Context.Guild.Id);

            if (option == "anime")
            {
                if (server.animeRoles.Contains((role.Id, days)))
                {
                    await ReplyAsync("Already added animerole " + role.Name + " with " + days + " days.");
                    return;
                }
                server.animeRoles.Add((role.Id, days));
                await ReplyAsync("Added " + role.Name + " as the rank for having " + days + " of watching anime");
            }
            else if(option == "manga")
            {
                if (server.mangaRoles.Contains((role.Id, days)))
                {
                    await ReplyAsync("Already added mangarole " + role.Name + " with " + days + " days.");
                    return;
                }
                server.mangaRoles.Add((role.Id, days));
                await ReplyAsync("Added " + role.Name + " as the rank for having " + days + " of reading manga");
            }
            else
            {
                await ReplyAsync("Incorrect use of modes, the modes are: `anime` and `manga`");
            }
            server.SaveData();
            
        }
        [Command("editrank")]
        [Summary("Edits a rank for how many days you have spent with anime/manga (Options: [anime, manga])")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task EditRank(string option, IRole role, decimal newDays)
        {
            DiscordServer server = DiscordServer.GetServerFromID(Context.Guild.Id);

            if (option == "anime")
            {
                int animeRoleIndex = server.animeRoles.FindIndex(x => x.roleId == role.Id);
                if (animeRoleIndex >= 0) {
                    server.animeRoles[animeRoleIndex] = (role.Id, newDays);
                }
                else
                {
                    await ReplyAsync("Failed to change rank, the role is not set from before.");
                    return;
                }
            }
            else if (option == "manga")
            {
                int mangaRoleIndex = server.mangaRoles.FindIndex(x => x.roleId == role.Id);
                if (mangaRoleIndex >= 0)
                {
                    server.mangaRoles[mangaRoleIndex] = (role.Id, newDays);
                }
                else
                {
                    await ReplyAsync("Failed to change rank, the role is not set from before.");
                    return;
                }
            }
            else
            {
                await ReplyAsync("Incorrect use of modes, the modes are: `anime` and `manga`");
                return;
            }
            server.SaveData();
        }

        [Command("removerank")]
        [Summary("Removes a rank (Options: [anime, manga])")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task RemoveRank(string option, IRole role)
        {
            DiscordServer server = DiscordServer.GetServerFromID(Context.Guild.Id);

            if (option == "anime")
            {
                int animeRoleIndex = server.animeRoles.FindIndex(x => x.roleId == role.Id);
                if (animeRoleIndex >= 0)
                {
                    server.animeRoles.RemoveAt(animeRoleIndex);
                }
                else
                {
                    await ReplyAsync("Failed to change rank, the role is not set from before.");
                    return;
                }
            }
            else if (option == "manga")
            {
                int mangaRoleIndex = server.mangaRoles.FindIndex(x => x.roleId == role.Id);
                if (mangaRoleIndex >= 0)
                {
                    server.mangaRoles.RemoveAt(mangaRoleIndex);
                }
                else
                {
                    await ReplyAsync("Failed to remove rank, the role is not set from before.");
                    return;
                }
            }
            else
            {
                await ReplyAsync("Incorrect use of modes, the modes are: `anime` and `manga`");
                return;
            }
            server.SaveData();
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
            // CALCULATING USER INFO

            await gUser.UpdateMALInfo();

            // ANIME
            decimal? animeDays = gUser.daysWatchedAnime;
            ulong animeRoleId = 0;
            decimal currentAnimeDays = 0;
            server.animeRoles.ForEach(role => { if (role.days < animeDays) { currentAnimeDays = role.days; animeRoleId = role.roleId; } });

            // MANGA
            decimal? mangaDays = gUser.daysReadManga;
            ulong mangaRoleId = 0;
            decimal currentMangaDays = 0;
            server.mangaRoles.ForEach(role => { if (role.days < mangaDays) { currentMangaDays = role.days; mangaRoleId = role.roleId; } });

            // GETTING USER INFO

            Console.WriteLine("Getting userid: " + gUser.userID + " from: " + gUser.Username);
            IGuildUser guildUser = server.Guild.GetUser(gUser.userID);

            // DELETING ROLES

            var delAnimeRoles = server.animeRoles.ToList();
            delAnimeRoles.Remove((animeRoleId, currentAnimeDays));
            delAnimeRoles.ToList().ForEach(x => { if (!guildUser.RoleIds.Contains(x.roleId)) delAnimeRoles.Remove(x); });

            var delMangaRoles = server.mangaRoles.ToList();
            delMangaRoles.Remove((mangaRoleId, currentMangaDays));
            delMangaRoles.ToList().ForEach(x => { if (!guildUser.RoleIds.Contains(x.roleId)) delMangaRoles.Remove(x); });

            var rolesToDelete = delAnimeRoles.Select(z => server.Guild.GetRole(z.roleId)).ToList();
            delMangaRoles.ForEach(x => { rolesToDelete.Add(server.Guild.GetRole(x.roleId)); });

            rolesToDelete.ForEach(x => { Console.WriteLine(gUser.Username + " lost rank " + x.Name); });
            await guildUser.RemoveRolesAsync(rolesToDelete);

            // ADDING ROLES

            if (animeRoleId != 0 && animeRoleId != sUser.currentAnimeRankId)
            {
                if (!guildUser.RoleIds.Contains(animeRoleId))
                {
                    IRole role = server.Guild.GetRole(animeRoleId);
                    await guildUser.AddRoleAsync(role);
                    Console.WriteLine(gUser.Username + " got anime rank " + role.Name + " in server " + server.Guild.Name);
                }
                sUser.currentAnimeRankId = animeRoleId;
            }

            if (mangaRoleId != 0 && mangaRoleId != sUser.currentMangaRankId)
            {
                if (!guildUser.RoleIds.Contains(mangaRoleId))
                {
                    IRole role = server.Guild.GetRole(mangaRoleId);
                    await guildUser.AddRoleAsync(role);
                    Console.WriteLine(gUser.Username + " got manga rank " + role.Name + " in server " + server.Guild.Name);
                }
                sUser.currentMangaRankId = mangaRoleId;
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
            if(server.animeRoles.Count <= 0 && server.mangaRoles.Count <= 0)
            {
                await ReplyAsync("This server does not have any anime and manga ranks. (For administrators: Use `" + Program.botPrefix + "addrank`");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder() {
                Title = "List of Ranks",
                Color = Program.embedColor
            };

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
                embed.AddField(new EmbedFieldBuilder()
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
                embed.AddField(new EmbedFieldBuilder()
                {
                    Name = "Manga",
                    Value = mangaRoleList
                });
            }

            await ReplyAsync("", false, embed.Build());
        }
    }
}
