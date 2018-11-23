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

using MALBot.Handler;
using MALBot.Modules;
using System.Threading;

namespace MALBot
{
    public class Program
    {
        public const ulong botID = 515269277553655823;
        public const char botPrefix = '.';
        public static readonly string inviteURL = "https://discordapp.com/api/oauth2/authorize?client_id=" + botID + "&permissions=0&scope=bot";
        
        public static Color embedColor = new Color(114, 137, 218);
        public const string EMPTY_EMBED_SPACE = "\u200b";

        public static DiscordSocketClient _client;
        public static CommandService _commands;
        public static IServiceProvider _services;

        public static IJikan _jikan;

        public static List<DiscordServer> discordServers;
        public static List<GlobalUser> globalUsers;

        public Task OnJoinedGuild(SocketGuild guild)
        {
            discordServers.Add(new DiscordServer(guild));
            return Task.CompletedTask;
        }

        public Task OnLeftGuild(SocketGuild guild)
        {
            DiscordServer server = DiscordServer.GetServerFromID(guild.Id);
            if (server != null)
            {
                DiscordServer.DeleteServerFile(guild);
                discordServers.Remove(server);
            }
            return Task.CompletedTask;
        }

        public async Task OnReadyAsync()
        {
            globalUsers = new List<GlobalUser>();
            discordServers = new List<DiscordServer>();
            foreach (SocketGuild guild in _client.Guilds)
            {
                DiscordServer newServer = new DiscordServer(guild);
                discordServers.Add(newServer);

                foreach (SocketUser user in newServer.Guild.Users)
                    if (!user.IsBot)
                        if (globalUsers.Find(x => x.userID == user.Id) == null)
                            globalUsers.Add(new GlobalUser(user));

                await Ranks.UpdateUserRoles(newServer);
            }
        }

        public Task OnUserJoined(SocketGuildUser user)
        {
            DiscordServer server = DiscordServer.GetServerFromID(user.Guild.Id);
            if (server != null)
            {
                server.Users.Add(new ServerUser(user));
            }
            return Task.CompletedTask;
        }

        public async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        public async Task RunBotAsync()
        {
            string botToken = "";
            // Get bot token
            if (File.Exists("botToken.txt"))
            {
                botToken = File.ReadAllText("botToken.txt");
            }
            if (string.IsNullOrEmpty(botToken))
            {
                Console.WriteLine("Bot Token does not exist, make sure its correct in the botToken.txt file");
                return;
            }
            
            _client = new DiscordSocketClient();
            _commands = new CommandService();

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            _jikan = new Jikan(true);

            _client.Log += Log;

            _client.Ready += OnReadyAsync;
            _client.LeftGuild += OnLeftGuild;
            _client.JoinedGuild += OnJoinedGuild;
            _client.UserJoined += OnUserJoined;

            _client.MessageReceived += async (arg) =>
            {
                if (globalUsers == null)
                    return;

                if (arg is null || arg.Author.IsBot)
                    return;

                GlobalUser user = globalUsers.FirstOrDefault(x => x.userID == arg.Author.Id);
                if (user == default(GlobalUser)) globalUsers.Add(new GlobalUser(arg.Author));

                ulong guildId = ((IGuildChannel)arg.Channel).Guild.Id;
                DiscordServer server = DiscordServer.GetServerFromID(guildId);
                if(arg.Channel.Id == server.animeListChannelId)
                {
                    await AutoAdder.AddUser(arg);
                }
            };

            await RegisterCommandsAsync();
            
            await _client.LoginAsync(TokenType.Bot, botToken);

            await _client.StartAsync();
            
            //Ranks.SetupTimer();

            await Task.Delay(-1);
        }

        private static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            if (message is null || message.Author.IsBot) return;

            
            DiscordServer server = discordServers.Find(x => x.Guild.GetChannel(message.Channel.Id) == message.Channel);
            ServerUser user = server.GetUserFromId(message.Author.Id);

            int argPos = 0;
            if (message.HasStringPrefix(botPrefix.ToString(), ref argPos))
            {
                var context = new SocketCommandContext(_client, message);

                var result = await _commands.ExecuteAsync(context, argPos, _services);

                if (!result.IsSuccess)
                {
                    if (result.ErrorReason != "Unknown command.")
                    {
                        Console.WriteLine(result.ErrorReason);
                        await message.Channel.SendMessageAsync(result.ErrorReason);
                    }
                }
            }
        }

        private Task Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            if (arg.Exception != null)
            {
                Console.WriteLine(arg.Exception.StackTrace);
            }
            return Task.CompletedTask;
        }
    }
}