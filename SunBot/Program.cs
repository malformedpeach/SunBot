using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using SunBot.Services;

namespace SunBot
{
    class Program
    {
        public static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            using (var services = ConfigureServices())
            {
                var config = services.GetRequiredService<Configuration>();
                if (config.Bot == null) return;
                
                var client = services.GetRequiredService<DiscordSocketClient>();
                client.Log += Log;


                await client.LoginAsync(TokenType.Bot, config.Bot.Token);
                await client.StartAsync();

                await services.GetRequiredService<CommandHandler>().InstallCommandsAsync();

                await Task.Delay(-1);
            }
        }

        private ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<Configuration>()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandler>()
                .BuildServiceProvider();
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
