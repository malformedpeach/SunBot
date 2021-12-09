using Discord;
using Discord.Audio;
using NAudio.Wave;
using SunBot.Models;
using SunBot.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SunBot.Services
{
    public class AudioService
    {
        private const int EMBED_MAX_CAPACITY = 5900;
        private Configuration _config;
        private IAudioClient _audioClient;
        private CancellationTokenSource _tokenSource;
        private Song _currentSong;
        private Queue<Song> _songQueue = new Queue<Song>();
        private bool _playing = false;

        public AudioService(Configuration config)
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
            if (_audioClient?.ConnectionState == ConnectionState.Disconnecting ||
                _audioClient?.ConnectionState == ConnectionState.Disconnected) 
                return;

            _playing = false;
            await _audioClient.StopAsync();
        }
        
        public async Task PlaySongAsync(IVoiceChannel voiceChannel, string userInput = "")
        {
            if (_audioClient == null ||
                _audioClient?.ConnectionState == ConnectionState.Disconnected)
            {
                await JoinVoiceChannelAsync(voiceChannel);

                if (_audioClient == null || 
                    _audioClient?.ConnectionState == ConnectionState.Disconnected ||
                    _audioClient?.ConnectionState == ConnectionState.Disconnecting) return;
            }

            if (!string.IsNullOrEmpty(userInput))
            {
                Song song = await YoutubeExplodeHelper.GetSongAsync(userInput);
                var embed = new EmbedBuilder();

                if (song != null)
                {
                    _songQueue.Enqueue(song);
                    embed.Description = $"Queued: [{song.Title}]({song.Url})";
                    embed.Color = Color.Gold;
                }
                else
                {
                    embed.Description = $"Something went wrong :(";
                    embed.Color = Color.Red;
                }
                
                await _config.DefaultTextChannel.SendMessageAsync(embed: embed.Build());
            }

            if(!_playing)
            {
                _playing = true;
                await ProcessQueueAsync();
            }
        }

        public async Task PlaySongQueueAsync(IVoiceChannel voiceChannel, string userInput)
        {
            if (_audioClient == null ||
                _audioClient?.ConnectionState == ConnectionState.Disconnected)
            {
                await JoinVoiceChannelAsync(voiceChannel);

                if (_audioClient == null ||
                    _audioClient?.ConnectionState == ConnectionState.Disconnected ||
                    _audioClient?.ConnectionState == ConnectionState.Disconnecting) return;
            }

            try
            {
                var embed = new EmbedBuilder
                {
                    Description = "This might take a while depending on playlist size.",
                    Color = Color.Gold
                };
                _config.DefaultTextChannel.SendMessageAsync(embed: embed.Build());

                var newQueue = await YoutubeExplodeHelper.GetSongQueueAsync(userInput);
                _songQueue = newQueue;
                _playing = true;
                await ProcessQueueAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine($"AudioService.PlaySongQueueAsync(): {e.Message}");

                var embed = new EmbedBuilder
                {
                    Description = "Something went wrong :(",
                    Color = Color.Red
                };
                _config.DefaultTextChannel.SendMessageAsync(embed: embed.Build());
            }
        }

        public async Task StopSongAsync()
        {
            if (!_playing) return;

            _playing = false;
            _tokenSource.Cancel();

            var embed = new EmbedBuilder
            {
                Description = "Stopping playback",
                Color = Color.Gold
            };

            await _config.DefaultTextChannel.SendMessageAsync(embed: embed.Build());
        }

        public async Task SkipSongAsync()
        {
            if (!_playing) return; // Skip songs when playback is stopped?

            if (!_tokenSource.IsCancellationRequested)
            {
                var embed = new EmbedBuilder
                {
                    Description = $"Skipping: [{_currentSong.Title}]({_currentSong.Url})",
                    Color = Color.Red
                };
                await _config.DefaultTextChannel.SendMessageAsync(embed: embed.Build());
                
                _tokenSource.Cancel();
            }
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
                var builder = new StringBuilder(1024);
                var currentQueue = _songQueue.ToArray();
                
                if(_playing)
                {
                    embed.Description = $"Currently playing: [{_currentSong.Title}]({_currentSong.Url})";
                }

                for (int i = 0; i < currentQueue.Length; i++)
                {
                    string songLine = $"{i + 1}. [{currentQueue[i].Title}]({currentQueue[i].Url})";

                    if (songLine.Length + builder.Length + embed.Length > EMBED_MAX_CAPACITY)
                        break;

                    if (songLine.Length + builder.Length > builder.Capacity)
                    {
                        embed.AddField("______", builder.ToString());
                        
                        builder.Clear();
                        builder.AppendLine(songLine);
                    }
                    else
                    {
                        builder.AppendLine(songLine);
                    }
                }
                
                if (embed.Length + builder.Length < EMBED_MAX_CAPACITY)
                {
                    embed.AddField("______", builder.ToString());
                    embed.Color = Color.Gold;
                }
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

        private async Task ProcessQueueAsync()
        {
            while(_songQueue.Count > 0 && _playing)
            {
                _tokenSource = new CancellationTokenSource();
                var cancellationToken = _tokenSource.Token;

                _currentSong = _songQueue.Dequeue();
                var streamInfo = await YoutubeExplodeHelper
                    .GetAudioStreamWithHighestBitrate(_currentSong.Url);
                
                await using var mediaReader = new MediaFoundationReader(streamInfo.Url);
                await using var outputStream = _audioClient.CreatePCMStream(AudioApplication.Music);

                try
                {
                    var embed = new EmbedBuilder
                    {
                        Description = $"Now playing: [{_currentSong.Title}]({_currentSong.Url})",
                        Color = Color.Gold
                    };
                    await _config.DefaultTextChannel.SendMessageAsync(embed: embed.Build());

                    await mediaReader.CopyToAsync(outputStream, cancellationToken);
                    await outputStream.FlushAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"AudioService.ProcessQueueAsync: {ex.Message}");
                }
                finally
                {
                    _tokenSource.Dispose();
                }
            }

            if (_songQueue.Count == 0 && _playing)
            {
                var embed = new EmbedBuilder
                {
                    Description = "Queue is empty, stopping playback",
                    Color = Color.Red,
                };
                await _config.DefaultTextChannel.SendMessageAsync(embed: embed.Build());
                
                _playing = false;
            }
        }
    }
}
