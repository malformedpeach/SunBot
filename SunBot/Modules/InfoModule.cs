using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using SunBot.Services;

namespace SunBot.Modules
{
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        //private DiscordSocketClient _client;
        //private Configuration _config;

        //public InfoModule(IServiceProvider service)
        //{
        //    _client = service.GetRequiredService<DiscordSocketClient>();
        //    _config = service.GetRequiredService<Configuration>();
        //    _client.UserJoined += AnnounceJoinedUser;
        //    _client.SetGameAsync($"{_config.Bot.Prefix}help");
        //}

        //public async Task AnnounceJoinedUser(SocketGuildUser user)
        //{
        //    var channel = user.Guild.DefaultChannel;

        //    await channel.SendMessageAsync($"Welcome to {user.Guild.Name}, {user}");
        //}

        //[Command("say")]
        //[Summary("Echoes a message.")]
        //[Alias("echo")]
        //public Task SayAsync([Remainder] [Summary("The text to echo")] string echo) => ReplyAsync(echo);
    }
}
