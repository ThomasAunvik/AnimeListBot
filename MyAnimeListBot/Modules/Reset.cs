using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MALBot.Handler;

namespace MALBot.Modules
{
    public class Reset : ModuleBase<SocketCommandContext>
    {
        [Command("resetserver")]
        [Summary("Reloads the bot for the server. (Admin)")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ResetServer()
        {
            IUserMessage message = await ReplyAsync("Reseting...");
            DiscordServer server = DiscordServer.GetServerFromID(Context.Guild.Id);
            if (server != null)
            {
                server.Users = new List<ServerUser>();
                foreach(SocketGuildUser user in server.Guild.Users)
                    if(server.GetUserFromId(user.Id) == null)
                        server.Users.Add(new ServerUser(user));
            }
            else await message.ModifyAsync(x => x.Content = "Server Resetted.");

            server.SaveData();
                server.LoadData();
            await message.ModifyAsync(x => x.Content = "L-Like this?");
        }

        [Command("save")]
        [Summary("Saves the server and reloads it. (Admin)")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public void SaveAndLoad()
        {
            DiscordServer server = DiscordServer.GetServerFromID(Context.Guild.Id);
            if (server != null)
            {
                server.SaveData();
                server.LoadData();
            }
            else
            {
                Program.discordServers.Add(new DiscordServer(Context.Guild));
            }
        }
    }
}
