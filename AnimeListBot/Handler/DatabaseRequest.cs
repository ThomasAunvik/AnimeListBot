using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Npgsql;

namespace AnimeListBot.Handler
{
    public class DatabaseRequest
    {
        public static async Task<string> GetAllServers()
        {
            DataSet set = await DatabaseConnection.SendSql("SELECT * FROM public.discord_server");
            StringBuilder message = new StringBuilder();

            message.Append(set.Tables[0].Rows[0].ItemArray[0]);
            for(int i = 1; i < set.Tables.Count; i++)
            {
                message.Append(", " + set.Tables[0].Rows[0].ItemArray[i].ToString());
            }
            return message.ToString();
        }

        public static async Task<string> GetAllUsers()
        {
            DataSet set = await DatabaseConnection.SendSql("SELECT * FROM public.discord_user");
            StringBuilder message = new StringBuilder();

            message.Append(set.Tables[0].Rows[0].ItemArray[0]);
            for (int i = 1; i < set.Tables.Count; i++)
            {
                message.Append(", " + set.Tables[0].Rows[i].ItemArray[0].ToString());
            }
            return message.ToString();
        }

        public static async Task<DiscordServer> GetServerById(ulong id)
        {
            DataSet set = await DatabaseConnection.SendSql("SELECT * from public.discord_server where server_id = " + id.ToString());

            if (set.Tables[0].Rows.Count <= 0) { 
                DiscordServer server = new DiscordServer(Program._client.GetGuild(id));
                await CreateServer(server);
                return server;
            }

            DataRow row = set.Tables[0].Rows[0];
            try
            {
                DiscordServer discordServer = new DiscordServer();
                discordServer.id = ulong.Parse(row.ItemArray[0].ToString());
                discordServer.animeListChannelId = ulong.Parse(row.ItemArray[1].ToString());
                discordServer.animeRoleIds = ((long[])row.ItemArray[2]).ToList().ConvertAll(i => (ulong)i);
                discordServer.animeRoleDays = ((double[])row.ItemArray[3]).ToList();
                discordServer.mangaRoleIds = ((long[])row.ItemArray[4]).ToList().ConvertAll(i => (ulong)i);
                discordServer.mangaRoleDays = ((double[])row.ItemArray[5]).ToList();
                return discordServer;
            }
            catch (Exception e) {
                await Program._logger.LogError(e);
                return null;
            }
        }

        public static async Task<bool> CreateServer(DiscordServer server)
        {
            await DatabaseConnection.SendSql(string.Format(
                @"INSERT INTO public.discord_server (
                server_id, register_channel_id, animerole_id, animerole_days, mangarole_id, mangarole_days) VALUES (
                '{0}'::bigint, '{1}'::bigint, '{2}'::bigint[], '{3}'::double precision[], '{4}'::bigint[], '{5}'::double precision[])
                 returning server_id;",
                server.id.ToString(), server.animeListChannelId.ToString(),
                "{" + string.Join(", ", server.animeRoleIds) + "}",
                "{" + string.Join(", ", server.animeRoleDays) + "}",
                "{" + string.Join(", ", server.mangaRoleIds) + "}",
                "{" + string.Join(", ", server.mangaRoleDays) + "}"
            ));
            return true;
        }

        public static async Task<bool> UpdateServer(DiscordServer server)
        {
            await DatabaseConnection.SendSql(string.Format(
                @"UPDATE public.discord_server SET
                register_channel_id = '{0}'::bigint,
                animerole_id = '{1}'::bigint[],
                animerole_days = '{2}'::double precision[],
                mangarole_id = '{3}'::bigint[],
                mangarole_days = '{4}'::double precision[] 
                WHERE server_id = '{5}';",
                server.animeListChannelId.ToString(),
                "{" + string.Join(", ", server.animeRoleIds) + "}",
                "{" + string.Join(", ", server.animeRoleDays) + "}",
                "{" + string.Join(", ", server.mangaRoleIds) + "}",
                "{" + string.Join(", ", server.mangaRoleDays) + "}",
                server.id
            ));
            return true;
        }

        public static async Task<DiscordUser> GetUserById(ulong id)
        {
            DataSet set = await DatabaseConnection.SendSql("SELECT * from public.discord_user where user_id = " + id);
            if (set.Tables[0].Rows.Count <= 0) return null;

            DataRow row = set.Tables[0].Rows[0];

            DiscordUser discordUser = new DiscordUser();
            discordUser.userID = ulong.Parse(row.ItemArray[0].ToString());
            discordUser.animeList = (DiscordUser.AnimeList)((int)row.ItemArray[3]);

            string mal_username = row.ItemArray[1].ToString();
            string anilist_username = row.ItemArray[2].ToString();

            if (mal_username != string.Empty) await discordUser.UpdateMALInfo(mal_username);
            if (anilist_username != string.Empty) await discordUser.UpdateAnilistInfo(anilist_username);

            return discordUser;
        }

        public static async Task<bool> DoesUserIdExist(ulong id)
        {
            DataSet set = await DatabaseConnection.SendSql("SELECT * from public.discord_user where user_id = " + id);
            if (set.Tables[0].Rows.Count <= 0) return false;

            DataRow table = set.Tables[0].Rows[0];
            return ulong.Parse(table.ItemArray[0].ToString()) == id;
        }

        public static async Task<bool> CreateUser(DiscordUser user)
        {
            await DatabaseConnection.SendSql(string.Format(
                @"INSERT INTO public.discord_user (user_id, mal_username, anilist_username, list_preference) VALUES (
                    '{0}'::bigint, '{1}'::text, '{2}'::text, '{3}'::integer)
                     returning user_id;",
                user.userID, user.malProfile?.Username, user.anilistProfile?.name, (int)user.animeList
            ));
            return true;
        }

        public static async Task<bool> UpdateUser(DiscordUser user)
        {
            await DatabaseConnection.SendSql(string.Format(
                @"UPDATE public.discord_user SET
                list_preference = '{0}'::integer,
                anilist_username = '{1}'::text,
                mal_username = '{2}'::text
                WHERE user_id = '{3}';",
                (int)user.animeList, user.anilistProfile?.name, user.malProfile?.Username, user.userID
            ));
            return true;
        }
    }
}
