using Discord.Commands;
using SunBot.Models.Enums;
using SunBot.Services;
using System.Threading.Tasks;

namespace SunBot.Modules
{
    public class GameModule : ModuleBase<SocketCommandContext>
    {
        public BlackjackService Service { get; set; } 

        [Command("blackjack")]
        [Summary("Start a game of blackjack!")] // TODO: v fix this summary
        public async Task BlackJackAsync([Summary("Start/Hit/Stand")][Remainder]string action = "")
        {
            if (action.ToLower() == BlackjackAction.Start.ToString().ToLower())
            {
                // Shuffle deck and deal initial hand
                Service.StartGameAsync();
            }
            else if (action.ToLower() == BlackjackAction.Hit.ToString().ToLower())
            {
                // Deal card to player, check if bust
                Service.HitAsync();
            }
            else if (action.ToLower() == BlackjackAction.Stand.ToString().ToLower())
            {
                // Begin drawing cards for dealer
                Service.StandAsync();
            }
            else
            {
                await ReplyAsync("Please provide an action when using this command.\n" +
                                 "Use the `help` command for more info!");
            }
        }

        [Command("foo")]
        public async Task Foo()
        {
            Service.Foo();
        }
    }
}
