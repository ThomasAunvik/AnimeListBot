using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AnimeListBot.Handler
{
    public class EmbedHandler : EmbedBuilder
    {
        IUser user;
        IUserMessage embedMessage;

        public EmbedHandler(IUser user, string title = "", string description = "")
        {
            this.user = user;

            Title = title;
            Description = description;
            Author = new EmbedAuthorBuilder() { Name = user.Username, IconUrl = user.GetAvatarUrl() };
            Color = Program.embedColor;
        }

        public async Task SendMessage(IMessageChannel channel, string message = "")
        {
            embedMessage = await channel.SendMessageAsync(message, false, Build());
        }

        public async Task UpdateEmbed()
        {
            if (embedMessage != null)
            {
                await embedMessage.ModifyAsync(x => x.Embed = Build());
            }
        }

        public async Task EditMessage(string message)
        {
            if (embedMessage != null)
            {
                await embedMessage.ModifyAsync(x => x.Content = message);
            }
        }
    }
}
