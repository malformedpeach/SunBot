using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SunBot.Modules
{
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        [Command("ban")]
        [Alias("banuser")]
        [Summary("Bans a specified user")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task BanUserAsync([Summary("User to ban")] SocketUser user = null)
        {
            if (user == null)
            {
                var embed = new EmbedBuilder
                {
                    Title = "Command error",
                    Description = "Enter a **user** to ban when using this command.",
                    Color = Color.Gold,
                };
                embed.AddField("Example", "banuser @user", false);

                await ReplyAsync(embed: embed.Build());
            }
            else
            {
                // Doesn't work, figure out why
                //await Context.Guild.AddBanAsync(user, 0, null);
                await ReplyAsync($"Banning user: {user.Username}#{user.Discriminator}");
            }
        }
    }
}
