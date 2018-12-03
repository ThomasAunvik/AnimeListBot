using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace AnimeListBot.Handler
{
    public class Logger
    {
        string logPath;
        int logLines = 0;

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
            }
        }

        public async Task Log(string message)
        {
            using (StreamWriter writer = new StreamWriter(logPath, true))
            {
                await writer.WriteAsync((logLines == 0 ? "" : writer.NewLine) + message);
                Console.WriteLine(message);
                writer.Close();
            }
            logLines++;
        }

        public async Task LogError(string errorMessage, EmbedBuilder sendEmbed)
        {
            const ulong ownerId = 96580514021912576;
            IUser owner = Program._client.GetUser(ownerId);
            var dmOwner = await owner?.GetOrCreateDMChannelAsync();

            await owner.SendMessageAsync("", false, sendEmbed.Build());

            await Log(errorMessage);
        }

        public async Task LogError(string errorMessage, IUser user = null)
        {
            EmbedHandler embed = new EmbedHandler(user, "Error");
            embed.AddField("ErrorMessage", errorMessage);
            await LogError(errorMessage, embed);
        }

        public async Task LogError(Exception exception, EmbedBuilder embed)
        {
            string errorMessage = exception.Message + "\n" + exception.StackTrace;
            await LogError(errorMessage, embed);
        }

        public async Task LogError(Exception exception, IUser user = null)
        {
            EmbedHandler embed = new EmbedHandler(user, "Exception");
            embed.AddField("Exception Message", exception.Message);
            embed.AddField("Stacktrace", Format.Sanitize(exception.StackTrace).Substring(0, 1024));
            await LogError(exception, embed);
        }

        public async Task LogError(CommandInfo info, ICommandContext context, IResult result)
        {
            ExecuteResult execute = (ExecuteResult)result;
            Exception e = execute.Exception;

            EmbedHandler embed = new EmbedHandler(context.User, "Command Exception");
            embed.AddField("Command Used", context.Message);
            embed.AddField("Exception Message", e.Message);
            embed.AddField("Stacktrace", Format.Sanitize(e.StackTrace).Substring(0, 1024));
            await LogError(e, embed);
        }
    }
}
