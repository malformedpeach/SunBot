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
        private bool _playing = false;
        private ISocketMessageChannel _responseChannel;

        public async Task<Embed> JoinVoiceChannelAsync(IVoiceChannel voiceChannel, ISocketMessageChannel textChannel) 
        {
            _responseChannel = textChannel; // I think i'm cool with this
            var embed = new EmbedBuilder();

            if (voiceChannel == null)
            {
                embed.Description = "Could not join channel.";
                embed.Color = Color.DarkRed;

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

            if(string.IsNullOrEmpty(songUrl) && _songQueue.Count > 0)
            {
                _playing = true; // Wonky af? rework this
                await PlaySongAsync();
            }
            else if(_playing)
            {
                Song song = await GetHighestBitrateUrlAsync(songUrl);
                _songQueue.Enqueue(song);

                embed.Description = $"Queued: [{song.Title}]({song.OriginalUrl})";
                embed.Color = Color.DarkGreen;

                return embed.Build();
            }
            else if(!string.IsNullOrEmpty(songUrl) && _songQueue.Count == 0)
            {
                _playing = true; // Wonky af? rework this
                Song song = await GetHighestBitrateUrlAsync(songUrl);
                _songQueue.Enqueue(song);
                
                await PlaySongAsync();
            }

            embed.Description = "No more songs in queue.";
            embed.Color = Color.DarkRed;

            return embed.Build();
        }
        
        public void StopSongAsync()
        {
            // TODO: Error handling, already disposed?

            _playing = false;
            _outputStream.Dispose();
            _mediaReader.Dispose();
        }

        public void SkipSongAsync()
        {
            // Here next my man
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
            #region Loop this sucka?
            while(_songQueue.Count > 0)
            {
                Song currentSong = _songQueue.Dequeue();

                _mediaReader = new MediaFoundationReader(currentSong.AudioUrl);
                _outputStream = _audioClient.CreatePCMStream(AudioApplication.Music);
                
                try
                {
                    var embed = new EmbedBuilder
                    {
                        Description = $"Now playing: [{currentSong.Title}]({currentSong.OriginalUrl})",
                        Color = Color.DarkGreen
                    };
                    await _responseChannel.SendMessageAsync(embed: embed.Build());

                    await _audioClient.SetSpeakingAsync(true);
                    await _mediaReader.CopyToAsync(_outputStream);
                    await _outputStream.FlushAsync();
                    await _audioClient.SetSpeakingAsync(false);

                    _mediaReader.Dispose();
                    _outputStream.Dispose();
                }
                catch (OperationCanceledException)
                {
                    // TODO: Cancelled message for user.
                    Console.WriteLine("Canceled!");
                    _playing = false;
                    continue;
                }
            }
            #endregion

            #region Recursion probs bad idea, stackoverflow inc
            //_mediaReader = new MediaFoundationReader(_songQueue.Dequeue());
            //_outputStream = _audioClient.CreatePCMStream(AudioApplication.Music);

            //try
            //{
            //    await _audioClient.SetSpeakingAsync(true);
            //    await _mediaReader.CopyToAsync(_outputStream);
            //    await _outputStream.FlushAsync();
            //    await _audioClient.SetSpeakingAsync(false);

            //    _mediaReader.Dispose();
            //    _outputStream.Dispose();
            //}
            //catch (OperationCanceledException)
            //{
            //    // TODO: Canceled message to channel
            //    Console.WriteLine("Canceled");
            //}
            //finally
            //{
            //    if (_songQueue.Count > 0) await PlaySongAsync();
            //    else if (_songQueue.Count == 0) _playing = false;
            //}
            #endregion
        }
    }


    public class Song
    {
        public string Title { get; set; }
        public string AudioUrl { get; set; }
        public string OriginalUrl { get; set; }
    }
}
