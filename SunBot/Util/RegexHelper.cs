using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SunBot.Util
{
    public static class RegexHelper
    {
        private const string YOUTUBE_URL_PATTERN = @"^((?:https?:)?\/\/)?((?:www|m)\.)?((?:youtube\.com|youtu.be))(\/(?:[\w\-]+\?v=|embed\/|v\/)?)([\w\-]+)(\S+)?$";

        /// <summary>
        /// This method checks if a string is a youtube URL.
        /// </summary>
        /// <param name="theString">the string to check</param>
        /// <returns>A boolean. True if <c>theString</c> is a match, and false if not.</returns>
        public static bool IsYoutubeUrl(string theString)
        {
            var regex = new Regex(YOUTUBE_URL_PATTERN);
            
            if (regex.IsMatch(theString))
                return true;
            else 
                return false;
        }
    }
}
