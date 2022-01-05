using SunBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace SunBot.Util
{
    public static class YoutubeExplodeHelper
    {
        private static YoutubeClient _youtube = new YoutubeClient();

        public static async Task<Song> GetSongAsync(string userInput)
        {
            var video = await GetVideoAsync(userInput);

            if (video == null)
                return null;
            else
            {
                Song song = new Song
                {
                    Id = video.Id,
                    Title = video.Title,
                    Url = video.Url
                };

                return song;
            }
        }
        
        public static async Task<Queue<Song>> GetSongQueueAsync(string userInput)
        {
            var queue = new Queue<Song>();
            var playlist = await GetPlaylistAsync(userInput);

            if (playlist == null)
                return null;

            await foreach (var video in _youtube.Playlists.GetVideosAsync(playlist.Id))
            {
                var song = await GetSongAsync(video.Id);
                queue.Enqueue(song);
            }

            return queue;
        }

        public static async Task<IStreamInfo> GetAudioStreamWithHighestBitrate(string url)
        {
            var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(url);

            var streamInfo = streamManifest.GetAudioOnlyStreams().Where(x => x.AudioCodec == "opus").TryGetWithHighestBitrate();

            //if (streamInfo == null)
            //    streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

            return streamInfo;
        }
        

        private static async Task<Video> GetVideoAsync(string userInput)
        {
            Video video = null;

            if (RegexHelper.IsYoutubeUrl(userInput))
            {
                video = await _youtube.Videos.GetAsync(userInput);
            }
            else // search term
            {
                var searchResult = await _youtube.Search
                    .GetVideosAsync(userInput)
                    .FirstOrDefaultAsync();

                if (searchResult != null)
                    video = await _youtube.Videos.GetAsync(searchResult.Id);
            }

            return video;
        }

        private static async Task<Playlist> GetPlaylistAsync(string userInput)
        {
            Playlist playlist = null;

            if (RegexHelper.IsYoutubeUrl(userInput))
            {
                playlist = await _youtube.Playlists.GetAsync(userInput);
            }
            else
            {
                var searchResult = await _youtube.Search
                    .GetPlaylistsAsync(userInput)
                    .FirstOrDefaultAsync();

                playlist = await _youtube.Playlists.GetAsync(searchResult.Id);
            }

            return playlist;
        }
    }
}
