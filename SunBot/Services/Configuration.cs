using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SunBot.Services
{
    class Configuration
    {
        public Bot Bot { get; set; }

        public Configuration()
        {
            this.Bot = JsonConvert.DeserializeObject<Bot>(File.ReadAllText("appsettings.json"));
        }
    }

    class Bot
    {
        public string Token { get; set; }
        public char Prefix { get; set; }
    }
}
