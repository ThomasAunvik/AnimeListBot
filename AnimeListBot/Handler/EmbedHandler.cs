using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace AnimeListBot.Handler
{
    public class EmbedHandler : EmbedBuilder
    {
        const int MAX_FIELD_VALUE_LENGTH = 2048;

        IUser user;
        IUserMessage embedMessage;

        private static List<EmbedHandler> activatedEmoteActions = new List<EmbedHandler>();
        private List<(IEmote, Action)> emojiActions = new List<(IEmote, Action)>();
        private DateTime emoteTimeout;

        public EmbedHandler(IUser user, string title = "", string description = "")
        {
            this.user = user;

            Title = title;
            Description = description;

            if (user != null)
            {
                Author = new EmbedAuthorBuilder() { Name = user.Username + "#" + user.Discriminator, IconUrl = user.GetAvatarUrl() };
            }

            Color = Program.embedColor;
        }

        public async Task SendMessage(IMessageChannel channel, string message = "")
        {
            embedMessage = await channel.SendMessageAsync(message, false, Build());
            
            await embedMessage.AddReactionsAsync(emojiActions.Select(x => x.Item1).ToArray());
            emoteTimeout = DateTime.Now.AddSeconds(120);
            CheckTimeouts();
        }

        public async Task UpdateEmbed()
        {
            if (embedMessage != null)
            {
                await embedMessage.ModifyAsync(x => x.Embed = Build());

                IEmote[] newEmotes = emojiActions.Select(x => x.Item1).ToArray();
                if (newEmotes.Length > 0 && embedMessage.Reactions.Select(x => x.Key) != newEmotes)
                {
                    await embedMessage.RemoveAllReactionsAsync();
                    await embedMessage.AddReactionsAsync(newEmotes);
                    emoteTimeout = DateTime.Now.AddSeconds(120);

                    CheckTimeouts();
                }
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

        public async Task AddEmojiActions(List<(IEmote, Action)> actions)
        {
            if (!activatedEmoteActions.Contains(this))
            {
                activatedEmoteActions.Add(this);
            }
            emojiActions.AddRange(actions);

            if (embedMessage != null) await UpdateEmbed();
        }

        public void AddEmojiAction(IEmote emote, Action action)
        {
            if (!activatedEmoteActions.Contains(this))
            {
                activatedEmoteActions.Add(this);
            }
            emojiActions.Add((emote, action));
        }

        public static void ExecuteAnyEmoteAction(IEmote emote, IMessage message)
        {
            CheckTimeouts();
            EmbedHandler embed = activatedEmoteActions.Find(x => x.embedMessage?.Id == message.Id);
            if (embed == null) return;

            (IEmote, Action) actionEmote = embed.emojiActions.Find(x => x.Item1 == emote);
            if(actionEmote != (null, null))
            {
                actionEmote.Item2();
            }
        }

        public static void CheckTimeouts()
        {
            activatedEmoteActions.ToList().ForEach(x =>
            {
                if(x.emoteTimeout < DateTime.Now)
                {
                    activatedEmoteActions.Remove(x);
                }
            });
        }
    }
}
