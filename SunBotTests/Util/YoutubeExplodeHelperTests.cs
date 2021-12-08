using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using SunBot.Util;
using SunBot.Models;
using System.Threading.Tasks;

namespace SunBotTests.Util
{
    [TestClass()]
    public class YoutubeExplodeHelperTests
    {

        [TestMethod()]
        public void GetSong_WithUrl()
        {
            // Arrange
            string userInput = "https://www.youtube.com/watch?v=C0DPdy98e4c";
            Song expected = new Song
            {
                Id = "C0DPdy98e4c",
                Title = "TEST VIDEO",
            };

            // Act
            var task = Task.Run(() => YoutubeExplodeHelper.GetSongAsync(userInput));
            task.Wait();
            
            var actual = task.Result;

            // Assert
            Assert.AreEqual(expected.Id, actual.Id);
        }

        [TestMethod()]
        public void GetSong_WithSearchTerm()
        {
            // Arrange
            string userInput = "Bohemian Rhapsody";
            Song expected = new Song
            {
                Id = "fJ9rUzIMcZQ",
                Title = "Queen – Bohemian Rhapsody (Official Video Remastered)"
            };

            // Act
            var task = Task.Run(() => YoutubeExplodeHelper.GetSongAsync(userInput));
            task.Wait();

            var actual = task.Result;

            // Assert
            Assert.AreEqual(expected.Id, actual.Id);
        }
    }
}
