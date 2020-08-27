using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SunBot
{
    class Program
    {
        public static void Main(string[] args) 
            => new Program().MainAsync().GetAwaiter().GetResult();


        private DiscordSocketClient _client;
        private Configuration _config;

        public async Task MainAsync()
        {
            _config = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText("appsettings.json"));

            _client = new DiscordSocketClient();
            _client.Log += Log;

            await _client.LoginAsync(TokenType.Bot, _config.BotToken);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
