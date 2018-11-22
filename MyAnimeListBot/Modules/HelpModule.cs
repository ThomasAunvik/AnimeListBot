using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MALBot.Modules
{
    public class HelpModule : ModuleBase<ICommandContext>
    {
        private CommandService _service;
        private readonly IServiceProvider _services;

        public HelpModule(CommandService service, IServiceProvider services)
        {
            _service = service;
            _services = services;
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

            var result = _service.Search(Context, command);

            if (!result.IsSuccess)
            {
                await ReplyAsync($"Sorry, couldn't find a command like **{command}**.");
                return;
            }

            var builder = new EmbedBuilder()
            {
                Color = Program.embedColor,
                Title = $"Command: **{command}**",
                Description = $"{result.Commands.FirstOrDefault().Command.Summary}"
            };

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
            }

            await ReplyAsync("", false, builder.Build());
        }

        [Command("contact")]
        [Summary("Send a message directly to the developer")]
        public async Task Contact([Remainder] string message = null)
        {
            const ulong ownerId = 96580514021912576;
            IUser owner = await Context.Client.GetUserAsync(ownerId);
            var dmOwner = await owner?.GetOrCreateDMChannelAsync();
            var dmRequestor = await Context.User.GetOrCreateDMChannelAsync();

            if (string.IsNullOrWhiteSpace(message))
            {
                await dmRequestor.SendMessageAsync("Welcome, This is the contact service to message the developer.\n" +
                    "To directly contact the developer use the command as following `.contact whatever you want the developer to know` And he will contact you as soon as possible.\n"
                );
            }
            message = string.IsNullOrWhiteSpace(message) ? "Just a false alarm" : message;
            await dmOwner.SendMessageAsync($"{Context.Guild.Name}:{Context.User.Mention} ({Context.User.Username}#{Context.User.Discriminator} {Context.User.Id}) ({Context.Guild.Id}) {message}");
        }

        [Command("invite")]
        [Summary("Get an invite for this bot, to let it join other servers")]
        public async Task Invite() => await ReplyAsync($"{Program.inviteURL}");

        private async Task HelpAsync()
        {
            var foo = await Context.Client.GetApplicationInfoAsync();

            var builder = new EmbedBuilder()
            {
                Color = Program.embedColor,
                Description = $"These are the commands you can use \nFor more detailed command explanations type `{ Program.botPrefix }help <command>`"
            };

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
                    builder.AddField(x =>
                    {
                        x.Name = name;
                        x.Value = description;
                        x.IsInline = false;
                    });
                }
            }

            await ReplyAsync("", false, builder.Build());
        }

        public async Task ContextlessHelpAsync(string command, ISocketMessageChannel channel)
        {
            var result = _service.Search(Context, command);

            var builder = new EmbedBuilder()
            {
                Color = Program.embedColor,
                Title = $"Command: **{command}**",
                Description = $"{result.Commands.FirstOrDefault().Command.Summary}"
            };

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
            }

            await channel.SendMessageAsync("", false, builder.Build());
        }

        [Command("github")]
        [Summary("Gets the GitHub link of the project")]
        public async Task GithubLink()
        {
            await ReplyAsync("Hifumi Project: https://github.com/ThomasAunvik/DiscordTemplateBot");
        }
    }
}