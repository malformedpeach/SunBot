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
            string playlistSearch = "";
            
            if (RegexHelper.IsYoutubeUrl(userInput))
            {
                playlistSearch = userInput;
            }
            else
            {
                var searchResult = await _youtube.Search
                    .GetPlaylistsAsync(userInput)
                    .FirstOrDefaultAsync();

                playlistSearch = searchResult.Id;
            }

            await foreach (var video in _youtube.Playlists.GetVideosAsync(playlistSearch))
            {
                var song = await GetSongAsync(video.Id);
                queue.Enqueue(song);
            }

            return queue;

            //var searchResult = await _youtube.Search
            //    .GetPlaylistsAsync(userInput)
            //    .FirstOrDefaultAsync();

            //var videosSubset = await _youtube.Playlists
            //    .GetVideosAsync(searchResult.Id)
            //    .CollectAsync(20);

            //foreach (var video in videosSubset)
            //{
            //    var song = await GetSongAsync(video.Id);
            //    newQueue.Enqueue(song);
            //}

            //await foreach (var batch in _youtube.Playlists.GetVideoBatchesAsync(searchResult.Id)) 
            //{
            //    foreach (var video in batch.Items)
            //    {
            //        var song = await GetSongAsync(video.Id);
            //        newQueue.Enqueue(song);
            //    }
            //}
        }

        // process queue helper, call method when song gets dequeued for playback
        public static async Task<IStreamInfo> GetAudioStreamWithHighestBitrate(string url)
        {
            var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(url);
            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
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

        //private static async Task<Playlist> GetPlaylistAsync(string userInput)
        //{
        //    Playlist playlist = null;

        //    if (RegexHelper.IsYoutubeUrl(userInput))
        //    {
        //        playlist = await _youtube.Playlists.GetAsync(userInput);
        //    }
        //    else // search term
        //    {
        //        var searchResult = await _youtube.Search
        //            .GetPlaylistsAsync(userInput)
        //            .FirstOrDefaultAsync();

        //        if (searchResult != null)
        //            playlist = await _youtube.Playlists.GetAsync(searchResult.Id);
        //    }

        //    return playlist;
        //}
    }
}
