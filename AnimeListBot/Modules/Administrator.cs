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
using Discord.Commands;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using AnimeListBot.Handler;
using AnimeListBot.Handler.Anilist;
using System.Globalization;
using Discord.WebSocket;
using Discord;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using JikanDotNet;
using AnimeListBot.Handler.Database;
using Microsoft.Extensions.Logging;

namespace AnimeListBot.Modules
{
    [RequireOwner]
    public class Administrator : ModuleBase<ShardedCommandContext>
    {
        private IDatabaseService _db;
        private ILogger<BotLogger> _logger;

        public Administrator(IDatabaseService db, ILogger<BotLogger> logger)
        {
            _db = db;
            _logger = logger;
        }

        [Command("stop")]
        public async Task StopBot()
        {
            EmbedHandler embed = new EmbedHandler(Context.User);
            await Program._logger.Log("Stopping bot... Command run by: " + Context.User.Username);

            embed.Title = "Stopping Bot.";
            await embed.SendMessage(Context.Channel);

            Program.stop = true;
            await Program._client.StopAsync();
        }

        [Command("ignoreexception")]
        [Summary(
            "Ignores an exception from dm'ing the owner.\n" +
            "   Options:\n" +
            "   - add\n" +
            "   - remove\n" +
            "   - view/list\n"
        )]
        public async Task IgnoreException(string option = "list", [Remainder]string ignore = "")
        {
            EmbedHandler embed = new EmbedHandler(Context.User);
            Config config = Config.GetConfig();
            switch (option) {
                case "add":
                    if (string.IsNullOrEmpty(ignore))
                    {
                        embed.Title = "Cant add an empty string for ignoring exception";
                        break;
                    }
                    config.ignoredExceptionMessages.Add(ignore);
                    embed.Title = "Exception Message Ignored: " + ignore;
                    break;
                case "view":
                case "list":
                    string message = string.Empty;
                    if (config.ignoredExceptionMessages.Count <= 0)
                    {
                        embed.Title = "There are no ignored exceptions to view.";
                        break;
                    }
                    config.ignoredExceptionMessages.ToList().ForEach(x => message += x + "\n");
                    embed.Title = "Ignored Exeptions.";
                    embed.AddFieldSecure("List", message);
                    break;
                case "remove":
                    if (config.ignoredExceptionMessages.Remove(ignore)) embed.Title = "Exception Removed from ignorelist: " + ignore;
                    else embed.Title = "Failed to remove from ignorelist: " + ignore;
                    break;
                default:
                    embed.Title = "Incorrect command arguments (add, view/list, remove).";
                    break;
            }
            config.Save();
            await embed.SendMessage(Context.Channel);
        }

        [Command("errortest")]
        public async Task SendErrorTest([Remainder]string message)
        {
            EmbedHandler embed = new EmbedHandler(Context.User);
            embed.Title = "Sent fake error.";
            await embed.SendMessage(Context.Channel);
            throw new Exception(message);
        }

        [Command("sendmessage")]
        public async Task SendMessage(string title, string message)
        {
            EmbedHandler embed = new EmbedHandler(Context.User);
            embed.Title = title;
            embed.Description = message;
            await embed.SendMessage(Context.Channel);
        }

        [Command("setgamestatus")]
        [Summary(
            "Setting Game Status for the bot" +
            "\n    Playing = 0," +
            "\n    Streaming = 1," +
            "\n    Listening = 2," +
            "\n    Watching = 3"
        )]
        public async Task SetGameStatus(Discord.ActivityType activityType, string gameMessage)
        {
            EmbedHandler embed = new EmbedHandler(Context.User, "Set game status to: " + Enum.GetName(typeof(Discord.ActivityType), activityType) + " " + gameMessage);
            await Program._client.SetGameAsync(gameMessage, null, activityType);
            await embed.SendMessage(Context.Channel);
        }

        [Command("setonlinestatus")]
        [Summary(
            "Setting User Status for the Bot" +
            "\n    Status:" +
            "\n    Offline = 0," +
            "\n    Online = 1," +
            "\n    Idle = 2," +
            "\n    AFK = 3," +
            "\n    DoNotDisturb = 4," +
            "\n    Invisible = 5"
        )]
        public async Task SetOnlineStatus(Discord.UserStatus status)
        {
            EmbedHandler embed = new EmbedHandler(Context.User, "Set online status to: " + Enum.GetName(typeof(Discord.UserStatus), status));
            await Program._client.SetStatusAsync(status);
            await embed.SendMessage(Context.Channel);
        }

        [Command("anilimit")]
        public async Task GetAniLimit()
        {
            EmbedHandler embed = new EmbedHandler(Context.User, "Retrieving Anilist Headers...");
            await embed.SendMessage(Context.Channel);
            
            AniHeaderResult result = await AniHeaderCheck.CheckHeaders();

            CultureInfo en_US = new CultureInfo("en-US");

            embed.Title = "";
            embed.AddFieldSecure(
                "**Anilist Headers**",
                "**Rate Limit:** " + result.RateLimit_Limit + "\n" +
                "**Remaining Rate Limit:** " + result.RateLimit_Remaining + "\n" +
                (result.RateLimit_Reset.HasValue ? "**Rate Limit Reset:** " + result.RateLimit_Reset.Value.ToString("HH:mm:ss", en_US) + " UTC" : "") +
                (result.RetryAfter > 0 ? "**Retry After:** " + result.RetryAfter + " seconds.\n" : "")
            );
            await embed.UpdateEmbed();
        }

        [Command("updateallguilds")]
        public async Task UpdateAllGuilds()
        {
            EmbedHandler embed = new EmbedHandler(Context.User, "Updating All Guilds");
            await embed.SendMessage(Context.Channel);

            List<DiscordServer> guilds = _db.GetAllServers();
            for (int guildIndex = 0; guildIndex < guilds.Count; guildIndex++)
            {
                SocketGuild guild = Program._client.GetGuild(guilds[guildIndex].ServerId);
                DiscordServer server = await _db.GetServerById(guild.Id);
                server.UpdateGuildInfo(guild);

                server.ranks.MangaRanks.ForEach(x => x.UpdateRank(guild));
                server.ranks.AnimeRanks.ForEach(x => x.UpdateRank(guild));
                server.UpdateGuildRoles();
            }

            await _db.SaveChangesAsync();
            embed.Title = "All guilds are updated";
            await embed.UpdateEmbed();
        }

        [Command("updateallusers")]
        public async Task UpdateAllUsers()
        {
            EmbedHandler embed = new EmbedHandler(Context.User, "Updating All Users");
            await embed.SendMessage(Context.Channel);

            List<DiscordUser> users = _db.GetAllUsers();
            for (int userIndex = 0; userIndex < users.Count; userIndex++)
            {
                DiscordUser user = users[userIndex];
                user.RefreshMutualGuilds();
            }

            await _db.SaveChangesAsync();
            embed.Title = "All users are updated";
            await embed.UpdateEmbed();
        }

        [Command("testcommand")]
        public async Task TestCommand()
        {
            EmbedHandler embed = new EmbedHandler(Context.User, "Current Prefix");

            DiscordServer server = await _db.GetServerById(Context.Guild.Id);
            embed.Description = server.Prefix;

            await _db.SaveChangesAsync();
            await embed.SendMessage(Context.Channel);
        }
    }
}
