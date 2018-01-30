using System;
using Newtonsoft.Json;
using SpotifyPlaylistComparer.Controllers;
using SpotifyPlaylistComparer.Models;

namespace PlaylistComparer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Started");

            var spotifyApiController = new SpotifyApiController();

            var authenticateResult = spotifyApiController.SetClientCredentialsAuthToken().GetAwaiter().GetResult();

            if (authenticateResult)
            {
                Console.WriteLine("Succesfully received token");

                Console.WriteLine("What is the user-id of the user who created the first playlist:");
                var userIdOne = Console.ReadLine();
                Console.WriteLine("What is the playlist-id of the first playlist:");
                var playlistIdOne = Console.ReadLine();
                Console.WriteLine("What is the user-id of the user who created the second playlist:");
                var userIdTwo = Console.ReadLine();
                Console.WriteLine("What is the playlist-id of the second playlist:");
                var playlistIdTwo = Console.ReadLine();
                Console.WriteLine("Do you want to see songs which only exist in the first playlist or in the second playlist [press 1 or 2]");
                var whichPlaylist = Console.ReadLine();

                var jsonPlaylistOne = spotifyApiController.GetPublicPlaylist(userIdOne, playlistIdOne, 0, 100).GetAwaiter().GetResult();
                var playlistOne = JsonConvert.DeserializeObject<Playlist>(jsonPlaylistOne);
                if (playlistOne.total > 100)
                {
                    for (int i = 1; i < Math.Ceiling(playlistOne.total / (decimal)100); i++)
                    {
                        jsonPlaylistOne = spotifyApiController.GetPublicPlaylist(userIdOne, playlistIdOne, 100 * i, 100).GetAwaiter().GetResult();
                        playlistOne.items.AddRange(JsonConvert.DeserializeObject<Playlist>(jsonPlaylistOne).items);
                    }
                }

                var jsonPlaylistTwo = spotifyApiController.GetPublicPlaylist(userIdTwo, playlistIdTwo, 0, 100).GetAwaiter().GetResult();
                var playlistTwo = JsonConvert.DeserializeObject<Playlist>(jsonPlaylistTwo);
                if (playlistTwo.total > 100)
                {
                    for (int i = 1; i < Math.Ceiling(playlistTwo.total / (decimal)100); i++)
                    {
                        jsonPlaylistTwo = spotifyApiController.GetPublicPlaylist(userIdTwo, playlistIdTwo, 100 * i, 100).GetAwaiter().GetResult();
                        playlistTwo.items.AddRange(JsonConvert.DeserializeObject<Playlist>(jsonPlaylistTwo).items);
                    }
                }

                Console.WriteLine("Playlists comparen ...");

                if (whichPlaylist == "1")
                {
                    foreach (var itemOne in playlistOne.items)
                    {
                        if (!playlistTwo.items.Contains(itemOne))
                            Console.WriteLine($"Exists only in playlist one: {itemOne.track.name}");
                    }
                }
                else if (whichPlaylist == "2")
                {
                    foreach (var itemTwo in playlistTwo.items)
                    {
                        if (!playlistOne.items.Contains(itemTwo))
                            Console.WriteLine($"Exists only in playlist two: {itemTwo.track.name}");
                    }
                }
                else
                {
                    Console.WriteLine("NAN");
                }


                Console.WriteLine("Job done");
            }
            else
            {
                Console.WriteLine("Unable to receive token, check client / secret keys");
            }

            Console.ReadKey();
        }
    }
}
