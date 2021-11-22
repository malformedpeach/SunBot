using Discord;
using Discord.Commands;
using SunBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunBot.Modules
{
    public class GameModule : ModuleBase<SocketCommandContext>
    {
        public BlackjackService Service { get; set; } 


        [Command("blackjack")]
        [Summary("Start a game of blackjack")]
        public async Task BlackJackAsync()
        {
            Card card = Service.DrawCard();
            var suit = card.Suit.ToString().ToLower();
            var embed = new EmbedBuilder
            {
                Title = "Testing!",
                Description = $"You drew: {card.Rank} of {card.Suit}"
            };

            await ReplyAsync(embed: embed.Build());
        }
    }
}
