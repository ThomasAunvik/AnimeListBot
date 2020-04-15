using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AnimeListBot.Handler
{
    public partial class DatabaseConnection : DbContext
    {
        private static DatabaseConnection _db;
        public static DatabaseConnection db { get {
                if (_db == null) _db = new DatabaseConnection();
                return _db;
            } }

        public virtual DbSet<DiscordServer> DiscordServer { get; set; }
        public virtual DbSet<DiscordUser> DiscordUser { get; set; }

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

        public static DataBaseLogin UpdateLogin()
        {
            string text = File.ReadAllText("database_login.json");
            return JsonConvert.DeserializeObject<DataBaseLogin>(text);
        }

        private string GetConnectionString()
        {
            DataBaseLogin login = UpdateLogin();
            return string.Format("Server={0};Port={1};" +
                    "User Id={2};Password={3};Database={4};",
                    login.ip, login.port, login.userid,
                    login.password, login.catalog);
        }

        public static async Task<DataSet> SendSql(string sql)
        {
            DataBaseLogin login = UpdateLogin();

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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(GetConnectionString());
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DiscordServer>(entity =>
            {
                entity.HasKey(e => e.ServerId)
                    .HasName("Servers_pkey");

                entity.ToTable("discord_server");

                entity.Property(e => e.ServerId)
                    .HasColumnName("server_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.AnimeroleDays).HasColumnName("animerole_days");

                entity.Property(e => e.AnimeroleId).HasColumnName("animerole_id");

                entity.Property(e => e.MangaroleDays).HasColumnName("mangarole_days");

                entity.Property(e => e.MangaroleId).HasColumnName("mangarole_id");

                entity.Property(e => e.Prefix).HasColumnName("prefix");

                entity.Property(e => e.RegisterChannelId).HasColumnName("register_channel_id");
            });

            modelBuilder.Entity<DiscordUser>(entity =>
            {
                entity.HasKey(e => e.UserId)
                    .HasName("Users_pkey");

                entity.ToTable("discord_user");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.AnilistUsername).HasColumnName("anilist_username");

                entity.Property(e => e.AnimeDays).HasColumnName("anime_days");

                entity.Property(e => e.ListPreference).HasColumnName("list_preference");

                entity.Property(e => e.MalUsername).HasColumnName("mal_username");

                entity.Property(e => e.MangaDays).HasColumnName("manga_days");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
