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
    }

    class Bot
    {
        public string Token { get; set; }
        public char Prefix { get; set; }
    }
}
