﻿using Discord;
using Discord.WebSocket;
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

            if (user != null)
            {
                Author = new EmbedAuthorBuilder() { Name = user.Username, IconUrl = user.GetAvatarUrl() };
            }

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
            else
            {
                throw new Exception("Unable to update Embed, there is no message set.");
            }
        }

        public async Task EditMessage(string message)
        {
            if (embedMessage != null)
            {
                await embedMessage.ModifyAsync(x => x.Content = message);
            }
            else
            {
                throw new Exception("Unable to edit embed message, there is no message set.");
            }
        }

        
    }
}
