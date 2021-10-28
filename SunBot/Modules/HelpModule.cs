using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using SunBot.Services;
using System;
using System.Threading.Tasks;

namespace SunBot.Modules
{
    [Group("help")]
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private Configuration _config;
        private CommandService _commandService;

        public HelpModule(IServiceProvider services)
        {
            _config = services.GetRequiredService<Configuration>();
            _commandService = services.GetRequiredService<CommandService>();
        }

        // Help Command
        [Command("")]
        [Summary("Info on commands.")]
        public async Task HelpAsync()
        {
            var commands = _commandService.Commands;

            var embed = new EmbedBuilder
            {
                Title = "Bot Commands",
                Description = $"The prefix of the bot is {_config.Bot.Prefix}",
                Color = Color.Gold
            };

            foreach (var command in commands)
            {
                if (string.IsNullOrEmpty(command.Name)) continue;

                embed.AddField("Title", command.Name);
                embed.AddField("Description", command.Summary);
            };
             
            await ReplyAsync(embed: embed.Build());
        }

        // Commands under InfoModule
        [Command("say")]
        [Summary("Info on command: \"say\"")]
        public async Task HelpSayAsync()
        {
            var embed = new EmbedBuilder
            {
                Title = "Say command",
                Description = "Echoes a message.",
                Color = Color.Gold
            };
            embed.AddField("Example", $"`{_config.Bot.Prefix}say Hello world!`");

            await ReplyAsync(embed: embed.Build());
        }

        // Commands under AdminModule
        [Command("ban")]
        [Summary("Info on command: \"ban\"")]
        public async Task HelpBanAsync()
        {
            var embed = new EmbedBuilder
            {
                Title = "Ban command",
                Description = "Bans a specified user.",
                Color = Color.Gold
            };
            embed.AddField("Parameters", "`user` - the user to be banned\n" +
                                         "`ban reason` - the reason for the ban (this parameter is optional!)");
            embed.AddField("Examples", $"{_config.Bot.Prefix}ban `user` `ban reason`\n" +
                                       $"{_config.Bot.Prefix}ban `@testuser` `toxic behaviour`\n" +
                                       $"{_config.Bot.Prefix}ban `@testuser`");

            await ReplyAsync(embed: embed.Build());
        }

        [Command("kick")]
        [Summary("Info on command: \"kick\"")]
        public async Task HelpKickAsync()
        {
            var embed = new EmbedBuilder
            {
                Title = "Kick command",
                Description = "Kicks a specified user.",
                Color = Color.Gold
            };
            embed.AddField("Parameters", "`user` - the user to be kicked\n" +
                                         "`kick reason` - the reason for the kick (this parameter is optional!)");
            embed.AddField("Examples", $"{_config.Bot.Prefix}kick `user` `kick reason`\n" +
                                       $"{_config.Bot.Prefix}kick `@testuser` `toxic behaviour`\n" +
                                       $"{_config.Bot.Prefix}kick `@testuser`");

            await ReplyAsync(embed: embed.Build());
        }

        [Command("clear")]
        [Summary("Info on command: \"clear\"")]
        public async Task HelpClearAsync()
        {
            var embed = new EmbedBuilder
            {
                Title = "Clear command",
                Description = "Clears a specified amount of messages from a text channel.",
                Color = Color.Gold
            };
            embed.AddField("Parameters", "`amount of messages` - the amount of messages to clear from the textchannel");
            embed.AddField("Examples", $"{_config.Bot.Prefix}clear `amount of messages`\n" +
                                       $"{_config.Bot.Prefix}clear `50`");

            await ReplyAsync(embed: embed.Build());
        }
    }
}
