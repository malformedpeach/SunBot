﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using SunBot.Services;

namespace SunBot.Modules
{
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        private DiscordSocketClient _client;
        private Configuration _config;

        public InfoModule(IServiceProvider service)
        {
            _client = service.GetRequiredService<DiscordSocketClient>();
            _config = service.GetRequiredService<Configuration>();
            _client.UserJoined += AnnounceJoinedUser;
        }

        public async Task AnnounceJoinedUser(SocketGuildUser user)
        {
            //var embed = new EmbedBuilder
            //{
            //    Title = "Welcome!",
            //    Description = $"Welcome to {user.Guild.Name}, {user}."
            //};

            var channel = user.Guild.DefaultChannel;

            await channel.SendMessageAsync($"Welcome to {user.Guild.Name}, {user}");
        }

        [Command("say")]
        [Summary("Echoes a message.")]
        [Alias("echo")]
        public Task SayAsync([Remainder] [Summary("The text to echo")] string echo) => ReplyAsync(echo);

        [Command("help")]
        [Summary("Info on commands.")]
        public async Task HelpAsync()
        {
            var embed = new EmbedBuilder
            {
                Title = "Bot Commands",
                Description = $"The prefix of the bot is `{_config.Bot.Prefix}`",
                Color = Color.Gold
            };
            embed.AddField("General", "`say`, `help`")
                 .AddField("Admin", "`ban`, `kick`, `clear`")
                 .WithFooter($"For more information use {_config.Bot.Prefix}help (command)");


            await ReplyAsync(embed: embed.Build());
        }

        
    }
}
