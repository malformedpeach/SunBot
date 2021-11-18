using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using SunBot.Services;
using Discord.Audio;
using System.Linq;

namespace SunBot
{
    class Program
    {
        public static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            using var services = ConfigureServices();
            var config = services.GetRequiredService<IConfiguration>();

            if (config.Bot == null)
            {
                Console.Write("Press any key to continue..");
                Console.ReadKey();
                return;
            }

            var client = services.GetRequiredService<DiscordSocketClient>();
            client.GuildAvailable += config.InitializeDefaultChannel;

            await client.LoginAsync(TokenType.Bot, config.Bot.Token);
            await client.StartAsync();

            await services.GetRequiredService<CommandHandler>().InstallCommandsAsync();

            await Task.Delay(-1);
        }

        private ServiceProvider ConfigureServices()
        {
            var client = new DiscordSocketClient(new DiscordSocketConfig
            {
                ExclusiveBulkDelete = true
            });
            client.Log += Log;
            client.UserJoined += AnnounceUserJoined;

            return new ServiceCollection()
                .AddSingleton<IConfiguration, Configuration>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandler>()
                .AddSingleton<DiscordSocketClient>(client)
                .AddSingleton<AudioService>()
                .BuildServiceProvider();
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private Task AnnounceUserJoined(SocketGuildUser user)
        {
            var defaultChannel = user.Guild.TextChannels.FirstOrDefault(x => x.Name == "general");

            if (defaultChannel == null) defaultChannel = user.Guild.DefaultChannel;

            defaultChannel.SendMessageAsync($"Welcome to {user.Guild.Name}, {user.Mention}!");
            return Task.CompletedTask;
        }
    }
}
