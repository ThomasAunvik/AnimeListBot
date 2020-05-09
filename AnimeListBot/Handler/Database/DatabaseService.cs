using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimeListBot.Handler.Database
{
    public class DatabaseService : DatabaseConnection
    {
        public List<DiscordServer> GetAllServers()
        {
            return DiscordServer.ToList();
        }

        public List<DiscordUser> GetAllUsers()
        {
            return DiscordUser.ToList();
        }

        public bool DoesServerIdExist(ulong id)
        {
            return DiscordServer.Find(id) == null;
        }

        public async Task<DiscordServer> GetServerById(ulong id)
        {
            DiscordServer server = await DiscordServer.FindAsync(id);
            if (server == null)
            {
                server = new DiscordServer(Program._client.GetGuild(id));
                await CreateServer(server);
            }
            return server;
        }

        public async Task<bool> CreateServer(DiscordServer server)
        {
            DiscordServer.Add(server);
            await SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveServer(DiscordServer server)
        {
            DiscordServer.Remove(await GetServerById(server.ServerId));
            await SaveChangesAsync();
            return true;
        }

        public async Task<DiscordUser> GetUserById(ulong id, bool forceUpdate = false)
        {
            DiscordUser user = DiscordUser.Find(id);
            if (user == null)
            {
                await CreateUser(user = new DiscordUser(Program._client.GetUser(id)));
                if (user == null || user.UserId == 0) return null;
            }

            if (user.MalUsername != string.Empty && (user.malCachedTime < DateTime.Now || forceUpdate))
            {
                user.malCachedTime = DateTime.Now.AddMinutes(15);
                await user.UpdateMALInfo(user.MalUsername);
            }

            if (user.AnilistUsername != string.Empty && (user.anilistCachedTime < DateTime.Now || forceUpdate))
            {
                user.anilistCachedTime = DateTime.Now.AddMinutes(15);
                await user.UpdateAnilistInfo(user.AnilistUsername);
            }

            return user;
        }

        public bool DoesUserIdExist(ulong id)
        {
            return DiscordUser.Find(id) != null;
        }

        public async Task<bool> CreateUser(DiscordUser user)
        {
            if (user == null || user.UserId == 0) return false;
            DiscordUser.Add(user);
            await SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveUser(DiscordUser user)
        {
            DiscordUser.Remove(await GetUserById(user.UserId));
            await SaveChangesAsync();
            return true;
        }
    }
}
