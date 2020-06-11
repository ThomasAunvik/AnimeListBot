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
using AnimeListBot.Handler;
using AnimeListBot.Handler.Database;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace AnimeListBot.Modules
{
    public class HelpModule : ModuleBase<ShardedCommandContext>
    {
        private CommandService _service;
        private readonly IServiceProvider _services;
        private IDatabaseService _db;

        public HelpModule(CommandService service, IServiceProvider services, IDatabaseService db)
        {
            _service = service;
            _services = services;
            _db = db;
        }

        [Command("help")]
        [Summary("Displays basic help command.")]
        public virtual async Task HelpAsync([Remainder]string command = null)
        {
            if (command == null)
            {
                await HelpAsync();
                return;
            }

            EmbedHandler embed = GetCommandHelp(command, Context);
            await embed.SendMessage(Context.Channel);
        }

        public static EmbedHandler GetCommandHelp(string command, ICommandContext context)
        {
            CommandService commands = Program._services.GetService(typeof(CommandService)) as CommandService;
            var result = commands.Search(context, command);

            var builder = new EmbedHandler(context.User, $"Command: **{command}**");

            if (!result.IsSuccess)
            {
                builder.Title = $"Sorry, couldn't find a command like {command}.";
                builder.Description = "";
                return builder;
            }

            builder.Description = result.Commands.FirstOrDefault().Command.Summary;

            bool isOwner = Program.botOwners.Contains(context.User.Id);

            foreach (var match in result.Commands)
            {
                var cmd = match.Command;
                builder.AddField(x =>
                {
                    x.Name = string.Join(", ", cmd.Aliases);
                    x.Value = $"**Usage:** {cmd.Aliases.First()} {string.Join(" ", cmd.Parameters.Select(p => p.IsOptional ? $"[{p.Name}]" : $"<{p.Name}>"))}\n" +
                             $"{cmd.Remarks}";
                    x.IsInline = false;
                });

                if (cmd.Preconditions.Count > 0)
                {
                    List<PreconditionAttribute> preconditions = cmd.Preconditions.ToList();
                    List<PreconditionAttribute> sameGroup = new List<PreconditionAttribute>();
                    string preconditionString = string.Empty;
                    for (int i = 0; i < preconditions.Count; i++)
                    {
                        PreconditionAttribute attrib = cmd.Preconditions[i];
                        var same = preconditions.FindAll(x => x.Group == attrib.Group && x.Group != null);
                        if (same.Count != 0)
                        {
                            sameGroup.AddRange(same);
                            i += same.Count - 1;
                        }
                        else sameGroup.Add(attrib);

                        preconditionString += string.Join("\n**Or**\n", sameGroup.Select(x =>
                        {
                            if (x is RequireUserPermissionAttribute userPerm) return userPerm.ChannelPermission != null ? ("User Channel Permission: " + userPerm.ChannelPermission.ToString()) : ("User Guild Permission: " + userPerm.GuildPermission.ToString());
                            if (x is RequireBotPermissionAttribute botPerm) return botPerm.ChannelPermission != null ? ("Bot Channel Permission: " + botPerm.ChannelPermission.ToString()) : ("Bot Guild Permission: " + botPerm.GuildPermission.ToString());
                            if (x is RequireValidAnimelistAttribute) return "Valid Animelist Profile";
                            if (x is RequireOwnerAttribute) return "Bot Owner Permission";
                            return x.GetType().Name;
                        })
                        ) + "\n";

                        sameGroup = new List<PreconditionAttribute>();
                    }

                    builder.AddField(x =>
                    {
                        x.Name = "Preconditions";
                        x.Value = preconditionString;
                        x.IsInline = false;
                    });
                }
            }
            if (builder.Fields.Count <= 0)
            {
                builder.Title = $"Sorry, couldn't find a command like {command}.";
                builder.Description = "";
            }
            return builder;
        }

        [Command("contact")]
        [Summary("Send a message directly to the developer")]
        public async Task Contact([Remainder] string message = null)
        {
            EmbedHandler embed = new EmbedHandler(Context.User);

            const ulong ownerId = 96580514021912576;
            IUser owner = Context.Client.GetUser(ownerId);
            var dmOwner = await owner?.GetOrCreateDMChannelAsync();
            var dmRequestor = await Context.User.GetOrCreateDMChannelAsync();

            if (string.IsNullOrWhiteSpace(message))
            {
                embed.AddFieldSecure("Bot Contacter", "Welcome, This is the contact service to message the developer.\n" +
                    "To directly contact the developer use the command as following `.contact whatever you want the developer to know` And he will contact you as soon as possible.\n" +
                    "Please join the support server if more help is required: https://discord.gg/Q9cf46R"
                    );

                await embed.SendMessage(dmRequestor);
            }

            embed.Fields.Clear();
            message = string.IsNullOrWhiteSpace(message) ? "Just a false alarm" : message;
            embed.Title = "Bot Contactor";
            embed.AddFieldSecure("Message", message);
            embed.AddFieldSecure("Info", $"Guild: {Context.Guild.Name} ({Context.Guild.Id})\nUser: {Context.User.Mention} ({Context.User.Username}#{Context.User.Discriminator} {Context.User.Id})");
            await embed.SendMessage(dmOwner);

            EmbedHandler embedRecieved = new EmbedHandler(Context.User, "Message sent to Bot Owner");
            embedRecieved.AddFieldSecure("Message", message);
            embedRecieved.AddFieldSecure("Support Server", "https://discord.gg/Q9cf46R");
            await embedRecieved.SendMessage(Context.Channel);
        }

        [Command("invite")]
        [Summary("Get an invite for this bot, to let it join other servers")]
        public async Task Invite() => await ReplyAsync($"{Program.inviteURL}");

        private async Task HelpAsync()
        {
            var foo = await Context.Client.GetApplicationInfoAsync();

            DiscordServer server = await _db.GetServerById(Context.Guild.Id);

            var builder = GetHelpEmbed();
            builder.Description = $"These are the commands you can use \nFor more detailed command explanations type `{server.Prefix}help <command>`";

            await builder.SendMessage(Context.Channel);
        }

        public async Task ContextlessHelpAsync(string command, ISocketMessageChannel channel)
        {
            var result = _service.Search(Context, command);

            var builder = GetHelpEmbed();

            await channel.SendMessageAsync("", false, builder.Build());
        }
        
        public EmbedHandler GetHelpEmbed()
        {
            IUser contextUser = Context?.User;

            var builder = new EmbedHandler(contextUser);
            builder.Description = $"These are the commands you can use \nFor more detailed command explanations type `{Program.botPrefix}help <command>`";

            foreach (var module in _service.Modules)
            {
                string description = string.Join(", ", module.Commands
                     .Select(x => x)
                     .Where(x => x.CheckPreconditionsAsync(Context, _services).Result.IsSuccess)
                     .Select(x => $"`'{x.Aliases.First()}'`")
                     .Distinct()
                 );

                if (!string.IsNullOrWhiteSpace(description))
                {
                    var name = module.Name.Contains("Module") ? module.Name.Substring(0, module.Name.Length - "Module".Length) : module.Name;
                    if (name == typeof(Administrator).Name && !Program.botOwners.Contains(Context.User.Id)) continue;
                    builder.AddField(x =>
                    {
                        x.Name = name;
                        x.Value = description;
                        x.IsInline = false;
                    });
                }
            }
            return builder;
        }

        [Command("github")]
        [Summary("Gets the GitHub link of the project")]
        public async Task GithubLink()
        {
            await ReplyAsync("AnimeList Bot Project: https://github.com/ThomasAunvik/AnimeListBot");
        }

        [Command("gitstatus")]
        public async Task GitStatus()
        {
            EmbedHandler embed = new EmbedHandler(Context.User, "Git Status");

            string commitText = "None";
            if (!string.IsNullOrEmpty(Program.currentCommit))
            {
                commitText = "[" + Program.currentCommit.Substring(0, 7) + "](" +
                             "https://github.com/ThomasAunvik/AnimeListBot/commit/" + Program.currentCommit + ")";
            }

            embed.AddFieldSecure("Commit", commitText);
            embed.AddFieldSecure("Status", string.IsNullOrEmpty(Program.gitStatus) ? "None" : Program.gitStatus);
            await embed.SendMessage(Context.Channel);
        }

        [Command("prefix")]
        [RequireUserPermission(GuildPermission.Administrator, Group = "Admin")]
        [RequireOwner(Group = "Admin")]
        public async Task Prefix(string newPrefix = "")
        {
            EmbedHandler embed = new EmbedHandler(Context.User);
            DiscordServer server = await _db.GetServerById(Context.Guild.Id);
            if (string.IsNullOrEmpty(newPrefix))
            {
                embed.Title = "Current Prefix";
                embed.Description = "`" + server.Prefix + "`";
                await embed.SendMessage(Context.Channel);
                return;
            }

            if (newPrefix.Length > 3)
            {
                embed.Title = "Prefix length is too large (Max 3 characters)";
                await embed.SendMessage(Context.Channel);
                return;
            }

            server.Prefix = newPrefix;
            await _db.SaveChangesAsync();

            embed.Title = "Prefix Set to";
            embed.Description = "`" + server.Prefix + "`";
            await embed.SendMessage(Context.Channel);
        }
    }
}
