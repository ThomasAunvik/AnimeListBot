using Discord.Commands;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using AnimeListBot.Handler;

namespace AnimeListBot.Modules
{
    public class Administrator : ModuleBase<ICommandContext>
    {
        [Command("stop")]
        public async Task StopBot()
        {
            EmbedHandler embed = new EmbedHandler(Context.User);
            if(Program.botOwners.Contains(Context.User.Id.ToString()))
            {
                embed.Title = "Stopping Bot.";
                await embed.SendMessage(Context.Channel);

                await Program._client.StopAsync();
                Program.stop = true;
            }
            else{
                embed.Title = "You dont have permission to do this command.";
                await embed.SendMessage(Context.Channel);
            }
        }

        [Command("errortest")]
        public async Task SendErrorTest([Remainder]string message)
        {
            EmbedHandler embed = new EmbedHandler(Context.User);
            if (Program.botOwners.Contains(Context.User.Id.ToString()))
            {
                embed.Title = "Sent fake error.";
                await embed.SendMessage(Context.Channel);
                try
                {
                    throw new Exception("Fake Error: " + message);
                }catch(Exception e)
                {
                    await Program._logger.LogError(e);
                }
            }
            else
            {
                embed.Title = "You dont have permission to do this command.";
                await embed.SendMessage(Context.Channel);
            }
        }
    }
}
