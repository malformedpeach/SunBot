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
            var channel = (Context.User as IGuildUser)?.VoiceChannel;
            var connected = await Service.JoinVoiceChannelAsync(channel);

            // TODO fix messages
            if (connected) await ReplyAsync($"Joined {channel.Name}.");
            else await ReplyAsync($"Could not join channel.");
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
            //var channel = (Context.User as IGuildUser)?.VoiceChannel;
            await Service.EnqueueSongAsync(songUrl);
        }

        [Command("stop", RunMode = RunMode.Async)]
        public async Task StopAsync()
        {
            //var channel = (Context.User as IGuildUser)?.VoiceChannel;
            Service.StopSongAsync();
        }
    }
}
