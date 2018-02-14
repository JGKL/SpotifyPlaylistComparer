using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using SpotifyPlaylistComparer.Controllers;
using SpotifyPlaylistComparer.Extensions;
using SpotifyPlaylistComparer.Models;

namespace PlaylistComparer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Started\r\n");

            var spotifyApiController = new SpotifyApiController();

            var task = spotifyApiController.SetClientCredentialsAuthToken();

            Console.Write("Getting authentication token");

            Console.WriteLine();

            var authenticateResult = task.Result;

            if (authenticateResult)
            {
                Console.WriteLine("Succesfully received token\r\n");

                var playlistOneMessage = "Enter the first playlist it's Spotity URI:";
                Console.WriteLine(playlistOneMessage);
                var playlistOneSpotifyUri = Console.ReadLine();

                var playlistTwoMessage = "Enter the seconds playlist it's Spotify URI:";
                Console.WriteLine(playlistTwoMessage);
                var playlistTwoSpotifyUri = Console.ReadLine();

                var playlistOneData = playlistOneSpotifyUri.SpotifyUriToDataConverter();
                var playlistTwoData = playlistTwoSpotifyUri.SpotifyUriToDataConverter();

                Console.WriteLine("Do you want to see songs which only exist in the first playlist or in the second playlist [press 1 or 2]");
                var whichPlaylist = Console.ReadLine();

                Console.Write("Comparing playlists ");

                var uniqueTracksInPlaylist = new List<Track>();

                var workerThread = new Thread(new ThreadStart(() => {
                    var playlistOne = spotifyApiController.GetPublicPlaylist(playlistOneData[4], playlistOneData[8]);
                    var playlistTwo = spotifyApiController.GetPublicPlaylist(playlistTwoData[4], playlistTwoData[8]);

                    if (whichPlaylist == "1")
                    {
                        foreach (var track in playlistOne.items)
                        {
                            var result = playlistTwo.items.FirstOrDefault(x => x.track.name == track.track.name);

                            if (result == null)
                                uniqueTracksInPlaylist.Add(track.track);
                        }
                    }
                    else if (whichPlaylist == "2")
                    {
                        foreach (var track in playlistTwo.items)
                        {
                            var result = playlistOne.items.FirstOrDefault(x => x.track.name == track.track.name);

                            if (result == null)
                                uniqueTracksInPlaylist.Add(track.track);
                        }
                    }
                }));

                workerThread.Start();

                var start = DateTime.Now;
                while (workerThread.ThreadState != ThreadState.Stopped)
                {
                    var end = DateTime.Now;
                    var dif = (end - start).TotalMilliseconds;

                    if (dif > 1000)
                    {
                        start = end;
                        Console.Write(".");
                    }
                };

                Console.WriteLine();

                if (uniqueTracksInPlaylist.Count > 0)
                {
                    var index = 0;
                    foreach (var track in uniqueTracksInPlaylist)
                    {
                        index++;
                        Console.WriteLine($"{index}. {track.name}");
                    }
                }
                else
                {
                    Console.WriteLine("Geen verschillen gevonden");
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
