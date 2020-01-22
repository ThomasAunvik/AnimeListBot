using AnimeListBot.Handler;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AnimeListBot.Modules
{
    public class Testing : ModuleBase<ICommandContext>
    {
        [Command("getservers")]
        public async Task GetAllServers()
        {
            string response = await DatabaseRequest.GetAllServers();
            EmbedHandler embed = new EmbedHandler(Context.User, "Servers in DB");
            embed.AddFieldSecure(new EmbedFieldBuilder() {
               Name = "In DB:",
               Value = response
            });
            await embed.SendMessage(Context.Channel);
        }

        [Command("getusers")]
        public async Task GetAllUsers()
        {
            string response = await DatabaseRequest.GetAllUsers();
            EmbedHandler embed = new EmbedHandler(Context.User, "Users in DB");
            embed.AddFieldSecure(new EmbedFieldBuilder()
            {
                Name = "In DB:",
                Value = response
            });
            await embed.SendMessage(Context.Channel);
        }
    }
}
