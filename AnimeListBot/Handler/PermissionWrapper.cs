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
    }
}
