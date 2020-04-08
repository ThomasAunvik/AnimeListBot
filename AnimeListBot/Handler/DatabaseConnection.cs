using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AnimeListBot.Handler
{
    class DatabaseConnection
    {
        public struct DataBaseLogin
        {
            public string ip;
            public string port;
            public string catalog;
            public string userid;
            public string password;

            public DataBaseLogin(string ip, string port, string catalog, string userid, string password)
            {
                this.ip = ip;
                this.port = port;
                this.catalog = catalog;
                this.userid = userid;
                this.password = password;
            }
        }

        private static DataBaseLogin login;

        public static void UpdateLogin()
        {
            string text = File.ReadAllText("database_login.json");
            login = JsonConvert.DeserializeObject<DataBaseLogin>(text);
        }

        public static async Task<DataSet> SendSql(string sql)
        {
            UpdateLogin();

            string connstring = String.Format("Server={0};Port={1};" +
                    "User Id={2};Password={3};Database={4};",
                    login.ip, login.port, login.userid,
                    login.password, login.catalog);
            NpgsqlConnection conn = new NpgsqlConnection(connstring);
            await conn.OpenAsync();

            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            DataSet ds = new DataSet();
            ds.Reset();

            da.Fill(ds);

            conn.Close();

            return ds;
        }
    }
}
