using Discord.WebSocket;
using SunBot.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SunBot.Services
{
    public interface IConfiguration
    {
        public Bot Bot { get; set; }
        public SocketTextChannel DefaultTextChannel { get; set; }

        public Task InitializeDefaultChannel(SocketGuild guild);
        public void SaveToAppSettings();
    }
}
