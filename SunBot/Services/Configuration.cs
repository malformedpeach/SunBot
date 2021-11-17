using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using SunBot.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SunBot.Services
{
    public class Configuration
    {
        public Bot Bot { get; set; }
        public SocketTextChannel DefaultTextChannel { get; set; }


        public Configuration()
        {
            try
            {
                this.Bot = JsonConvert.DeserializeObject<Bot>(File.ReadAllText("appsettings.json"));
            }
            catch (Exception ex)
            {
                if (ex.InnerException.Message == "String must be exactly one character long.")
                    Console.WriteLine("Configuration error: Please set your desired prefix in appsettings.json.");
                else
                    Console.WriteLine($"Configuration error: {ex.Message}");
            }
        }

        public Task InitializeDefaultChannel(SocketGuild guild)
        {
            DefaultTextChannel = guild.TextChannels.FirstOrDefault(x => x.Id == Bot.DefaultTextChannelId);
            if (DefaultTextChannel == null) DefaultTextChannel = guild.DefaultChannel;

            return Task.CompletedTask;
        }

        public void SaveToAppSettings()
        {
            throw new NotImplementedException();
        }
    }
}
