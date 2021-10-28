using Discord;
using Discord.Audio;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace SunBot.Services
{
    public class AudioService
    {
        public IAudioClient AudioClient { get; set; }

        public async Task JoinVoiceChannelAsync(IVoiceChannel channel) 
        {
            AudioClient = await channel.ConnectAsync();
        }

        public async Task LeaveVoiceChannelAsync(IVoiceChannel channel)
        {
            await channel.DisconnectAsync();
        }

        public async Task PlaySongAsync(string songUrl)
        {
            //if(AudioClient.ConnectionState != ConnectionState.Connected)
            //{
            //    // Connect
            //}

            var youtube = new YoutubeClient();

            var video = await youtube.Videos.GetAsync(songUrl);

            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
            var url = streamInfo.Url;

            using (var mf = new MediaFoundationReader(url))
            {
                var outputStream = AudioClient.CreatePCMStream(AudioApplication.Music);

                await AudioClient.SetSpeakingAsync(true);
                await mf.CopyToAsync(outputStream);
                await outputStream.FlushAsync();
                await AudioClient.SetSpeakingAsync(false);
            }
        }
    }
}
