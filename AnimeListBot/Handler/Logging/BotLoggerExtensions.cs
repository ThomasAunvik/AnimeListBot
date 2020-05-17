using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler.Logging
{
    public static class BotLoggerExtensions
    {
        public static ILoggerFactory AddBotLogger(
                                      this ILoggerFactory loggerFactory,
                                      BotLoggerConfiguration config)
        {
            loggerFactory.AddProvider(new BotLoggerProvider(config));
            return loggerFactory;
        }
        public static ILoggerFactory AddBotLogger(
                                          this ILoggerFactory loggerFactory)
        {
            var config = new BotLoggerConfiguration();
            return loggerFactory.AddBotLogger(config);
        }
        public static ILoggerFactory AddBotLogger(
                                        this ILoggerFactory loggerFactory,
                                        Action<BotLoggerConfiguration> configure)
        {
            var config = new BotLoggerConfiguration();
            configure(config);
            return loggerFactory.AddBotLogger(config);
        }
    }
}
