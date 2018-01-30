using System.Collections.Generic;

namespace SpotifyPlaylistComparer.Models
{
    public class Playlist
    {
        public string href { get; set; }
        public List<Item> items { get; set; }
        public int total { get; set; }
    }
}
