using System;
using System.Threading.Tasks;
using Discord;

namespace MALBot.Handler
{
    public static class PermissionWrapper
    {
        public static async Task DeleteMessage(IMessage message)
        {
            try
            {
                await message.DeleteAsync();
                return;
            }
            catch(Exception)
            {
                Console.WriteLine("Bot does not have permission to delete message");
                return;
            }
        }
    }
}
