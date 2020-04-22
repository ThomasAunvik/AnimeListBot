using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AnimeListBot.Handler
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    class RequireValidAnimelistAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            DiscordUser user = await DatabaseRequest.GetUserById(context.User.Id);
            if (!user.HasValidAnimelist())
            {
                return PreconditionResult.FromError("User has not registered a MAL or Anilist account to his discord user.");
            }
            return PreconditionResult.FromSuccess();
        }
    }
}
