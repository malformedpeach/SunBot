using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using SunBot.Util;

namespace SunBotTests.Util
{
    [TestClass]
    public class RegexHelperTests
    {
        [TestMethod]
        public void IsYoutubeUrlTest_ShouldReturnFalse()
        {
            // Arrange 
            string userInput = "https://soundcloud.com/";

            // Act
            var result = RegexHelper.IsYoutubeUrl(userInput);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsYoutubeUrlTest_ShouldReturnTrue()
        {
            // Arrange
            string userInput = "https://www.youtube.com/watch?v=C0DPdy98e4c";

            // Act
            var result = RegexHelper.IsYoutubeUrl(userInput);

            // Assert
            Assert.IsTrue(result);
        }
    }
}
