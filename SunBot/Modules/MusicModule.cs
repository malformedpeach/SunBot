﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SunBot.Services;
using System.Linq;
using System.Threading.Tasks;

namespace SunBot.Modules
{
    public class MusicModule : ModuleBase<SocketCommandContext>
    {
        public AudioService Service { get; set; }


        [Command("join", RunMode = RunMode.Async)]
        [Summary("Join the voice channel invoking user is connected to")]
        public async Task JoinAsync()
        {
            var voiceChannel = (Context.User as IGuildUser)?.VoiceChannel;
            await Service.JoinVoiceChannelAsync(voiceChannel);
        }

        [Command("leave", RunMode = RunMode.Async)]
        [Summary("Leaves voice channel")]
        public async Task LeaveAsync()
        {
            await Service.LeaveVoiceChannelAsync();
        }

        [Command("play", RunMode = RunMode.Async)]
        [Summary("Plays/Enqueues the song from specified url")]
        public async Task PlayAsync([Summary("youtube url")]string songUrl = "")
        {
            var voiceChannel = (Context.User as IGuildUser)?.VoiceChannel;
            await Service.PlaySongAsync(voiceChannel, songUrl);
        }

        [Command("stop", RunMode = RunMode.Async)]
        [Summary("Stops playback of queue")]
        public async Task StopAsync()
        {
            await Service.StopSongAsync();
        }
        
        [Command("skip", RunMode = RunMode.Async)]
        [Summary("Skips the current song")]
        public async Task SkipAsync()
        {
            await Service.SkipSongAsync();
        }

        [Command("queue", RunMode = RunMode.Async)]
        [Summary("Displays a list of the enqueued songs")]
        public async Task GetQueueAsync()
        {
            await Service.GetCurrentQueue();
        }

        [Command("clearqueue", RunMode = RunMode.Async)]
        [Summary("Clears the queue")]
        public async Task ClearQueueAsync()
        {
            await Service.ClearQueueAsync();
        }

    }
}
