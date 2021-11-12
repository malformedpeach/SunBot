using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
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
        private IAudioClient _audioClient;
        private MediaFoundationReader _mediaReader;
        private AudioOutStream _outputStream;
        private Queue<Song> _songQueue = new Queue<Song>();
        private Song _currentSong;
        private bool _playing = false;
        private ISocketMessageChannel _responseChannel;

        public async Task<Embed> JoinVoiceChannelAsync(IVoiceChannel voiceChannel, ISocketMessageChannel textChannel) 
        {
            _responseChannel = textChannel; // I think i'm cool with this
            var embed = new EmbedBuilder();

            if (voiceChannel == null)
            {
                embed.Description = "Could not join channel.";
                embed.Color = Color.Red;

                return embed.Build();
            }
            else
            {
                _audioClient = await voiceChannel.ConnectAsync();
                return embed.Build();
            }
        }

        public async Task LeaveVoiceChannelAsync(IVoiceChannel channel)
        {
            await channel.DisconnectAsync();
        }
        
        public async Task<Embed> EnqueueSongAsync(IVoiceChannel voiceChannel, ISocketMessageChannel textChannel, string songUrl = "")
        {
            var embed = new EmbedBuilder();

            if (_audioClient == null)
            {
                var resultMessage =  await JoinVoiceChannelAsync(voiceChannel, textChannel);

                if (!string.IsNullOrEmpty(resultMessage.Description))
                {
                    return resultMessage;
                }
            }


            if (_playing)
            {
                Song song = await GetHighestBitrateUrlAsync(songUrl);
                _songQueue.Enqueue(song);

                embed.Description = $"Queued: [{song.Title}]({song.OriginalUrl})";
                embed.Color = Color.Green;

                return embed.Build();
            }
            else if (!string.IsNullOrEmpty(songUrl) && _songQueue.Count == 0)
            {
                _playing = true; // Wonky af? rework this
                Song song = await GetHighestBitrateUrlAsync(songUrl);
                _songQueue.Enqueue(song);

                await PlaySongAsync();
            }
            else if (string.IsNullOrEmpty(songUrl) && _songQueue.Count > 0)
            {
                _playing = true; // Wonky af? rework this
                await PlaySongAsync();
            }


            // Check if zero. If user stops playback we don't want to show the 'no more songs' message.
            if (_songQueue.Count == 0)
            {
                _playing = false;
                embed.Description = "No more songs in queue.";
                embed.Color = Color.Red;
            }

            return embed.Build();
        }
        
        public async Task StopSongAsync()
        {
            _playing = false;

            if (_outputStream != null) await _outputStream.DisposeAsync();
            if (_mediaReader != null) await _mediaReader.DisposeAsync();
        }

        public async Task SkipSongAsync()
        {
            var embed = new EmbedBuilder
            {
                Description = $"Skipping: [{_currentSong.Title}]({_currentSong.OriginalUrl})",
                Color = Color.Red
            };
            await _responseChannel.SendMessageAsync(embed: embed.Build());

            if (_outputStream != null) await _outputStream.DisposeAsync();
            if (_mediaReader != null) await _mediaReader.DisposeAsync();
        }

        public Embed GetCurrentQueue()
        {
            var embed = new EmbedBuilder();

            if (_songQueue.Count == 0)
            {
                embed.Description = "Queue is empty.";
                embed.Color = Color.Red;
            }
            else
            {
                var builder = new StringBuilder();
                var currentQueue = _songQueue.ToArray();
                builder.AppendLine($"Currently playing: [{_currentSong.Title}]({_currentSong.OriginalUrl})");

                for (int i = 0; i < currentQueue.Length; i++)
                {
                    builder.AppendLine($"{i + 1}. [{currentQueue[i].Title}]({currentQueue[i].OriginalUrl})");
                }

                embed.Description = builder.ToString();
                embed.Color = Color.Green;
            }

            return embed.Build();
        }

        private async Task<Song> GetHighestBitrateUrlAsync(string songUrl)
        {
            // TODO: Error handling? Rename method to something more fitting.
            var youtube = new YoutubeClient();

            var video = await youtube.Videos.GetAsync(songUrl);

            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

            Song song = new Song
            {
                Title = video.Title,
                AudioUrl = streamInfo.Url,
                OriginalUrl = songUrl,
            };
            
            return song;
        }
        
        private async Task PlaySongAsync()
        {
            while (_songQueue.Count > 0 && _playing)
            {
                _currentSong = _songQueue.Dequeue();

                _mediaReader = new MediaFoundationReader(_currentSong.AudioUrl);
                _outputStream = _audioClient.CreatePCMStream(AudioApplication.Music);

                try
                {
                    var embed = new EmbedBuilder
                    {
                        Description = $"Now playing: [{_currentSong.Title}]({_currentSong.OriginalUrl})",
                        Color = Color.Green
                    };
                    await _responseChannel.SendMessageAsync(embed: embed.Build());

                    await _audioClient.SetSpeakingAsync(true);
                    await _mediaReader.CopyToAsync(_outputStream);
                    await _outputStream.FlushAsync();
                    await _audioClient.SetSpeakingAsync(false);

                    await _mediaReader.DisposeAsync();
                    await _outputStream.DisposeAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"AudioService.PlaySongAsync: {e.Message}");
                }
            }
        }
    }

    public class Song
    {
        public string Title { get; set; }
        public string AudioUrl { get; set; }
        public string OriginalUrl { get; set; }
    }
}
