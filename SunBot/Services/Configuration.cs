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
    public class Configuration : IConfiguration
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
                Console.WriteLine($"Configuration error: Take a look at your appsettings.json.\n" +
                                  $"Exception message: {ex.Message}");
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
            var json = JsonConvert.SerializeObject(Bot);

            if (File.Exists("appsettings.json")) File.WriteAllText("appsettings.json", json);
        }
    }
}
