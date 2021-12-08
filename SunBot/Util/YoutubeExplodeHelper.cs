using SunBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Common;
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

            var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(video.Id);
            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
            
            Song song = new Song
            {
                Id = video.Id,
                Title = video.Title,
                AudioUrl = streamInfo.Url,
                OriginalUrl = video.Url
            };

            return song;
        }

        public static async Task<Queue<Song>> GetPlaylistAsync(string userInput)
        {
            var searchResult = await _youtube.Search
                .GetPlaylistsAsync(userInput)
                .FirstOrDefaultAsync();

            var newQueue = new Queue<Song>();

            var videosSubset = await _youtube.Playlists
                .GetVideosAsync(searchResult.Id)
                .CollectAsync(20);

            foreach (var video in videosSubset)
            {
                var song = await GetSongAsync(video.Id);
                newQueue.Enqueue(song);
            }

            //await foreach (var batch in _youtube.Playlists.GetVideoBatchesAsync(searchResult.Id)) 
            //{
            //    foreach (var video in batch.Items)
            //    {
            //        var song = await GetSongAsync(video.Id);
            //        newQueue.Enqueue(song);
            //    }
            //}

            return newQueue;
        }

        private static async Task<Video> GetVideoAsync(string userInput)
        {
            var youtube = new YoutubeClient();
            Video video = null;

            if (RegexHelper.IsYoutubeUrl(userInput))
            {
                video = await youtube.Videos.GetAsync(userInput);
            }
            else // searchterm
            {
                var searchResult = await youtube.Search
                    .GetVideosAsync(userInput)
                    .FirstOrDefaultAsync();

                if (searchResult != null)
                    video = await youtube.Videos.GetAsync(searchResult.Id);
            }

            return video;
        }
    }
}
