using System;
using System.Collections.Generic;
using System.IO;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MALBot.Handler
{
    public class DiscordServer
    {
        private SocketGuild _guild;
        public SocketGuild Guild { get { return _guild; } }

        public SaveDiscordServer discordServerSave;

        //Savings
        public List<ServerUser> Users = new List<ServerUser>();

        public ulong animeListChannelId = 0;

        public List<(ulong roleId, decimal days)> animeRoles = new List<(ulong roleId, decimal days)>();

        public DiscordServer(SocketGuild guild)
        {
            _guild = guild;
            LoadData();

            if (Users == null)
                Users = new List<ServerUser>();
            foreach (SocketGuildUser user in Guild.Users)
                if (GetUserFromId(user.Id) == null)
                    Users.Add(new ServerUser(user));
                else
                    GetUserFromId(user.Id).UpdateInfo(user);

            SaveData();
        }

        public SaveDiscordServer LoadData()
        {
            if (File.Exists("DiscordServerFiles/" + _guild.Id + ".json"))
            {
                string JSONstring = File.ReadAllText("DiscordServerFiles/" + _guild.Id + ".json");
                SaveDiscordServer save = JsonConvert.DeserializeObject<SaveDiscordServer>(JSONstring);
                if (save != null)
                {
                    discordServerSave = save;
                    if (save.users != null)
                        Users = save.users;

                    if(save.animeRoles != null)
                        animeRoles = save.animeRoles;

                    return save;
                }
            }
            return null;
        }

        public void SaveData()
        {
            discordServerSave = new SaveDiscordServer(this);

            string outputJSON = JsonConvert.SerializeObject(discordServerSave);

            string jsonFormatted = JToken.Parse(outputJSON).ToString(Formatting.Indented);

            FileStream stream = null;
            if (!Directory.Exists("DiscordServerFiles/"))
                Directory.CreateDirectory("DiscordServerFiles/");
            if (!File.Exists("DiscordServerFiles/" + _guild.Id + ".json"))
                stream = File.Create("DiscordServerFiles/" + _guild.Id + ".json");
            if (stream != null)
                stream.Close();
            File.WriteAllText("DiscordServerFiles/" + _guild.Id + ".json", jsonFormatted);
        }

        public static void DeleteServerFile(SocketGuild guild)
        {
            if (File.Exists("DiscordServerFiles / " + guild.Id + ".json"))
                File.Delete("DiscordServerFiles / " + guild.Id + ".json");
        }

        public static DiscordServer GetServerFromID(ulong id)
        {
            return Program.discordServers.Find(x => x.Guild.Id == id);
        }

        public static ServerUser GetUserFromId(DiscordServer server, ulong id)
        {
            return server.GetUserFromId(id);
        }

        public ServerUser GetUserFromId(ulong id)
        {
            return Users.Find(x => x.userID == id);
        }
    }

    public class SaveDiscordServer
    {
        public string serverName;
        public ulong serverId;
        public List<(ulong roleId, decimal days)> animeRoles;

        public List<ServerUser> users;

        public SaveDiscordServer(DiscordServer server)
        {
            if (server != null)
            {
                if (server.Guild != null)
                {
                    serverName = server.Guild.Name;
                    serverId = server.Guild.Id;
                }

                if (server.animeRoles != null)
                    animeRoles = server.animeRoles;

                if (server.Users != null)
                    users = server.Users;
            }
        }
    }
}