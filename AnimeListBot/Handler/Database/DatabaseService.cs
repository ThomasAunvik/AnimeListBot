using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimeListBot.Handler.Database
{
    public class DatabaseService : IDatabaseService
    {
        protected readonly DatabaseConnection dbConn;

        public DatabaseService(DatabaseConnection db)
        {
            dbConn = db;
        }

        public List<DiscordServer> GetAllServers()
        {
            return dbConn.DiscordServer.ToList();
        }

        public List<DiscordUser> GetAllUsers()
        {
            return dbConn.DiscordUser.ToList();
        }

        public bool DoesServerIdExist(ulong id)
        {
            return dbConn.DiscordServer.Find(id) == null;
        }

        public async Task<DiscordServer> GetServerById(ulong id)
        {
            DiscordServer server = await dbConn.DiscordServer.FindAsync(id);
            if (server == null)
            {
                server = new DiscordServer(Program._client.GetGuild(id));
                await CreateServer(server);
            }
            return server;
        }

        public async Task<bool> CreateServer(DiscordServer server)
        {
            dbConn.DiscordServer.Add(server);
            await dbConn.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveServer(DiscordServer server)
        {
            dbConn.DiscordServer.Remove(await GetServerById(server.ServerId));
            await dbConn.SaveChangesAsync();
            return true;
        }

        public async Task<DiscordUser> GetUserById(ulong id, bool forceUpdate = false)
        {
            DiscordUser user = dbConn.DiscordUser.Find(id);
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
            return dbConn.DiscordUser.Find(id) != null;
        }

        public async Task<bool> CreateUser(DiscordUser user)
        {
            if (user == null || user.UserId == 0) return false;
            dbConn.DiscordUser.Add(user);
            await dbConn.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveUser(DiscordUser user)
        {
            dbConn.DiscordUser.Remove(await GetUserById(user.UserId));
            await dbConn.SaveChangesAsync();
            return true;
        }

        public Cluster GetCluster(int id)
        {
            return dbConn.Cluster.Find(id);
        }

        public List<Cluster> GetAllClusters()
        {
            return dbConn.Cluster.ToList();
        }

        public async Task SaveChangesAsync()
        {
            await dbConn.SaveChangesAsync();
        }

        public void Dispose()
        {
            dbConn.Dispose();
        }
    }
}
