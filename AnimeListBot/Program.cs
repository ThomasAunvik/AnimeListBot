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

namespace AnimeListBot
{
    public class Program
    {
        public const ulong botID = 515269277553655823;
        public const string botPrefix = ".";
        public static readonly string inviteURL = "https://discordapp.com/api/oauth2/authorize?client_id=" + botID + "&permissions=0&scope=bot";
        
        public static Color embedColor = new Color(114, 137, 218);
        public const string EMPTY_EMBED_SPACE = "\u200b";

        public static DiscordSocketClient _client;
        public static CommandService _commands;
        public static IServiceProvider _services;

        public static Logger _logger;

        public static IJikan _jikan;

        public static readonly HttpClient httpClient = new HttpClient();

        public static List<DiscordServer> discordServers;
        public static List<GlobalUser> globalUsers;

        public static string[] botOwners;
        public static string currentCommit;
        public static string gitStatus;

        public static bool stop = false;

        public static DateTime BOT_START_TIME { get; private set; }

        public async Task OnJoinedGuild(SocketGuild guild)
        {
            DiscordServer newServer = new DiscordServer(guild);
            discordServers.Add(newServer);
            try
            {
                newServer.isUpdatingRoles = true;
                await Ranks.UpdateUserRoles(newServer, null);
                newServer.isUpdatingRoles = false;
            }
            catch (Exception e)
            {
                newServer.isUpdatingRoles = false;
                await _logger.LogError(e);
            }
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
            await Stats.LoadStats();

            globalUsers = new List<GlobalUser>();
            discordServers = new List<DiscordServer>();
            foreach (SocketGuild guild in _client.Guilds)
            {
                DiscordServer newServer = new DiscordServer(guild);
                discordServers.Add(newServer);

                foreach (SocketUser user in newServer.Guild.Users)
                {
                    if (!user.IsBot)
                    {
                        if (globalUsers.Find(x => x.userID == user.Id) == null)
                            globalUsers.Add(new GlobalUser(user));
                    }
                }

                try
                {
                    newServer.isUpdatingRoles = true;
                    await Ranks.UpdateUserRoles(newServer, null);
                    newServer.isUpdatingRoles = false;
                }
                catch (Exception e)
                {
                    newServer.isUpdatingRoles = false;
                    await _logger.LogError(e);
                }
            }
        }

        public async Task OnUserJoined(SocketGuildUser user)
        {
            DiscordServer server = DiscordServer.GetServerFromID(user.Guild.Id);
            if (server != null)
            {
                server.Users.Add(new ServerUser(user));
            }

            if(globalUsers.Find(x => x.userID == user.Id) != null)
            {
                GlobalUser gUser = new GlobalUser(user);
                globalUsers.Add(gUser);
            }

            await Ranks.UpdateUserRole(user, null);
        }

        public async Task RegisterCommandsAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            _commands.CommandExecuted += OnCommandExecuted;
            _client.MessageReceived += HandleCommandAsync;
        }

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

            _client?.Dispose();
            _client = new DiscordSocketClient();

            (_commands as IDisposable)?.Dispose();
            _commands = new CommandService();

            (_services as IDisposable)?.Dispose();
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

            _client.ReactionAdded += OnReactionAdded;
            
            await RegisterCommandsAsync();
            
            await _client.LoginAsync(TokenType.Bot, botToken);

            await _client.StartAsync();

            while (!stop)
            {
                await Task.Delay(20);
            }

            await _logger.Log("Stopping Bot...");
            return;
        }

        private Task OnReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            EmbedHandler.ExecuteAnyEmoteAction(arg3.Emote, arg1.Value);
            return Task.CompletedTask;
        }

        private static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            try
            {
                var message = arg as SocketUserMessage;
                if (message is null || message.Author.IsBot) return;
                
                IDMChannel dmChannel = await arg.Author.GetOrCreateDMChannelAsync();
                if (arg.Channel.Id == dmChannel?.Id) return;

                GlobalUser user = globalUsers.FirstOrDefault(x => x.userID == arg.Author.Id);
                if (user == default(GlobalUser)) globalUsers.Add(new GlobalUser(arg.Author));
                
                ulong guildId = ((IGuildChannel)arg.Channel).Guild.Id;
                DiscordServer server = DiscordServer.GetServerFromID(guildId);
                if (arg?.Channel?.Id == server?.animeListChannelId)
                {
                    await AutoAdder.AddUser(arg);
                }

                int argPos = 0;
                if (message.HasStringPrefix(botPrefix, ref argPos))
                {
                    var context = new SocketCommandContext(_client, message);
                    var result = await _commands.ExecuteAsync(context, argPos, _services);
                }
            }catch(Exception e)
            {
                await _logger.LogError(e, arg.Author);
            }
        }

        private async Task OnCommandExecuted(Optional<CommandInfo> info, ICommandContext context, IResult result)
        {
            await Stats.CommandUsed();

            if (result is ExecuteResult)
            {
                ExecuteResult executeResult = (ExecuteResult)result;
                if (executeResult.Exception != null)
                {
                    string errorMessage = "Command Error: " + result.ErrorReason;
                    EmbedHandler embed = new EmbedHandler(context.Message.Author, errorMessage);
                    await embed.SendMessage(context.Channel);
                    await _logger.LogError(info.GetValueOrDefault(), context, result);
                }
            }

            if(result is ParseResult)
            {
                ParseResult parseResult = (ParseResult)result;
                if (!parseResult.IsSuccess)
                {
                    string message = context.Message.Content;
                    message = message.Remove(0, botPrefix.Length);
                    message = message.Split(" ")[0];
                    EmbedHandler embed = HelpModule.GetCommandHelp(message, context);
                    await embed.SendMessage(context.Channel);
                }
            }
        }

        private async Task Log(LogMessage arg)
        {
            await _logger.Log(arg.Message);
            if (arg.Exception != null)
            {
                await _logger.LogError(arg.Exception);
            }
        }
    }
}
