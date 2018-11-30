using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AnimeListBot.Handler;

namespace AnimeListBot.Modules
{
    public class Reset : ModuleBase<SocketCommandContext>
    {
        [Command("resetserver")]
        [Summary("Reloads the bot for the server. (Admin)")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ResetServer()
        {
            EmbedHandler embed = new EmbedHandler(Context.User, "Resetting Server Data...");
            await embed.SendMessage(Context.Channel);

            DiscordServer server = DiscordServer.GetServerFromID(Context.Guild.Id);
            if (server != null)
            {
                server.Users = new List<ServerUser>();
                foreach(SocketGuildUser user in server.Guild.Users)
                    if(server.GetUserFromId(user.Id) == null)
                        server.Users.Add(new ServerUser(user));

                server.SaveData();
                server.LoadData();
                embed.Title = "Server Data resetted.";
            }
            else
            {
                embed.Title = "Server Data never existed in this bot.";
            }
        }

        [Command("save")]
        [Summary("Saves the server and reloads it. (Admin)")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SaveAndLoad()
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

            EmbedHandler embed = new EmbedHandler(Context.User, "Server Saved...");
            await embed.SendMessage(Context.Channel);
        }
    }
}
