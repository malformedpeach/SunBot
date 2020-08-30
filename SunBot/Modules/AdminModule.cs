using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using SunBot.Services;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SunBot.Modules
{
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        private Configuration _config;
        private IServiceProvider _service;

        public AdminModule(IServiceProvider service)
        {
            _config = service.GetRequiredService<Configuration>();
            _service = service;
        }

        [Command("ban")]
        [Alias("banuser")]
        [Summary("Bans a specified user")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task BanUserAsync([Summary("User to ban")] SocketUser user = null, [Remainder][Summary("Reason for ban")]string banReason = null)
        {
            if (user == null)
            {
                var embed = new EmbedBuilder
                {
                    Title = "Command error",
                    Description = "Mention a **user** to ban when using this command. \nOptional parameter: **ban reason**",
                    Color = Color.Gold,
                };
                embed.AddField("Example", $"{_config.Bot.Prefix}ban @user **ban reason**\n{_config.Bot.Prefix}banuser @user **ban reason**", false);

                await ReplyAsync(embed: embed.Build());
            }
            else
            {
                // Doesn't work, figure out why
                // Solution: didn't work because i never authorized the bot to ban users. 
                await Context.Guild.AddBanAsync(user: user, reason: banReason);
                await ReplyAsync($"{user} banned for: {banReason ?? "No reason."}");
            }
        }
    }
}
