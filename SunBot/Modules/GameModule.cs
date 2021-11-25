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
        public async Task BlackJackAsync([Summary("start/join")]string action)
        {
            if (action == "start")
                Service.StartGameAsync();
            else if (action == "join")
                Service.JoinTable(Context.User);
        }
    }
}
