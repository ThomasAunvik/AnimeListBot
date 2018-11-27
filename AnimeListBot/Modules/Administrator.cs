using Discord.Commands;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AnimeListBot.Modules
{
    public class Administrator : ModuleBase<ICommandContext>
    {
        [Command("stop")]
        public async Task StopBot()
        {
            if(Program.botOwners.Contains(Context.User.Id.ToString()))
            {
                await ReplyAsync("Stopping Bot.");
                await Program._client.StopAsync();
                Environment.Exit(0);
            }
            else{
                await ReplyAsync("You dont have permission to do this command.");
            }
        }
    }
}
