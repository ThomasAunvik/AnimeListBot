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
        public virtual DbSet<Cluster> Cluster { get; set; }


        private string GetConnectionString()
        {
            Config login = Config.GetConfig();
            return string.Format("Server={0};Port={1};" +
                    "User Id={2};Password={3};Database={4};",
                    login.ip, login.port, login.userid,
                    login.password, login.catalog);
        }

        public static async Task<DataSet> SendSql(string sql)
        {
            Config login = Config.GetConfig();

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

            modelBuilder.Entity<Cluster>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .HasName("Cluster_pkey");

                entity.ToTable("cluster");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedNever();

                entity.Property(e => e.ShardIdStart).HasColumnName("shard_id_start");

                entity.Property(e => e.ShardIdEnd).HasColumnName("shard_id_end");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
