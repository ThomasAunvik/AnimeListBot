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
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Globalization;
using Microsoft.Extensions.Logging;
using AnimeListBot.Handler.Logging;

namespace AnimeListBot.Handler
{
    public class BotLogger : ILogger
    {
        const int MAX_FIELD_VALUE_LENGTH = 1024;

        private readonly string _name;
        private readonly BotLoggerConfiguration _config;

        public BotLogger(string name, BotLoggerConfiguration config)
        {
            _name = name;
            _config = config;
        }

        public void LogDiscordError(string errorMessage, IUser user = null, IGuildChannel guildChannel = null)
        {
            EmbedHandler embed = new EmbedHandler(user, "Error", string.Empty, true);
            if (guildChannel != null)
            {
                embed.AddFieldSecure("Channel Info",
                    "Server Id: " + guildChannel.GuildId +
                    "\nChannel Id: <#" + guildChannel.Id + ">" +
                    "\nUser Id: <@" + user.Id + ">"
                );
            }
            embed.AddFieldSecure("ErrorMessage", errorMessage);
        }

        public void LogDiscordError(LogMessage message)
        {
            EmbedHandler embed = new EmbedHandler(null, "Exception", string.Empty, true);
            embed.AddFieldSecure("Severity", Enum.GetName(typeof(LogSeverity), message.Severity));
            embed.AddFieldSecure("Source", message.Source);
            embed.AddFieldSecure("Exception Message", message.Exception.Message);
            if (message.Exception.StackTrace != null)
            {
                embed.AddFieldSecure("Type", message.Exception.GetType().FullName);
                embed.AddFieldSecure("Stacktrace", TrancuateStacktrace(message.Exception.StackTrace));
            }
        }

        public void LogDiscordError(Exception exception, IUser user = null, IGuildChannel guildChannel = null)
        {
            EmbedHandler embed = new EmbedHandler(user, "Exception", string.Empty, true);
            if (guildChannel != null)
            {
                embed.AddFieldSecure("Channel Info",
                    "Server Id: " + guildChannel.GuildId +
                    "\nChannel Id: <#" + guildChannel.Id + ">" +
                    "\nUser Id: <@" + user.Id + ">"
                );
            }
            embed.AddFieldSecure("Exception Message", exception.Message);
            embed.AddFieldSecure("Type", exception.GetType().FullName);
            embed.AddFieldSecure("Stacktrace", TrancuateStacktrace(exception.StackTrace));
        }

        public void LogDiscordError(CommandInfo info, ICommandContext context, IResult result)
        {
            if (result is ExecuteResult)
            {
                Exception e = ((ExecuteResult)result).Exception;

                EmbedHandler embed = new EmbedHandler(context.User, "Command Exception", string.Empty, true);
                embed.AddFieldSecure("Channel Info",
                    "Server Id: " + ((IGuildChannel)context.Channel).GuildId +
                    "\nChannel Id: <#" + context.Channel.Id + ">" +
                    "\nUser Id: <@" + context.User.Id + ">"
                );
                embed.AddFieldSecure("Command Used", context.Message.Content);
                embed.AddFieldSecure("Exception Message", e.Message);
                embed.AddFieldSecure("Type", e.GetType().FullName);
                embed.AddFieldSecure("Stacktrace", TrancuateStacktrace(e.StackTrace));
            }
        }

        public string TrancuateStacktrace(string input)
        {
            if (input == null) return "";

            input = Format.Sanitize(input);
            if (input.Length > MAX_FIELD_VALUE_LENGTH)
            {
                input = input.Substring(0, MAX_FIELD_VALUE_LENGTH);
            }
            return input;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }
            EmbedHandler embed = new EmbedHandler(null);
            embed.Title = state.ToString();
            formatter.Invoke(state, exception);


            LogToFile(state.ToString());

            if (_config.SendToOwner)
            {
                if (Program._client == null) return;

                const ulong ownerId = 96580514021912576;
                ulong channelId = Program.TestingMode ? Config.cached.test_error_channel : Config.cached.error_channel;
                SocketUser owner = Program._client.GetUser(ownerId);

                SocketTextChannel channel = (SocketTextChannel)Program._client.GetChannel(channelId);

                if (owner != null)
                {
                    owner?.GetOrCreateDMChannelAsync();
                    owner.SendMessageAsync("", false, embed.Build());
                }

                if (channel != null)
                {
                    channel.SendMessageAsync("", false, embed.Build());
                }
            }
        }

        public void TryCreateLogFile()
        {
            if (!Directory.Exists(_config.FileDirectory))
            {
                Directory.CreateDirectory(_config.FileDirectory);
            }
            if (Directory.Exists(_config.FileDirectory))
            {
                using (FileStream logStream = File.Create(_config.FileDirectory + "/" + _config.FileName))
                {
                    logStream.Close();
                }
            }
        }

        public void LogToFile(string message)
        {
            try
            {
                DateTime localTime = DateTime.Now;
                string dateTimeMessage = "[" + localTime.ToString("T", DateTimeFormatInfo.InvariantInfo) + "] " + message;
                string path = _config.FileDirectory + "/" + _config.FileName;
                Console.WriteLine(dateTimeMessage);

                using StreamWriter writer = new StreamWriter(path, true);
                writer.WriteLine(dateTimeMessage);
            }
            catch (IOException e)
            {
                Console.WriteLine(e);
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel == _config.LogLevel;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }
}
