using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

using JikanDotNet;

using AnimeListBot.Handler;
using AnimeListBot.Modules;
using System.Threading;
using System.Net.Http;
using Discord.Net;

namespace AnimeListBot
{
    public class Program
    {
        public const ulong botID = 515269277553655823;
        public const string botPrefix = ".";
        public static readonly string inviteURL = "https://discordapp.com/api/oauth2/authorize?client_id=" + botID + "&permissions=0&scope=bot";
        
        public static Color embedColor = new Color(114, 137, 218);
        public const string EMPTY_EMBED_SPACE = "\u200b";

        public static DiscordShardedClient _client;
        public static IServiceProvider _services;

        public static Logger _logger;

        public static IJikan _jikan;

        public static readonly HttpClient httpClient = new HttpClient();

        public static string[] botOwners;
        public static string currentCommit;
        public static string gitStatus;

        public static bool stop = false;
        public static bool firstInitilized = false;

        public static DateTime BOT_START_TIME { get; private set; }

        private static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

        public async Task RunBotAsync()
        {
            BOT_START_TIME = DateTime.Now;

            _logger = new Logger();

            string botToken = "";
            // Get bot token
            if (File.Exists("botToken.txt"))
            {
                botToken = File.ReadAllText("botToken.txt");
            }
            if (string.IsNullOrEmpty(botToken))
            {
                await _logger.LogError("Bot Token does not exist, make sure its correct in the botToken.txt file");
                return;
            }

            if (File.Exists("botOwners.txt"))
            {
                botOwners = File.ReadAllLines("botOwners.txt");
            }

            if (File.Exists("current_commit.txt"))
            {
                currentCommit = File.ReadAllText("current_commit.txt");
            }

            if (File.Exists("git_status.txt"))
            {
                gitStatus = File.ReadAllText("git_status.txt");
            }

            _jikan = new Jikan(true);

            var config = new DiscordSocketConfig
            {
                TotalShards = 2
            };

            using (var services = ConfigureServices(config))
            {
                _services = services;
                _client?.Dispose();
                _client = services.GetRequiredService<DiscordShardedClient>();


                _client.Log += Log;
                _client.ShardReady += OnReadyAsync;
                _client.JoinedGuild += OnJoinedGuild;
                _client.LeftGuild += OnLeftGuild;

                _client.ReactionAdded += OnReactionAdded;

                await services.GetRequiredService<CommandHandlingService>().InitializeAsync();

                await _client.LoginAsync(TokenType.Bot, botToken);

                await _client.StartAsync();

                while (!stop)
                {
                    await Task.Delay(20);
                }
            }
            await _logger.Log("Stopping Bot...");
            return;
        }

        private ServiceProvider ConfigureServices(DiscordSocketConfig config)
        {
            return new ServiceCollection()
                .AddSingleton(new DiscordShardedClient(config))
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .BuildServiceProvider();
        }

        public async Task OnReadyAsync(DiscordSocketClient shard)
        {
            Console.WriteLine($"Shard Number {shard.ShardId} is connected and ready!");
            await Stats.LoadStats();
        }

        public async Task OnJoinedGuild(SocketGuild guild)
        {
            if ((DatabaseRequest.GetServerById(guild.Id)) == null)
            {
                await DatabaseRequest.CreateServer(new DiscordServer(guild));
            }
        }

        private async Task OnLeftGuild(SocketGuild arg)
        {
            DiscordServer server = DatabaseRequest.GetServerById(arg.Id);
            if(server != null)
            {
                await DatabaseRequest.RemoveServer(server);
            }
        }

        private Task OnReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if (arg3.UserId == _client.CurrentUser.Id) return Task.CompletedTask;

            EmbedHandler.ExecuteAnyEmoteAction(arg3);
            return Task.CompletedTask;
        }

        private async Task Log(LogMessage arg)
        {
            if (arg.Exception != null)
            {
                await _logger.LogError(arg);
                return;
            }
            await _logger.Log(arg.Message);
        }
    }
}
