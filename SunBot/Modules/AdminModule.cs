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
        public Configuration Config { get; set; }


        [Command("say", RunMode = RunMode.Async)]
        [Summary("Echoes a message")]
        public async Task SayAsync([Summary("Message to echo")][Remainder]string echo)
        {
            await ReplyAsync(echo);
        } 

        [Command("kick", RunMode = RunMode.Async)]
        [Summary("Kick a user from the server")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task KickAsync([Summary("@User")]SocketGuildUser user, [Summary("reason for kick (Optional)")][Remainder]string kickReason = "")
        {
            if (user == Context.Guild.Owner) await ReplyAsync("Big mistake homie!");
            else
            {
                await user.KickAsync(kickReason);
            }
        }

        [Command("ban", RunMode = RunMode.Async)]
        [Summary("Ban a user from the server")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task BanAsync([Summary("@User")]SocketGuildUser user, [Summary("reason for ban (Optional)")][Remainder]string banReason = "")
        {
            if (user == Context.Guild.Owner) await ReplyAsync("Big mistake homie!");
            else
            {
                await user.BanAsync(reason: banReason);
            }
        }

        [Command("clear", RunMode = RunMode.Async)]
        [Summary("Clear a specified amount of messages from a text channel")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ClearAsync([Summary("5")]int amount)
        {
            var messages = await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync();

            await (Context.Channel as ITextChannel).DeleteMessagesAsync(messages);

            var deleteMessage = await ReplyAsync($"I removed {amount} messages! this message will be removed in 5 seconds.");
            await Task.Delay(5000);
            await deleteMessage.DeleteAsync();
        }

        [Command("setdefaultchannel")]
        [Summary("Set my default response channel")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetDefaultChannel([Summary("#channel")] SocketTextChannel channel)
        {
            if (channel == null) return;
            else
            {
                Config.DefaultTextChannel = channel;
                Config.WriteToAppSettings();
            }
        }

        [Command("setprefix", RunMode = RunMode.Async)]
        [Summary("Set my summon signature (prefix)")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetBotPrefix(string prefix)
        {
            if (char.TryParse(prefix, out char newPrefix))
            {
                Config.Bot.Prefix = newPrefix;
                Config.WriteToAppSettings();
            }
            else
            {
                await ReplyAsync("You done goofed");
            }
        }
    }
}
