using Discord;
using Discord.Audio;
using Discord.WebSocket;
using NAudio.Wave;
using SunBot.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace SunBot.Services
{
    public class AudioService
    {
        private IConfiguration _config;
        private IAudioClient _audioClient;
        private CancellationTokenSource _tokenSource;
        private Song _currentSong;
        private Queue<Song> _songQueue = new Queue<Song>();
        private bool _playing = false;

        public AudioService(IConfiguration config)
        {
            _config = config;
        }

        public async Task JoinVoiceChannelAsync(IVoiceChannel voiceChannel) 
        {
            if (voiceChannel == null)
            {
                var embed = new EmbedBuilder
                {
                    Description = "Could not join channel.",
                    Color = Color.Red
                };
                await _config.DefaultTextChannel.SendMessageAsync(embed: embed.Build());
            }
            else
            {
                _audioClient = await voiceChannel.ConnectAsync();
            }
        }

        public async Task LeaveVoiceChannelAsync()
        {
            _playing = false;
            _tokenSource.Cancel();
            await _audioClient.StopAsync();
        }
        
        public async Task PlaySongAsync(IVoiceChannel voiceChannel, string songUrl = "")
        {
            if (_audioClient == null ||
                _audioClient?.ConnectionState == ConnectionState.Disconnected)
            {
                await JoinVoiceChannelAsync(voiceChannel);
            }


            if (_playing)
            {
                Song song = await GetHighestBitrateUrlAsync(songUrl);
                _songQueue.Enqueue(song);

                var embed = new EmbedBuilder();
                embed.Description = $"Queued: [{song.Title}]({song.OriginalUrl})";
                embed.Color = Color.Gold;
                await _config.DefaultTextChannel.SendMessageAsync(embed: embed.Build());
            }
            else if (!_playing && !string.IsNullOrEmpty(songUrl))
            {
                _playing = true;
                Song song = await GetHighestBitrateUrlAsync(songUrl);
                _songQueue.Enqueue(song);

                await ProcessQueueAsync();
            }
            else if (!_playing && string.IsNullOrEmpty(songUrl))
            {
                _playing = true;
                await ProcessQueueAsync();
            }
        }
        
        public async Task StopSongAsync()
        {
            var embed = new EmbedBuilder
            {
                Description = "Stopping playback",
                Color = Color.Gold
            };

            await _config.DefaultTextChannel.SendMessageAsync(embed: embed.Build());
            _playing = false;
            _tokenSource.Cancel();
        }

        public async Task SkipSongAsync()
        {
            var embed = new EmbedBuilder
            {
                Description = $"Skipping: [{_currentSong.Title}]({_currentSong.OriginalUrl})",
                Color = Color.Red
            };
            await _config.DefaultTextChannel.SendMessageAsync(embed: embed.Build());
            _tokenSource.Cancel();
        }

        public async Task GetCurrentQueue()
        {
            var embed = new EmbedBuilder();

            if (_songQueue.Count == 0)
            {
                embed.Description = "The queue is empty.";
                embed.Color = Color.Red;
            }
            else
            {
                var builder = new StringBuilder();
                var currentQueue = _songQueue.ToArray();

                if(_playing)
                {
                    builder.AppendLine($"Currently playing: [{_currentSong.Title}]({_currentSong.OriginalUrl})");
                }

                for (int i = 0; i < currentQueue.Length; i++)
                {
                    builder.AppendLine($"{i + 1}. [{currentQueue[i].Title}]({currentQueue[i].OriginalUrl})");
                }

                embed.Description = builder.ToString();
                embed.Color = Color.Gold;
            }

            await _config.DefaultTextChannel.SendMessageAsync(embed: embed.Build());
        }

        public async Task ClearQueueAsync()
        {
            var embed = new EmbedBuilder();
            
            if(_songQueue.Count > 0)
            {
                _songQueue.Clear();
                embed.Description = "Queue cleared!";
                embed.Color = Color.Gold;
            }
            else
            {
                embed.Description = "Queue already empty!";
                embed.Color = Color.Red;
            }

            await _config.DefaultTextChannel.SendMessageAsync(embed: embed.Build());
        }

        private async Task<Song> GetHighestBitrateUrlAsync(string songUrl)
        {
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
        
        private async Task ProcessQueueAsync()
        {
            while(_songQueue.Count > 0 && _playing)
            {
                _tokenSource = new CancellationTokenSource();
                var cancellationToken = _tokenSource.Token;

                _currentSong = _songQueue.Dequeue();

                using var mediaReader = new MediaFoundationReader(_currentSong.AudioUrl);
                using var outputStream = _audioClient.CreatePCMStream(AudioApplication.Music);
                try
                {
                    var embed = new EmbedBuilder
                    {
                        Description = $"Now playing: [{_currentSong.Title}]({_currentSong.OriginalUrl})",
                        Color = Color.Gold
                    };
                    await _config.DefaultTextChannel.SendMessageAsync(embed: embed.Build());

                    await mediaReader.CopyToAsync(outputStream, cancellationToken);
                    await outputStream.FlushAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"AudioService.ProcessQueue: {ex.Message}");
                }
                finally
                {
                    _tokenSource.Dispose();
                }
            }

            _playing = false;
            if (_songQueue.Count == 0)
            {
                var embed = new EmbedBuilder
                {
                    Description = "Queue is empty, stopping playback",
                    Color = Color.Red,
                };
                await _config.DefaultTextChannel.SendMessageAsync(embed: embed.Build());
            }
        }
    }
}
