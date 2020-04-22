using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AnimeListBot.Handler
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class RequireOwnerPermissionAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            ulong userId = context.User.Id;
            bool isBotOwner = Program.botOwners.Contains(userId);
            if (!isBotOwner)
            {
                return Task.FromResult(PreconditionResult.FromError("User is not an owner of this bot."));
            }

            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
