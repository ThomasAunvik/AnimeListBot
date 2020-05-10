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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

using JikanDotNet;

using AnimeListBot.Handler;
using AnimeListBot.Modules;
using System.Net.Http;
using DiscordBotsList.Api;
using AnimeListBot.Handler.Database;
using Microsoft.EntityFrameworkCore;

namespace AnimeListBot
{
    public class Program
    {
        public const ulong botID = 515269277553655823;
        public const string botPrefix = "al!";
        public static readonly string inviteURL = "https://discordapp.com/api/oauth2/authorize?client_id=" + botID + "&permissions=0&scope=bot";
        
        public static Color embedColor = new Color(114, 137, 218);
        public const string EMPTY_EMBED_SPACE = "\u200b";

        public static DiscordShardedClient _client;
        public static IServiceProvider _services;
        public static DatabaseService db;

        public static Logger _logger;

        public static IJikan _jikan;

        public static AuthDiscordBotListApi _dbl;

        public static readonly HttpClient httpClient = new HttpClient();

        public static List<ulong> botOwners;
        public static string currentCommit;
        public static string gitStatus;

        public static bool stop = false;
        public static bool firstInitilized = false;

        public static bool TestingMode { get; private set; }

        public static DateTime BOT_START_TIME { get; private set; }

        private static void Main(string[] args) => new Program().RunBotAsync(args).GetAwaiter().GetResult();

        public async Task RunBotAsync(string[] args)
        {
            if (args.Contains("-testing")) TestingMode = true;

            BOT_START_TIME = DateTime.Now;

            _logger = new Logger();


            Config bot_config = Config.GetConfig();
            botOwners = bot_config.bot_owners;

            db = new DatabaseService();
            Cluster cluster = db.Cluster.Find(bot_config.cluster_id);

            if (File.Exists("current_commit.txt"))
            {
                currentCommit = File.ReadAllText("current_commit.txt");
            }

            if (File.Exists("git_status.txt"))
            {
                gitStatus = File.ReadAllText("git_status.txt");
            }

            _jikan = new Jikan(true);
            _dbl = new AuthDiscordBotListApi(botID, bot_config.dbl_token);

            var shard_config = new DiscordSocketConfig
            {
                TotalShards = GetTotalShards(), 
            };

            int[] shardIds = cluster.GetShardIds();
            string shardIdstring = "[ " + shardIds[0].ToString();
            for (int i = 1; i < shardIds.Length; i++) shardIdstring += ", " + shardIds[i].ToString();
            shardIdstring += " ]";

            await _logger.Log("ShardStart: " + cluster.ShardIdStart
                + "\nShardEnd: " + cluster.ShardIdEnd
                + "\nShards: " + shardIdstring);

            using (var services = ConfigureServices(shard_config, cluster))
            {
                _services = services;
                _client?.Dispose();
                _client = services.GetRequiredService<DiscordShardedClient>();

                _client.Log += Log;
                _client.ShardReady += OnReadyAsync;
                _client.JoinedGuild += OnJoinedGuild;
                _client.LeftGuild += OnLeftGuild;
                _client.GuildMemberUpdated += OnGuildMemberUpdated;
                _client.UserLeft += OnUserLeft;
                _client.UserBanned += OnUserBanned;
                _client.RoleUpdated += OnRoleUpdated;

                _client.ReactionAdded += OnReactionAdded;

                await services.GetRequiredService<CommandHandlingService>().InitializeAsync();

                await _client.LoginAsync(TokenType.Bot, bot_config.bot_token);

                await _client.StartAsync();

                while (!stop)
                {
                    await Task.Delay(20);
                }
            }
            await _logger.Log("Stopping Bot...");
            return;
        }

        public int GetTotalShards()
        {
            if (Config.cached.override_shard_amount > 0) return Config.cached.override_shard_amount;

            int total = 0;
            
            db.Cluster.ToList().ForEach(x => total += x.GetShardCount());
            return total;
        }

