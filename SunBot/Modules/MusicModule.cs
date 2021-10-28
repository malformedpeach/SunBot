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
        public AudioService _service { get; set; }


        [Command("join", RunMode = RunMode.Async)]
        public async Task JoinAsync()
        {
            var channel = (Context.User as IGuildUser)?.VoiceChannel;
            await _service.JoinVoiceChannelAsync(channel);
        }

        [Command("leave", RunMode = RunMode.Async)]
        public async Task LeaveAsync()
        {
            var channel = (Context.User as IGuildUser)?.VoiceChannel;
            await _service.LeaveVoiceChannelAsync(channel);
        }

        [Command("play", RunMode = RunMode.Async)]
        public async Task PlayAsync(string songUrl)
        {
            await _service.PlaySongAsync(songUrl);
        }
    }
}
