using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Net;
using AnimeListBot.Modules;

namespace AnimeListBot.Handler
{
    class CommandHandlingService
    {
        private readonly CommandService _commands;
        private readonly DiscordShardedClient _discord;
        private readonly IServiceProvider _services;

        public CommandHandlingService(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _discord = services.GetRequiredService<DiscordShardedClient>();
            _services = services;

            _commands.CommandExecuted += CommandExecutedAsync;
            _commands.Log += LogAsync;
            _discord.MessageReceived += MessageReceivedAsync;
        }

        public async Task InitializeAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            // Ignore system messages, or messages from other bots
            if (!(rawMessage is SocketUserMessage message))
                return;
            if (message.Source != MessageSource.User)
                return;

            try
            {
                IDMChannel dmChannel = await message.Author.GetOrCreateDMChannelAsync();
                if (message.Channel.Id == dmChannel?.Id) return;
            }
            catch (HttpException) { return; }

            ulong guildId = ((IGuildChannel)message.Channel).Guild.Id;
            DiscordServer server = DatabaseRequest.GetServerById(guildId);
            if (message?.Channel?.Id == server?.RegisterChannelId)
            {
                await DiscordUser.CheckAndCreateUser(message.Author.Id);
                await AutoAdder.AddUser(message, server);
                return;
            }

            // This value holds the offset where the prefix ends
            var argPos = 0;
            if (!message.HasStringPrefix(server.Prefix, ref argPos))
                return;

            // A new kind of command context, ShardedCommandContext can be utilized with the commands framework
            var context = new ShardedCommandContext(_discord, message);
            await _commands.ExecuteAsync(context, argPos, _services);
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            // command is unspecified when there was a search failure (command not found); we don't care about these errors
            if (!command.IsSpecified)
                return;

            if (result.IsSuccess)
            {
                await Stats.CommandUsed();
                return;
            }

            DiscordServer server = DatabaseRequest.GetServerById(context.Guild.Id);

            if (result is ExecuteResult executeResult)
            {
                if (executeResult.Exception == null) return;

                string errorMessage = "Command Error: " + result.ErrorReason;
                EmbedHandler embed = new EmbedHandler(context.Message.Author, errorMessage);
                await embed.SendMessage(context.Channel);
                await Program._logger.LogError(command.GetValueOrDefault(), context, result);
            }

            if (result is ParseResult)
            {
                string message = context.Message.Content;
                message = message.Remove(0, server.Prefix.Length);
                message = message.Split(" ")[0];
                EmbedHandler embed = HelpModule.GetCommandHelp(message, context);
                await embed.SendMessage(context.Channel);
            }
        }

        private async Task LogAsync(LogMessage log)
        {
            await Program._logger.Log(log.ToString());
        }
    }
}
