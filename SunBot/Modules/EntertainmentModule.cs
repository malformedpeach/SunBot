using Discord.Commands;
using SunBot.Models.Enums;
using SunBot.Services;
using System.Threading.Tasks;

namespace SunBot.Modules
{
    public class EntertainmentModule : ModuleBase<SocketCommandContext>
    {
        public BlackjackService Service { get; set; } 

        [Command("blackjack")]
        [Summary("**Classic game of blackjack!** Somewhat modified rules for more interactivity!\n" +
                 "use the command with one of the following 'actions'.\n\n" +
                 "**start**: start game.\n" +
                 "**join**: join table.\n" +
                 "**leave**: leave table.\n" +
                 "**clear**: remove all players from table.\n" +
                 "**rules**: review the rules used in this implementation.")] // TODO: v fix this summary
        public async Task BlackJackAsync([Summary("start/join/leave/clear/rules")]string action = "")
        {
            if (action == "")
            {
                Service.StartSession();
            }

            // old
            //if (action == "start")
            //    Service.StartGameAsync();
            //else if (action == "join")
            //    Service.JoinTable(Context.User);
            //else if (action == "leave")
            //    Service.LeaveTable(Context.User.Id);
            //else if (action == "clear")
            //    Service.ClearTable();
            //else if (action == "rules")
            //    Service.GetRules();
        }
    }
}
