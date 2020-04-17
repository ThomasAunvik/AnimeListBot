﻿/*
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
    public class Favorites : ModuleBase<ShardedCommandContext>
    {
        [Command("favoriteanime")]
        public async Task FavAnime(IUser user = null)
        {
            if (user == null) user = Context.User;
            EmbedHandler embed = new EmbedHandler(user, "Loading Favorite Anime...");
            embed.SetOwner(Context.User);
            await embed.SendMessage(Context.Channel);

            DiscordUser gUser = await DatabaseRequest.GetUserById(user.Id);

            DiscordServer server = await DatabaseRequest.GetServerById(Context.Guild.Id);

            if (gUser != null && !string.IsNullOrWhiteSpace(gUser.GetAnimelistUsername()))
            {
                await gUser.UpdateUserInfo();
                UserProfile profile = gUser.malProfile;
                List<MALImageSubItem> favAnimeList = profile.Favorites.Anime.ToList();
                embed.Title = "";
                if (favAnimeList.Count <= 0) embed.Description = "No Anime Favorites...";

                string favListMessage = string.Empty;

                for (int i = 0; i < favAnimeList.Count && i < 10; i++)
                {
                    int index = i;
                    MALImageSubItem subItem = favAnimeList[index];
                    Emoji emote = new Emoji(Emotes.NUMBERS_EMOTES[index]);

                    favListMessage += emote + " " + subItem.Name + "\n";
                    embed.AddEmojiAction(emote, async () => {
                        embed.Fields.Clear();
                        embed.RemoveAllEmojiActions();
                        await Search.GetAnime(embed, user, subItem.MalId); 
                    });
                }

                if(favListMessage != string.Empty) embed.AddField("Favorite Anime", favListMessage);

                await embed.UpdateEmbed();
            }
        }

        [Command("favoritemanga")]
        public async Task FavManga (IUser user = null)
        {
            if (user == null) user = Context.User;
            EmbedHandler embed = new EmbedHandler(user, "Loading Favorite Manga...");
            embed.SetOwner(Context.User);
            await embed.SendMessage(Context.Channel);

            DiscordUser gUser = await DatabaseRequest.GetUserById(user.Id);

            DiscordServer server = await DatabaseRequest.GetServerById(Context.Guild.Id);

            if (gUser != null && !string.IsNullOrWhiteSpace(gUser.GetAnimelistUsername()))
            {
                await gUser.UpdateUserInfo();
                UserProfile profile = gUser.malProfile;
                List<MALImageSubItem> favMangaList = profile.Favorites.Manga.ToList();
                embed.Title = "";
                if (favMangaList.Count <= 0) embed.Description = "No Manga Favorites...";

                string favListMessage = string.Empty;

                for (int i = 0; i < favMangaList.Count && i < 10; i++)
                {
                    int index = i;
                    MALImageSubItem subItem = favMangaList[index];
                    Emoji emote = new Emoji(Emotes.NUMBERS_EMOTES[index]);

                    favListMessage += emote + " " + subItem.Name + "\n";
                    embed.AddEmojiAction(emote, async () => {
                        embed.Fields.Clear();
                        embed.RemoveAllEmojiActions();
                        await Search.GetManga(embed, user, subItem.MalId);
                    });
                }

                if (favListMessage != string.Empty) embed.AddField("Favorite Manga", favListMessage);

                await embed.UpdateEmbed();
            }
        }
    }
}
