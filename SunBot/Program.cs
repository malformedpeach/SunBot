using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using SunBot.Services;
using System.Linq;
using SunBot.Util;

namespace SunBot
{
    class Program
    {
        public static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            using var services = ConfigureServices();
            if (services == null)
            {
                Console.WriteLine("Press any key to continue..");
                Console.ReadKey();
                return;
            }
            
            var config = services.GetRequiredService<Configuration>();
            var client = services.GetRequiredService<DiscordSocketClient>();

            await client.LoginAsync(TokenType.Bot, config.Bot.Token);
            await client.StartAsync();

            await services.GetRequiredService<CommandHandler>().InstallCommandsAsync();

            await Task.Delay(-1);
        }

        private ServiceProvider ConfigureServices()
        {
            var config = new Configuration();
            if (config.Bot == null) return null;

            var client = new DiscordSocketClient(new DiscordSocketConfig
            {
                ExclusiveBulkDelete = true
            });
            var commandService = new CommandService();
            Logger.InitializeLogging(client, commandService);
            config.InitializeClient(client);
            
            return new ServiceCollection()
                .AddSingleton(config)
                .AddSingleton(client)
                .AddSingleton(commandService)
                .AddSingleton<CommandHandler>()
                .AddSingleton<AudioService>()
                .BuildServiceProvider();
        }
    }
}
