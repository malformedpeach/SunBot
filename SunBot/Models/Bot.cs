using System;
using System.Collections.Generic;
using System.Text;

namespace SunBot.Models
{
    public class Bot
    {
        public string Token { get; set; }
        public char Prefix { get; set; }
        public ulong DefaultTextChannelId { get; set; }
    }
}
