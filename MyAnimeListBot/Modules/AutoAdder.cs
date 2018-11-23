using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using MALBot.Handler;
using JikanDotNet;

namespace MALBot.Modules
{
    public class AutoAdder : ModuleBase<ICommandContext>
    {
        [Command("automal")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetChannel(ITextChannel channel)
        {
            DiscordServer server = DiscordServer.GetServerFromID(Context.Guild.Id);
            server.animeListChannelId = channel.Id;
            await Update();
        }

        [Command("automalupdate")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Update()
        {
            DiscordServer server = DiscordServer.GetServerFromID(Context.Guild.Id);
            ITextChannel channel = await Context.Guild.GetChannelAsync(server.animeListChannelId) as ITextChannel;

            IEnumerable<IMessage> messages = await channel.GetMessagesAsync(100).FlattenAsync();
            List<IMessage> listMessages = messages.ToList();

            listMessages.ForEach(async x =>
            {
                await AddUser(x);
            });
        }

        public static async Task AddUser(IMessage message)
        {
            Uri link = null;
            bool isValidLink = Uri.TryCreate(message.Content, UriKind.Absolute, out link);
            bool containsMAL = message.Content.Contains("myanimelist");
            if (isValidLink && containsMAL)
            {
                string usernamePart = link.Segments[link.Segments.Length - 1];
                UserProfile profile = await Program._jikan.GetUserProfile(usernamePart);
                if (profile != null)
                {
                    GlobalUser user = Program.globalUsers.Find(y => y.userID == message.Author.Id);
                    if (user == null)
                    {
                        user = new GlobalUser(message.Author);
                        user.MAL_Username = usernamePart;
                        Program.globalUsers.Add(user);
                        await user.UpdateMALInfo();
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(user.MAL_Username))
                        {
                            user.MAL_Username = usernamePart;
                            await user.UpdateMALInfo();
                        }
                    }
                }
            }
        }
    }
}
