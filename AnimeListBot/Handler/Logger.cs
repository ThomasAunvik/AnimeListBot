using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Discord;

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
                FileStream logStream = File.Create(logPath);
                logStream.Close();
            }
        }

        public async Task Log(string message)
        {
            StreamWriter writer = new StreamWriter(logPath, true);
            await writer.WriteAsync((logLines == 0 ? "" : writer.NewLine) + message);
            Console.WriteLine(message);
            writer.Close();

            logLines++;
        }

        public async Task LogError(string errorMessage)
        {
            const ulong ownerId = 96580514021912576;
            IUser owner = Program._client.GetUser(ownerId);
            var dmOwner = await owner?.GetOrCreateDMChannelAsync();

            await owner.SendMessageAsync("```" + errorMessage + "```");

            await Log(errorMessage);
        }

        public async Task LogError(Exception exception)
        {
            string errorMessage = exception.Message + "\n" + exception.StackTrace;
            await LogError(errorMessage);
        }
    }
}
