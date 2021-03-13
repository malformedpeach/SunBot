using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using SunBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SunBot.Modules
{
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        private Configuration _config;

        public AdminModule(IServiceProvider service)
        {
            _config = service.GetRequiredService<Configuration>();
        }

        [Command("ban")]
        [Summary("Bans a specified user")]
        [Alias("banuser")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task BanUserAsync([Summary("User to ban")] SocketGuildUser user = null, [Remainder][Summary("Reason for ban")]string banReason = null)
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
                await Context.Guild.AddBanAsync(user: user, reason: banReason);
                await ReplyAsync($"{user} banned for: {banReason ?? "No reason."}");
            }
        }

        [Command("kick")]
        [Summary("Kicks a specified user")]
        [Alias("kickuser")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task KickUserAsync([Summary("User to kick")] SocketGuildUser user = null, [Remainder][Summary("Reason for kick")]string kickReason = null)
        {
            if (user == null)
            {
                var embed = new EmbedBuilder
                {
                    Title = "Command error",
                    Description = "Mention a **user** to kick when using this command.",
                    Color = Color.Gold
                };
                embed.AddField("Example", $"{_config.Bot.Prefix}kick @user **kick reason**\n{_config.Bot.Prefix}kickuser @user **kick reason**", false);

                await ReplyAsync(embed: embed.Build());
            }
            else
            {
                await user.KickAsync(kickReason);
                await ReplyAsync($"{user} kicked for: {kickReason ?? "No reason."}");
            }
        }

        [Command("clear")]
        [Summary("Clears a specified amount of messages from a text channel")]
        [Alias("purge")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ClearChannelAsync([Summary("Amount of messages to clear")]int amount)
        {
            var messages = await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync();
            await (Context.Channel as ITextChannel).DeleteMessagesAsync(messages);

            var purgeMessage = await ReplyAsync($"{amount} messages purged, this message will be deleted in 5 seconds.");
            await Task.Delay(5000);
            await purgeMessage.DeleteAsync();
        }
    }
}
