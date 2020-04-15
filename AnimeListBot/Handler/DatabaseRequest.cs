using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Npgsql;
using System.Globalization;

namespace AnimeListBot.Handler
{
    public class DatabaseRequest
    {
        public static List<DiscordServer> GetAllServers()
        {
            return DatabaseConnection.db.DiscordServer.ToList();
        }

        public static List<DiscordUser> GetAllUsers()
        {
            return DatabaseConnection.db.DiscordUser.ToList();
        }

        public static DiscordServer GetServerById(ulong id)
        {
            return DatabaseConnection.db.DiscordServer.Where(x => x.ServerId == id).ToList().FirstOrDefault();
        }

        public static async Task<bool> CreateServer(DiscordServer server)
        {
            DatabaseConnection.db.DiscordServer.Add(server);
            await DatabaseConnection.db.SaveChangesAsync();
            return true;
        }

        public static async Task<bool> UpdateServer(DiscordServer server)
        {
            DatabaseConnection.db.DiscordServer.Where(x => x.ServerId == server.ServerId).FirstOrDefault().OverrideData(server);
            await DatabaseConnection.db.SaveChangesAsync();
            return true;
        }

        public static async Task<bool> RemoveServer(DiscordServer server)
        {
            DatabaseConnection.db.DiscordServer.Remove(GetServerById(server.ServerId));
            await DatabaseConnection.db.SaveChangesAsync();
            return true;
        }

        public static async Task<DiscordUser> GetUserById(ulong id, bool update = true)
        {
            DiscordUser user = DatabaseConnection.db.DiscordUser.Where(x => x.UserId == id).FirstOrDefault();

            if (update)
            {
                if (user.MalUsername != string.Empty && user.MalUsername != null) await user.UpdateMALInfo(user.MalUsername);
                if (user.AnilistUsername != string.Empty && user.AnilistUsername != null) await user.UpdateAnilistInfo(user.AnilistUsername);
            }

            return user;
        }

        public static bool DoesUserIdExist(ulong id)
        {
            return DatabaseConnection.db.DiscordUser.ToList().Exists(x => x.UserId == id);
        }

        public static async Task<bool> CreateUser(DiscordUser user)
        {
            DatabaseConnection.db.DiscordUser.Add(user);
            await DatabaseConnection.db.SaveChangesAsync();
            return true;
        }

        public static async Task<bool> RemoveUser(DiscordUser user)
        {
            DatabaseConnection.db.DiscordUser.Remove(await GetUserById(user.UserId));
            await DatabaseConnection.db.SaveChangesAsync();
            return true;
        }

        public static async Task<bool> UpdateUser(DiscordUser user)
        {
            DatabaseConnection.db.DiscordUser.Where(x => x.UserId == user.UserId).FirstOrDefault().OverrideData(user);
            await DatabaseConnection.db.SaveChangesAsync();
            return true;
        }
    }
}
