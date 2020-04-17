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

namespace AnimeListBot.Modules
{
    public class Administrator : ModuleBase<ShardedCommandContext>
    {
        [Command("stop")]
        public async Task StopBot()
        {
            EmbedHandler embed = new EmbedHandler(Context.User);
            if(Program.botOwners.Contains(Context.User.Id))
            {
                await Program._logger.Log("Stopping bot... Command run by: " + Context.User.Username);

                embed.Title = "Stopping Bot.";
                await embed.SendMessage(Context.Channel);

                Program.stop = true;
                await Program._client.StopAsync();
            }
            else{
                embed.Title = "You dont have permission to do this command.";
                await embed.SendMessage(Context.Channel);
            }
        }

        [Command("ignoreexception")]
        [Summary(
            "Ignores an exception from dm'ing the owner.\n" +
            "   Options:\n" +
            "   - add\n" +
            "   - remove\n" +
            "   - view\n"
        )]
        public async Task IgnoreException(string option, [Remainder]string ignore)
        {
            EmbedHandler embed = new EmbedHandler(Context.User);
            if (Program.botOwners.Contains(Context.User.Id))
            {
                Config config = Config.GetConfig();
                switch (option) {
                    case "add":
                        config.ignoredExceptionMessages.Add(ignore);
                        embed.Title = "Exception Message Ignored: " + ignore;
                        break;
                    case "view":
                        string message = string.Empty;
                        config.ignoredExceptionMessages.ToList().ForEach(x => message += x + "\n");
                        embed.Title = "Ignored Exeptions.";
                        embed.AddFieldSecure("List", message);
                        break;
                    case "remove":
                        if (config.ignoredExceptionMessages.Remove(ignore)) embed.Title = "Exception Removed from ignorelist: " + ignore;
                        else embed.Title = "Failed to remove from ignorelist: " + ignore;
                        break;
                }
                Config.OverrideConfig();
            }
            else
            {
                embed.Title = "You dont have permission to do this command.";
                await embed.SendMessage(Context.Channel);
            }
        }

        [Command("errortest")]
        public async Task SendErrorTest([Remainder]string message)
        {
            EmbedHandler embed = new EmbedHandler(Context.User);
            if (Program.botOwners.Contains(Context.User.Id))
            {
                embed.Title = "Sent fake error.";
                await embed.SendMessage(Context.Channel);
                throw new Exception(message);
            }
            else
            {
                embed.Title = "You dont have permission to do this command.";
                await embed.SendMessage(Context.Channel);
            }
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

            if (Program.botOwners.Contains(Context.User.Id))
            {
                await Program._client.SetGameAsync(gameMessage, null, activityType);
                await embed.SendMessage(Context.Channel);
            }
            else
            {
                embed.Title = "You dont have permission to do this command.";
                await embed.SendMessage(Context.Channel);
            }
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

            if (Program.botOwners.Contains(Context.User.Id))
            {
                await Program._client.SetStatusAsync(status);
                await embed.SendMessage(Context.Channel);
            }
            else
            {
                embed.Title = "You dont have permission to do this command.";
                await embed.SendMessage(Context.Channel);
            }
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

        [Command("gitstatus")]
        public async Task GitStatus()
        {
            EmbedHandler embed = new EmbedHandler(Context.User, "Git Status");

            string commitText = "None";
            if (!string.IsNullOrEmpty(Program.currentCommit))
            {
                commitText = "[" + Program.currentCommit.Substring(0, 7) + "](" +
                             "https://github.com/ThomasAunvik/AnimeListBot/commit/" + Program.currentCommit + ")";
            }

            embed.AddFieldSecure("Commit", commitText);
            embed.AddFieldSecure("Status", string.IsNullOrEmpty(Program.gitStatus) ? "None" : Program.gitStatus);
            await embed.SendMessage(Context.Channel);
        }

        [Command("prefix")]
        public async Task Prefix(string newPrefix = "")
        {
            EmbedHandler embed = new EmbedHandler(Context.User);
            DiscordServer server = DatabaseRequest.GetServerById(Context.Guild.Id);

            IGuildUser user = Context.Guild.GetUser(Context.User.Id);
            if (!user.GuildPermissions.Administrator)
            {
                if (!Program.botOwners.Contains(Context.User.Id))
                {
                    newPrefix = string.Empty;
                }
            }

            if (string.IsNullOrEmpty(newPrefix))
            {
                embed.Title = "Current Prefix";
                embed.Description = "`" + server.Prefix + "`";
                await embed.SendMessage(Context.Channel);
                return;
            }

            if (newPrefix.Length > 2)
            {
                embed.Title = "Prefix length is too large (Max 2 characters)";
                await embed.SendMessage(Context.Channel);
                return;
            }

            server.Prefix = newPrefix;
            await server.UpdateDatabase();

            embed.Title = "Prefix Set to";
            embed.Description = "`" + server.Prefix + "`";
            await embed.SendMessage(Context.Channel);
        }
    }
}
