/*
 * This file is part of AnimeList Bot
 *
 * AnimeList Bot is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * AnimeList Bot is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with AnimeList Bot.  If not, see <https://www.gnu.org/licenses/>
 */
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

        IGuildUser user;
        IGuildUser owner;
        IUserMessage embedMessage;

        private static List<EmbedHandler> activatedEmoteActions;
        private List<(IEmote, Action)> emojiActions = new List<(IEmote, Action)>();
        private DateTime emoteTimeout;

        public EmbedHandler(IUser user, string title = "", string description = "", bool debug = false)
        {
            this.user = (IGuildUser)user;

            Title = title;
            Description = description;

            if (user != null)
            {
                Author = new EmbedAuthorBuilder() { Name = user.Username + "#" + user.Discriminator, IconUrl = user.GetAvatarUrl() };
            }

            Color = Program.embedColor;
        }

        public void SetOwner(IUser user)
        {
            owner = (IGuildUser)user;
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
            if (embedMessage != null) return;

            await embedMessage.ModifyAsync(x => x.Embed = Build());
                
            await PermissionWrapper.DeleteAllEmotes(embedMessage);

            List<IEmote> allEmotes = embedMessage.Reactions.Keys.ToList();
            List<(IEmote, Action)> newEmotes = emojiActions.FindAll(x => allEmotes.Find(y=> y.Name == x.Item1.Name) == null);
            if (newEmotes.Count > 0 && embedMessage.Reactions.Select(x => x.Key) != newEmotes)
            {
                if(newEmotes.Count > 0) await embedMessage.AddReactionsAsync(newEmotes.Select(x=>x.Item1).ToArray());
                emoteTimeout = DateTime.Now.AddSeconds(120);

                CheckTimeouts();
            }
        }

        public async Task EditMessage(string message)
        {
            if (embedMessage == null) throw new Exception("Unable to edit embed message, there is no message set.");
            await embedMessage.ModifyAsync(x => x.Content = message);
           
        }

        public int AddFieldSecure(string name, object value, bool inline = false)
        {
            if (value is string)
            {
                string stringValue = value as string;
                value = SecureEmbedText(stringValue);
            }
            AddField(name, value, inline);
            return Fields.Count - 1; // Index value of field for Fields.
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
            if (value == null) return string.Empty;

            if(value.Contains("<br>")) value = value.Replace("<br>", "\n");
            if(value.Contains("\\n")) value = value.Replace("\\n", "\n");
            if(value.Contains("\n\r")) value = value.Replace("\n\r", "\n");
            if(value.Contains("\n\n")) value = value.Replace("\n\n", "\n");

            if (value.Length > MAX_FIELD_VALUE_LENGTH)
            {
                value = value.Substring(0, MAX_FIELD_VALUE_LENGTH - 3) + "...";
            }

            return value;
        }

        public async Task AddEmojiActions(List<(IEmote, Action)> actions)
        {
            if (activatedEmoteActions == null) activatedEmoteActions = new List<EmbedHandler>();

            if (!activatedEmoteActions.Contains(this))
            {
                activatedEmoteActions.Add(this);
            }
            emojiActions.AddRange(actions);

            if (embedMessage != null) await UpdateEmbed();
        }

        public void AddEmojiAction(IEmote emote, Action action)
        {
            if (activatedEmoteActions == null) activatedEmoteActions = new List<EmbedHandler>();

            if (!activatedEmoteActions.Contains(this))
            {
                activatedEmoteActions.Add(this);
            }
            emojiActions.Add((emote, action));
        }

        public void RemoveAllEmojiActions()
        {
            emojiActions.Clear();
            activatedEmoteActions.Remove(this);
        }

        public static void ExecuteAnyEmoteAction(SocketReaction socketReaction)
        {
            if (activatedEmoteActions == null) return;

            CheckTimeouts();
            EmbedHandler embed = activatedEmoteActions.Find(x => x.embedMessage?.Id == socketReaction.MessageId);
            if (embed == null) return;

            if (socketReaction.UserId != embed.owner.Id) return;

            (IEmote, Action) actionEmote = embed.emojiActions.Find(x => x.Item1.Name == socketReaction.Emote.Name);
            if(actionEmote != (null, null))
            {
                actionEmote.Item2();
            }
        }

        public static void CheckTimeouts()
        {
            if (activatedEmoteActions == null) return;

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
