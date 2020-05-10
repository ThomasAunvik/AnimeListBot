using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AnimeListBot.Handler.Database
{
    public interface IDatabaseService : IDisposable
    {
        public List<DiscordServer> GetAllServers();

        public List<DiscordUser> GetAllUsers();

        public bool DoesServerIdExist(ulong id);

        public Task<DiscordServer> GetServerById(ulong id);

        public Task<bool> CreateServer(DiscordServer server);

        public Task<bool> RemoveServer(DiscordServer server);

        public Task<DiscordUser> GetUserById(ulong id, bool forceUpdate = false);

        public bool DoesUserIdExist(ulong id);

        public Task<bool> CreateUser(DiscordUser user);

        public Task<bool> RemoveUser(DiscordUser user);

        public Cluster GetCluster(int id);

        public List<Cluster> GetAllClusters();

        public Task SaveChangesAsync();
    }
}
