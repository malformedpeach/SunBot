using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using SunBot.Services;
using System;
using System.Threading.Tasks;

using NAudio.Wave;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using Discord.Audio;

namespace SunBot.Modules
{
    public class MusicModule : ModuleBase<SocketCommandContext>
    {
        public AudioService Service { get; set; }


        [Command("join", RunMode = RunMode.Async)]
        public async Task JoinAsync()
        {
            var voiceChannel = (Context.User as IGuildUser)?.VoiceChannel;
            var textChannel = Context.Message.Channel;
            
            var resultEmbed = await Service.JoinVoiceChannelAsync(voiceChannel, textChannel);
            if (resultEmbed.Description != string.Empty) await ReplyAsync(embed: resultEmbed);
        }

        [Command("leave", RunMode = RunMode.Async)]
        public async Task LeaveAsync()
        {
            var channel = (Context.User as IGuildUser)?.VoiceChannel;
            await Service.LeaveVoiceChannelAsync(channel);
        }

        [Command("play", RunMode = RunMode.Async)]
        public async Task PlayAsync(string songUrl = "")
        {
            var voiceChannel = (Context.User as IGuildUser)?.VoiceChannel;
            var textChannel = Context.Message.Channel;

            var resultMessage = await Service.PlaySongAsync(voiceChannel, textChannel, songUrl);
            await ReplyAsync(embed: resultMessage);
        }

        [Command("stop", RunMode = RunMode.Async)]
        public async Task StopAsync()
        {
            await Service.StopSongAsync();
        }
        
        [Command("skip", RunMode = RunMode.Async)]
        public async Task SkipAsync()
        {
            await Service.SkipSongAsync();
        }

        [Command("queue", RunMode = RunMode.Async)]
        public async Task QueueAsync()
        {
            var resultEmbed = Service.GetCurrentQueue();
            await ReplyAsync(embed: resultEmbed);
        }

    }
}
