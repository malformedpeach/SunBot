using Discord;
using Discord.WebSocket;
using SunBot.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SunBot.Models
{
    public class BlackjackPlayer
    {
        public PlayerState State { get; set; }
        public IUser User { get; set; }
        public List<Card> Cards { get; set; }
        public int Points 
        { 
            get 
            {
                if (Cards.Any())
                {
                    return Cards.Sum(x => x.Value);
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}
