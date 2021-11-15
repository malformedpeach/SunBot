using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using SunBot.Services;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunBot.Modules
{
    [Group("help")]
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private Configuration _config;
        private CommandService _commandService;

        public HelpModule(IServiceProvider services)
        {
            _config = services.GetRequiredService<Configuration>();
            _commandService = services.GetRequiredService<CommandService>();
        }

        [Command("", RunMode = RunMode.Async)]
        [Summary("Info on commands.")]
        public async Task HelpAsync(string query = "")
        {
            if (query == string.Empty)
            {
                var modules = _commandService.Modules;
                var embed = new EmbedBuilder();
                var builder = new StringBuilder();

                embed.Title = "Jolly co-operation!";
                embed.Color = Color.Gold;
                embed.Footer = new EmbedFooterBuilder
                {
                    Text = $"{_config.Bot.Prefix}help [command] for more info!",
                };

                
                builder.AppendLine($"Spot my summon signature easily by its brilliant aura `{_config.Bot.Prefix}`!\n");
                
                foreach (var module in modules)
                {
                    if (module.Name == "help") continue;

                    builder.AppendLine($"**{module.Name}**");
                    
                    foreach (var command in module.Commands)
                    {
                        builder.AppendLine($"> {command.Name}");
                    }

                    builder.AppendLine();
                }

                embed.Description = builder.ToString();
                await ReplyAsync(embed: embed.Build());
            }
            else
            {
                var commands = _commandService.Commands.ToList();
                var command = commands.Find(x => x.Name == query);
                var embed = new EmbedBuilder();

                if (command == null)
                {
                    embed.Description = $"Could not find the command: `{query}`";
                    embed.Color = Color.Red;
                    await ReplyAsync(embed: embed.Build());
                }
                else
                {
                    var builder = new StringBuilder();
                    builder.AppendLine($"{command.Summary}\n");
                    builder.AppendLine($"**Example**");

                    builder.Append($"{_config.Bot.Prefix}{command.Name} ");

                    foreach(var parameter in command.Parameters)
                    {
                        builder.Append($"`{parameter.Summary}` ");
                    }


                    embed.Title = $"{command.Name} command";
                    embed.Description = builder.ToString();
                    embed.Color = Color.Gold;

                    await ReplyAsync(embed: embed.Build());
                }
            }
        }
    }
}
