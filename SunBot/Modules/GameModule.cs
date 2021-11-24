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
        public async Task BlackJackAsync()
        {
            Service.StartGameAsync(Context.User);
        }

        [Command("foo")]
        public async Task Foo()
        {
            Service.Foo();
        }
    }
}
