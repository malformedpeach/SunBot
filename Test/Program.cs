using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NAudio;
using NAudio.Wave;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace YoutubeExplodeTest
{
    class Program
    {

        public static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            var youtube = new YoutubeClient();

            var video = await youtube.Videos.GetAsync("https://www.youtube.com/watch?v=jBuKNkVFaMU");

            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
            var url = streamInfo.Url;
            using (var mf = new MediaFoundationReader(url))
            {
                using (var wo = new WaveOutEvent())
                {
                    wo.Init(mf);
                    wo.Play();

                    while(wo.PlaybackState == PlaybackState.Playing)
                    {
                        Thread.Sleep(1000);
                    }
                }
            }

            // ------------------------------------------------
            // Get video stream
            //var stream = await youtube.Videos.Streams.GetAsync(streamInfo);

            // Download video
            //await youtube.Videos.Streams.DownloadAsync(streamInfo, $"video.{streamInfo.Container}");
            //stream.BeginRead()


            // Play audio with waveout
            //var wave = new WaveOut();

            //var audioFile = new AudioFileReader("video.webm");
            //wave.Init(audioFile);
            //wave.Play();





            await Task.Delay(-1);
        }
    }
}