        private ServiceProvider ConfigureServices(DiscordSocketConfig shard_config, Cluster cluster)
        {
            return new ServiceCollection()
                .AddSingleton(new DiscordShardedClient(cluster.GetShardIds(), shard_config))
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddDbContext<DatabaseService>()
                .BuildServiceProvider();
        }

        int current_ready_shards;
        public async Task OnReadyAsync(DiscordSocketClient shard)
        {
            await _logger.Log($"Shard Number {shard.ShardId} is connected and ready!");
            await BotInfo.LoadStats();

            current_ready_shards++;

            DatabaseService db = _services.GetRequiredService<DatabaseService>();

            Cluster cluster = db.Cluster.Find(Config.cached.cluster_id);
            if (current_ready_shards >= cluster.GetShardCount())
            {
                await _logger.Log("Updated guild count: " + _client.Guilds.Count);
                await _dbl.UpdateStats(_client.Guilds.Count);
            }
        }

        public async Task OnJoinedGuild(SocketGuild guild)
        {
            DatabaseService db = _services.GetRequiredService<DatabaseService>();
            if (!db.DoesServerIdExist(guild.Id))
            {
                await db.CreateServer(new DiscordServer(guild));
            }

            await _logger.Log("Updated guild count: " + _client.Guilds.Count);
            await _dbl.UpdateStats(_client.Guilds.Count);
        }

        private async Task OnLeftGuild(SocketGuild arg)
        {
            DatabaseService db = _services.GetRequiredService<DatabaseService>();
            DiscordServer server = await db.GetServerById(arg.Id);
            if(server != null)
            {
                await db.RemoveServer(server);
            }

            await _logger.Log("Updated guild count: " + _client.Guilds.Count);
            await _dbl.UpdateStats(_client.Guilds.Count);
        }

        private async Task OnGuildMemberUpdated(SocketGuildUser arg1, SocketGuildUser arg2)
        {
            DatabaseService db = _services.GetRequiredService<DatabaseService>();
            if (db.DoesUserIdExist(arg2.Id)){
                DiscordUser user = await db.GetUserById(arg2.Id);
                user.RefreshMutualGuilds();
            }
        }

        private async Task OnUserBanned(SocketUser arg1, SocketGuild arg2)
        {
            DatabaseService db = _services.GetRequiredService<DatabaseService>();
            if (db.DoesUserIdExist(arg2.Id))
            {
                DiscordUser user = await db.GetUserById(arg1.Id);
                user.Servers.RemoveAll(x => x.ServerId == arg2.Id.ToString());
            }
        }

        private async Task OnUserLeft(SocketGuildUser arg)
        {
            DatabaseService db = _services.GetRequiredService<DatabaseService>();
            if (db.DoesUserIdExist(arg.Id))
            {
                DiscordUser user = await db.GetUserById(arg.Id);
                user.Servers.RemoveAll(x => x.ServerId == arg.Guild.Id.ToString());
            }
        }

        private async Task OnRoleUpdated(SocketRole arg1, SocketRole arg2)
        {
            DatabaseService db = _services.GetRequiredService<DatabaseService>();
            DiscordServer server = await db.GetServerById(arg2.Guild.Id);
            server.ranks.UpdateRankPermission(arg2.Id, arg2.Permissions.RawValue);
        }

        private Task OnReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {

            if (_client.CurrentUser == null || arg3.UserId == _client.CurrentUser?.Id) return Task.CompletedTask;

            EmbedHandler.ExecuteAnyEmoteAction(arg3);
            return Task.CompletedTask;
        }

        public static async Task Log(LogMessage log)
        {
            if (log.Exception != null && !Config.cached.ignoredExceptionMessages.Contains(log.Exception.Message))
            {
                await _logger.LogError(log);
                return;
            }
            await _logger.Log(log.Message);
        }
    }
}
