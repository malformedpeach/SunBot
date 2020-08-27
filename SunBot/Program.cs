using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace SunBot
{
    class Program
    {
        public static void Main(string[] args) 
            => new Program().MainAsync().GetAwaiter().GetResult();
        

        public async Task MainAsync()
        {
            DiscordSocketClient client;
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
