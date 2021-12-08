using System;
using System.Collections.Generic;
using System.Text;
using YoutubeExplode.Videos;

namespace SunBot.Models
{
    public class Song
    {
        public VideoId Id { get; set; }
        public string Title { get; set; }
        public string AudioUrl { get; set; }
        public string OriginalUrl { get; set; }
    }
}
