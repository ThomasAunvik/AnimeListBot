﻿/*
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
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Net;
using AnimeListBot.Modules;
using System.Linq;
using System.Collections.Generic;
using AnimeListBot.Handler.Database;
using GraphQL;

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
            _commands.Log += Program.Log;
            _discord.ShardReady += Ready;
            _discord.MessageReceived += MessageReceivedAsync;
        }

        public async Task InitializeAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private Task Ready(DiscordSocketClient arg)
        {
            return Task.CompletedTask;
        }

        public Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            // Ignore system messages, or messages from other bots
            if (!(rawMessage is SocketUserMessage message))
                return Task.CompletedTask;
            if (!(message.Channel is IGuildChannel guildChannel))
                return Task.CompletedTask;
            if (message.Source != MessageSource.User)
                return Task.CompletedTask;

            if (Program.TestingMode)
            {
                if (guildChannel.Id != Config.cached.test_channel)
                    return Task.CompletedTask;

                if (!Program.botOwners.Contains(message.Author.Id))
                {
                    return Task.CompletedTask;
                }
            }
            else
            {
                if (guildChannel.Id == Config.cached.test_channel)
                    return Task.CompletedTask;
            }
            Task.Run(async () => {
                using var serviceScope = _services.CreateScope();
                var db = serviceScope.ServiceProvider.GetRequiredService<IDatabaseService>();

                DiscordServer server = await db.GetServerById(guildChannel.GuildId);
                server.UpdateGuildInfo(guildChannel.Guild);

                if (server.stats == null) server.stats = new ServerStatistics();
                if (server.ranks == null) server.ranks = new ServerRanks();

                if (guildChannel.Id.ToString() == server.ranks.RegisterChannelId)
                {
                    DiscordUser user = await db.GetUserById(message.Author.Id);
                    await AutoAdder.AddUser(message, user, server);
                    await db.SaveChangesAsync();
                    return;
                }

                // This value holds the offset where the prefix ends
                var argPos = 0;
            
                if (!(message.HasStringPrefix(server.Prefix, ref argPos) || message.HasMentionPrefix(Program._client.CurrentUser, ref argPos)))
                    return;

                IGuildUser botUser = await guildChannel.Guild.GetCurrentUserAsync();
                ChannelPermissions perm = botUser.GetPermissions(guildChannel);
                if (!perm.SendMessages)
                    return;

                // A new kind of command context, ShardedCommandContext can be utilized with the commands framework
                var context = new ShardedCommandContext(_discord, message);
                await _commands.ExecuteAsync(context, argPos, serviceScope.ServiceProvider);
            });

            return Task.CompletedTask;
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            // command is unspecified when there was a search failure (command not found); we don't care about these errors
            if (!command.IsSpecified)
                return;

            IDatabaseService db = _services.GetRequiredService<IDatabaseService>();

            DiscordUser user = await db.GetUserById(context.Message.Author.Id);
            if (context.Message.Channel is IGuildChannel)
            {
                user.RefreshMutualGuilds();
                await db.SaveChangesAsync();
            }

            DiscordServer server = await db.GetServerById(context.Guild.Id);
            if (result.IsSuccess)
            {
                await BotInfo.CommandUsed();
                if (server.stats == null) server.stats = new ServerStatistics();
                server.stats.CommandsUsed++;
                await db.SaveChangesAsync();
                return;
            }

            if (result is ExecuteResult executeResult)
            {
                string errorMessage = "Command Error";
                EmbedHandler embed = new EmbedHandler(context.Message.Author, errorMessage, result.ErrorReason);
                await embed.SendMessage(context.Channel);

                if (executeResult.Exception == null) return;
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

            if(result.Error == CommandError.UnmetPrecondition)
            {
                string errorMessage = "Missing Permission Error";
                EmbedHandler embed = new EmbedHandler(context.Message.Author, errorMessage, result.ErrorReason);
                await embed.SendMessage(context.Channel);
                
            }
        }
    }
}
