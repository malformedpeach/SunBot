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
    /// <summary>
    /// Client configuration and util events
    /// </summary>
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
                Console.WriteLine($"Configuration error: Take a look at your appsettings.json.\n" +
                                  $"Exception message: {ex.Message}");
            }
        }
        
        public void WriteToAppSettings()
        {
            var json = JsonConvert.SerializeObject(Bot);

            if (File.Exists("appsettings.json")) File.WriteAllText("appsettings.json", json);
        }

        public void InitializeClient(DiscordSocketClient client)
        {
            client.GuildAvailable += InitializeDefaultChannel;
            client.UserJoined += AnnounceUserJoined;
            client.SetGameAsync($"{Bot.Prefix}help");
        }


        private Task InitializeDefaultChannel(SocketGuild guild)
        {
            DefaultTextChannel = guild.TextChannels.FirstOrDefault(x => x.Id == Bot.DefaultTextChannelId);
            if (DefaultTextChannel == null) DefaultTextChannel = guild.DefaultChannel;

            return Task.CompletedTask;
        }

        private async Task AnnounceUserJoined(SocketGuildUser user)
        {
            await DefaultTextChannel.SendMessageAsync($"Welcome to {user.Guild.Name}, {user.Mention}!");
        }
    }
}
