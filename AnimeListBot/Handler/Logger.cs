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

namespace AnimeListBot.Handler
{
    public class Logger
    {
        const int MAX_FIELD_VALUE_LENGTH = 1024;

        string logPath;
        int logLines = 0;

        StreamWriter writer;

        public Logger()
        {
            if (!Directory.Exists("logs"))
            {
                Directory.CreateDirectory("logs");
            }
            if (Directory.Exists("logs"))
            {
                DateTime localTime = DateTime.Now;
                string logPath = "logs/" + localTime.Year + "-" + localTime.Month + "-" + localTime.Day + "-" + localTime.Hour + "-" + localTime.Minute + "-" + localTime.Second + ".log";
                this.logPath = logPath;
                using (FileStream logStream = File.Create(logPath))
                {
                    logStream.Close();
                }

                writer = new StreamWriter(logPath, true);
            }
        }

        public async Task ReplaceLine(int line, string text)
        {
            if(line < 0)
            {
                await Log(text);
                return;
            }

            int cursorPos = Console.CursorTop;
            Console.SetCursorPosition(0, line);
            Console.Write(text);
            Console.SetCursorPosition(0, cursorPos);
        }

        public async Task Log(string message)
        {
            try
            {
                await writer.WriteAsync((logLines == 0 ? "" : writer.NewLine) + message);
                Console.WriteLine(message);
                logLines++;
            }catch(IOException e)
            {
                Console.WriteLine(e);   
            }
        }

        public async Task LogError(string errorMessage, EmbedBuilder sendEmbed)
        {
            await Log(errorMessage);
            if (Program._client == null) return;

            const ulong ownerId = 96580514021912576;
            const ulong channelId = 701267750214959164;
            SocketUser owner = Program._client.GetUser(ownerId);
            IGuildChannel channel = (IGuildChannel)Program._client.GetChannel(channelId);

            if (owner != null)
            {
                await owner?.GetOrCreateDMChannelAsync();
                await owner.SendMessageAsync("", false, sendEmbed.Build());
            }

            if(channel != null)
            {
                //await channel.("", false, sendEmbed.Build());
            }
        }

        public async Task LogError(string errorMessage, IUser user = null, IGuildChannel guildChannel = null)
        {
            EmbedHandler embed = new EmbedHandler(user, "Error", string.Empty, true);
            if (guildChannel != null)
            {
                embed.AddFieldSecure("Channel Info",
                    "Server Id: " + guildChannel.GuildId +
                    "\nChannel Id: <#" + guildChannel.Id + ">"
                );
            }
            embed.AddFieldSecure("ErrorMessage", errorMessage);
            await LogError(errorMessage, embed);
        }

        public async Task LogError(Exception exception, EmbedBuilder embed)
        {
            string errorMessage = exception.Message + "\n" + exception.StackTrace;
            await LogError(errorMessage, embed);
        }

        public async Task LogError(LogMessage message)
        {
            EmbedHandler embed = new EmbedHandler(null, "Exception", string.Empty, true);
            embed.AddFieldSecure("Severity", Enum.GetName(typeof(LogSeverity), message.Severity));
            embed.AddFieldSecure("Source", message.Source);
            embed.AddFieldSecure("Exception Message", message.Exception.Message);
            embed.AddFieldSecure("Type", message.Exception.GetType().FullName);
            embed.AddFieldSecure("Stacktrace", TrancuateStacktrace(message.Exception.StackTrace));
            await LogError(message.Exception, embed);
        }

        public async Task LogError(Exception exception, IUser user = null, IGuildChannel guildChannel = null)
        {
            EmbedHandler embed = new EmbedHandler(user, "Exception", string.Empty, true);
            if (guildChannel != null)
            {
                embed.AddFieldSecure("Channel Info",
                    "Server Id: " + guildChannel.GuildId +
                    "\nChannel Id: <#" + guildChannel.Id + ">"
                );
            }
            embed.AddFieldSecure("Exception Message", exception.Message);
            embed.AddFieldSecure("Type", exception.GetType().FullName);
            embed.AddFieldSecure("Stacktrace", TrancuateStacktrace(exception.StackTrace));
            await LogError(exception, embed);
        }

        public async Task LogError(CommandInfo info, ICommandContext context, IResult result)
        {
            if(result is ExecuteResult)
            {
                Exception e = ((ExecuteResult)result).Exception;

                EmbedHandler embed = new EmbedHandler(context.User, "Command Exception", string.Empty, true);
                embed.AddFieldSecure("Channel Info",
                    "Server Id: " + ((IGuildChannel)context.Channel).GuildId +
                    "\nChannel Id: <#" + context.Channel.Id + ">"
                );
                embed.AddFieldSecure("Command Used", context.Message.Content);
                embed.AddFieldSecure("Exception Message", e.Message);
                embed.AddFieldSecure("Type", e.GetType().FullName);
                embed.AddFieldSecure("Stacktrace", TrancuateStacktrace(e.StackTrace));
                await LogError(e, embed);
            }
        }

        public string TrancuateStacktrace(string input)
        {
            input = Format.Sanitize(input);
            if(input.Length > MAX_FIELD_VALUE_LENGTH)
            {
                input = input.Substring(0, MAX_FIELD_VALUE_LENGTH);
            }
            return input;
        }
    }
}
