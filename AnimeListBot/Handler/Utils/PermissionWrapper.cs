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
using System;
using System.Threading.Tasks;
using Discord;

namespace AnimeListBot.Handler
{
    public static class PermissionWrapper
    {
        public static async Task DeleteMessage(IUserMessage message)
        {
            try
            {
                await message.DeleteAsync();
                return;
            }
            catch(Exception)
            {
                IGuild guild = Program._client.GetGuild(((IGuildUser)message.Author).Guild.Id);
                await Program._logger.Log("Bot does not have permission to delete message in server: (" + guild.Id + ", " + guild.Name + ")");
                return;
            }
        }

        public static async Task DeleteAllEmotes(IUserMessage message)
        {
            try
            {
                await message.RemoveAllReactionsAsync();
                return;
            }
            catch (Exception)
            {
                IGuild guild = Program._client.GetGuild(((IGuildUser)message.Author).Guild.Id);
                await Program._logger.Log("Bot does not have permission to delete message emotes in server: (" + guild.Id + ", " + guild.Name + ")");
                return;
            }
        }
    }
}
