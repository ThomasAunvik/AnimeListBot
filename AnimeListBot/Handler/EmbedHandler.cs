using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AnimeListBot.Handler
{
    public class EmbedHandler : EmbedBuilder
    {
        const int MAX_FIELD_VALUE_LENGTH = 2048;

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

        public void AddFieldSecure(string name, object value, bool inline = false)
        {
            if (value is string)
            {
                string stringValue = value as string;
                value = SecureEmbedText(stringValue);
            }
            AddField(name, value, inline);
        }

        public void AddFieldSecure(EmbedFieldBuilder field)
        {
            if (field.Value is string)
            {
                string stringValue = field.Value as string;
                field.Value = SecureEmbedText(stringValue);
            }
            AddField(field);
        }

        public static string SecureEmbedText(string value)
        {
            if (value.Length > MAX_FIELD_VALUE_LENGTH)
            {
                value = value.Substring(0, MAX_FIELD_VALUE_LENGTH - 3) + "...";
            }
            return value;
        }
    }
}
