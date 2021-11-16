﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using SunBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SunBot.Modules
{
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        [Command("say", RunMode = RunMode.Async)]
        [Summary("Echoes a message")]
        public async Task SayAsync([Summary("Message to echo")]string echo)
        {
            await ReplyAsync(echo);
        } 

        [Command("kick", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task KickAsync(SocketGuildUser user, [Remainder]string kickReason = "")
        {
            if (user == Context.Guild.Owner) await ReplyAsync("Big mistake homie!");
            else
            {
                await user.KickAsync(kickReason);
            }
        }

        [Command("ban", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task BanAsync(SocketGuildUser user, [Remainder]string banReason = "")
        {
            if (user == Context.Guild.Owner) await ReplyAsync("Big mistake homie!");
            else
            {
                await user.BanAsync(reason: banReason);
            }
        }

        [Command("clear", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ClearAsync(int amount)
        {
            var messages = await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync();

            await (Context.Channel as ITextChannel).DeleteMessagesAsync(messages);

            var deleteMessage = await ReplyAsync($"I removed {amount} messages! this message will be removed in 5 seconds.");
            await Task.Delay(5000);
            await deleteMessage.DeleteAsync();
        }
    }
}
