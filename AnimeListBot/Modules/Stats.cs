using AnimeListBot.Handler;
using Discord.Commands;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;

namespace AnimeListBot.Modules
{
    public class Stats : ModuleBase<ICommandContext>
    {
        struct SavedStats
        {
            public int totalCommands;
        }

        private static SavedStats stats = new SavedStats();
        public static void CommandUsed() { stats.totalCommands++; }

        [Command("stats")]
        [Summary("Shows the stats for the discord bot.")]
        public async Task Uptime()
        {
            DateTime now = DateTime.Now;
            DateTime start = Program.BOT_START_TIME;

            TimeSpan uptime = now - start;

            int totalUserCount = 0;
            Program._client.Guilds.ToList().ForEach(x => { totalUserCount += x.MemberCount; });

            int totalChannelCount = 0;
            Program._client.Guilds.ToList().ForEach(x => { totalChannelCount += x.Channels.Count; });

            Process proc = Process.GetCurrentProcess();
            long memorySize = proc.PrivateMemorySize64;
            float memoryMB = memorySize / 1024 / 1024;

            EmbedHandler embed = new EmbedHandler(Context.User, "Bot Info");
            embed.AddField("Uptime", $"{Math.Round(uptime.TotalHours)} hours, {uptime.Minutes, 0} minutes, {uptime.Seconds} seconds", false);
            embed.AddField("Guilds", Program._client.Guilds.Count + $"\n({totalUserCount/Program._client.Guilds.Count}Avg Users/Guild)", true);
            embed.AddField("Channels", totalChannelCount, true);
            embed.AddField("Users", totalUserCount, false);
            embed.AddField("Total Commands", stats.totalCommands, false);
            embed.AddField("RAM Usage", memoryMB + " MB", false);

            await embed.SendMessage(Context.Channel);
            await SaveStats();
        }

        public static async Task SaveStats()
        {
            try
            {
                string json = JsonConvert.SerializeObject(stats);
                await File.WriteAllTextAsync("stats.bot", json);
            }catch(Exception e)
            {
                await Program._logger.LogError(e);
            }
        }

        public static async Task LoadStats()
        {
            try
            {
                if (File.Exists("stats.bot"))
                {
                    string json = await File.ReadAllTextAsync("stats.bot");
                    stats = JsonConvert.DeserializeObject<SavedStats>(json);
                }
            }catch(Exception e)
            {
                await Program._logger.LogError(e);
            }
        }
    }
}
