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
        private IAudioClient _audioClient;
        private CancellationTokenSource _tokenSource = null;
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
            _playing = false;
            ClearQueue();
            await channel.DisconnectAsync();
            _audioClient.Dispose();
        }
        
        public async Task<Embed> PlaySongAsync(IVoiceChannel voiceChannel, ISocketMessageChannel textChannel, string songUrl = "")
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
                embed.Color = Color.Gold;

                return embed.Build();
            }
            else if (!string.IsNullOrEmpty(songUrl) && _songQueue.Count == 0)
            {
                _playing = true; // Wonky af? rework this
                Song song = await GetHighestBitrateUrlAsync(songUrl);
                _songQueue.Enqueue(song);

                await ProcessQueueAsync();
            }
            else if (string.IsNullOrEmpty(songUrl) && _songQueue.Count > 0)
            {
                _playing = true; // Wonky af? rework this

                await ProcessQueueAsync();
            }


            // Check if zero. If user stops playback we don't want to show the 'no more songs' message.
            if (_songQueue.Count == 0 && _playing)
            {
                _playing = false;
                embed.Description = "No more songs in queue.";
                embed.Color = Color.Red;
            }

            return embed.Build();
        }
        
        public void StopSongAsync()
        {
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
            await _responseChannel.SendMessageAsync(embed: embed.Build());

            _tokenSource.Cancel();
        }

        public Embed GetCurrentQueue()
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

            return embed.Build();
        }

        public Embed ClearQueue()
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

            return embed.Build();
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
                    await _responseChannel.SendMessageAsync(embed: embed.Build());

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
        }
    }
}
