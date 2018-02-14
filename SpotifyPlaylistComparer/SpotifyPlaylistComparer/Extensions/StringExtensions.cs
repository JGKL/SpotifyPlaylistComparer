using System.Text.RegularExpressions;

namespace SpotifyPlaylistComparer.Extensions
{
    public static class StringExtensions
    {
        public static string[] SpotifyUriToDataConverter(this string url)
        {
            var result = Regex.Split(url, @"(-)|(:)");
            return result;
        }
    }
}
