using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace SunBot.Modules
{
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        private DiscordSocketClient _client;

        public InfoModule(IServiceProvider service)
        {
            _client = service.GetRequiredService<DiscordSocketClient>();
            _client.UserJoined += AnnounceJoinedUser;
        }

        public async Task AnnounceJoinedUser(SocketGuildUser user)
        {
            // Additional styling and logic to check if bot user joined etc
            
            //var channel = user.Guild.DefaultChannel;
            //await channel.SendMessageAsync($"Welcome! {user.Username}");
        }

        [Command("say")]
        [Summary("Echoes a message.")]
        [Alias("echo")]
        public Task SayAsync([Remainder] [Summary("The text to echo")] string echo) => ReplyAsync(echo);

        [Command("help")]
        [Summary("Info on commands.")]
        public async Task HelpAsync()
        {
            await ReplyAsync("foobar");
        }
    }
}
