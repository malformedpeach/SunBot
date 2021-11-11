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
        private IAudioClient _audioClient;
        private MediaFoundationReader _mediaReader;
        private AudioOutStream _outputStream;
        private Queue<string> _songQueue = new Queue<string>();
        private bool _playing = false;

        public async Task<bool> JoinVoiceChannelAsync(IVoiceChannel channel) 
        {
            if (channel == null) return false;
            else
            {
                _audioClient = await channel.ConnectAsync();
                return true;
            }
        }

        public async Task LeaveVoiceChannelAsync(IVoiceChannel channel)
        {
            await channel.DisconnectAsync();
        }

        public async Task EnqueueSongAsync(string songUrl = "")
        {
            if (_audioClient == null) return; // Not connected to channel

            if(string.IsNullOrEmpty(songUrl) && _songQueue.Count > 0)
            {
                // Wonky af? rework this
                _playing = true;
                await PlaySongAsync();
            }
            else if(_playing)
            {
                var url = await GetHighestBitrateUrlAsync(songUrl);
                _songQueue.Enqueue(url);
            }
            else if(!string.IsNullOrEmpty(songUrl) && _songQueue.Count == 0)
            {
                // Wonky af? rework this
                _playing = true;
                var url = await GetHighestBitrateUrlAsync(songUrl);
                _songQueue.Enqueue(url);
                await PlaySongAsync();
            }
        }

        public void StopSongAsync()
        {
            // TODO: Error handling, already disposed?

            _playing = false;
            _outputStream.Dispose();
            _mediaReader.Dispose();
        }


        private async Task<string> GetHighestBitrateUrlAsync(string songUrl)
        {
            // TODO: Error handling?
            var youtube = new YoutubeClient();

            var video = await youtube.Videos.GetAsync(songUrl);

            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
            return streamInfo.Url;
        }

        private async Task PlaySongAsync()
        {
            #region Loop this sucka?
            while(_songQueue.Count > 0)
            {
                _mediaReader = new MediaFoundationReader(_songQueue.Dequeue());
                _outputStream = _audioClient.CreatePCMStream(AudioApplication.Music);
                
                try
                {
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
                    continue; // break loop
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
}
