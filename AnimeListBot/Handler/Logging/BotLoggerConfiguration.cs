using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler.Logging
{
    public class BotLoggerConfiguration
    {
        public LogLevel LogLevel { get; set; } = LogLevel.Warning;
        public int EventId { get; set; } = 0;
        public bool SendToOwner { get; set; } = false;

        public string FileDirectory { get; set; } = "logs";
        public string FileName { get; set; } = "newest.log";
    }
}
