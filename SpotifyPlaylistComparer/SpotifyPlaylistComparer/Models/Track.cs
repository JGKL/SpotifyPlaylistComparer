using System.Collections.Generic;

namespace SpotifyPlaylistComparer.Models
{
    public class Track
    {
        public string name { get; set; }
        public List<Artist> artists { get; set; }
    }
}
