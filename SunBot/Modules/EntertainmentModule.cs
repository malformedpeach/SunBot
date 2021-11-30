using Discord.Commands;
using SunBot.Models.Enums;
using SunBot.Services;
using System;
using System.Threading.Tasks;

namespace SunBot.Modules
{
    public class EntertainmentModule : ModuleBase<SocketCommandContext>
    {
        public BlackjackService Service { get; set; } 

        [Command("blackjack")]
        [Summary("Classic game of blackjack\n Use this command to start a blackjack session")] 
        public async Task BlackJackAsync()
        {
            Service.StartSession();
        }

        [Command("roll")]
        [Summary("Roll a number 1-100\n" +
                 "Provide a number to set the max value for the roll")]
        public async Task RollAsync([Summary("5")]int max = 0)
        {
            Random random = new Random();
            await ReplyAsync($"{random.Next(1, max == 0 ? 100 : max)}");
        }
    }
}
