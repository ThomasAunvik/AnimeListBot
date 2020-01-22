using AnimeListBot.Handler;
using Discord;
using Discord.Commands;
using JikanDotNet;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace AnimeListBot.Modules
{
    public class FavoriteAnime : ModuleBase<ICommandContext>
    {
        [Command("favoriteanime")]
        public async Task FavAnime(IUser user = null)
        {
            if (user == null) user = Context.User;
            EmbedHandler embed = new EmbedHandler(user, "Loading Favorite Anime...");
            await embed.SendMessage(Context.Channel);

            DiscordUser gUser = await DatabaseRequest.GetUserById(user.Id);

            DiscordServer server = await DatabaseRequest.GetServerById(Context.Guild.Id);

            if (gUser != null && !string.IsNullOrWhiteSpace(gUser.GetAnimelistUsername()))
            {
                await gUser.UpdateUserInfo();
                UserProfile profile = gUser.malProfile;
                List<MALImageSubItem> favAnimeList = profile.Favorites.Anime.ToList();
                embed.Title = "Favorite Anime";

                for(int i = 0; i < favAnimeList.Count && i < 10; i++)
                {
                    int index = i;
                    MALImageSubItem subItem = favAnimeList[index];
                    Emoji emote = new Emoji(Emotes.NUMBERS_EMOTES[index]);
                    
                    embed.AddField(Program.EMPTY_EMBED_SPACE, emote + " " + subItem.Name);
                    embed.AddEmojiAction(emote, async () => await Search.SearchAnime(embed, user, subItem.Name));
                }

                await embed.UpdateEmbed();
            }
        }
    }
}
