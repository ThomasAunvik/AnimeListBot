using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler.Logging
{
    public class BotLoggerProvider : ILoggerProvider
    {
        private readonly BotLoggerConfiguration _config;
        private readonly ConcurrentDictionary<string, BotLogger> _loggers = new ConcurrentDictionary<string, BotLogger>();

        public BotLoggerProvider(BotLoggerConfiguration config)
        {
            _config = config;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new BotLogger(name, _config));
        }

        public void Dispose()
        {
            _loggers.Clear();
        }
    }
}
