using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using SunBot.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SunBot.Modules
{
    [Group("help")]
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private Configuration _config;

        public HelpModule(IServiceProvider services)
        {
            _config = services.GetRequiredService<Configuration>();
        }

        [Command("say")]
        [Summary("Info on command \"say\"")]
        public async Task HelpSayAsync()
        {
            var embed = new EmbedBuilder
            {
                Title = "Say Command",
                Description = "Echoes a message.",
                Color = Color.Gold
            };
            embed.AddField("Example", $"`{_config.Bot.Prefix}say Hello world!`");

            await ReplyAsync(embed: embed.Build());
        }
    }
}
