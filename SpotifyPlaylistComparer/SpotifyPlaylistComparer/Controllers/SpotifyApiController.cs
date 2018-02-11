using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SpotifyPlaylistComparer.Models;

namespace SpotifyPlaylistComparer.Controllers
{
    public class AuthToken
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string expires_in { get; set; }
        public string scope { get; set; }
    }

    public class SpotifyApiController
    {
        private readonly string _baseUrl = "https://api.spotify.com";

        private HttpClient _httpClient;
        private AuthToken _authToken;

        public SpotifyApiController()
        {
            _httpClient = new HttpClient();
        }

        //see https://developer.spotify.com/web-api/authorization-guide/#client_credentials_flow
        public async Task<bool> SetClientCredentialsAuthToken()
        {
            var spotifyClient = "";
            var spotifySecret = "";

            var parameters = new Dictionary<string, string> { { "grant_type", "client_credentials" } };
            var encodedContent = new FormUrlEncodedContent(parameters);

            var authHeader = Convert.ToBase64String(Encoding.Default.GetBytes($"{spotifyClient}:{spotifySecret}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

            var response = await _httpClient.PostAsync("https://accounts.spotify.com/api/token", encodedContent);

            if (response.IsSuccessStatusCode)
            {
                var authToken = await response.Content.ReadAsStringAsync();
                _authToken = JsonConvert.DeserializeObject<AuthToken>(authToken);

                return true;
            }

            return false;
        }

        private async Task<Playlist> GetPublicPlaylistChunk(string userId, string playlistId, int offset, int limit)
        {
            var endpoint = $"/v1/users/{userId}/playlists/{playlistId}/tracks?offset={offset}&limit={limit}";

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken.access_token);
            var response = await _httpClient.GetAsync($"{_baseUrl}{endpoint}");

            var responseString = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<Playlist>(responseString);

            return result;
        }

        public Playlist GetPublicPlaylist(string userId, string playlistId)
        {
            var playlistTasks = new List<Task<Playlist>>();

            var playlist = GetPublicPlaylistChunk(userId, playlistId, 0, 100).Result;
            if (playlist.total > 100)
            {
                for (int i = 1; i < Math.Ceiling(playlist.total / (decimal)100); i++)
                {
                    playlistTasks.Add(GetPublicPlaylistChunk(userId, playlistId, 100 * i, 100));
                }
            }

            var restOfPlaylist = Task.WhenAll(playlistTasks).Result;
            foreach (var playlistChunk in restOfPlaylist)
            {
                playlist.items.AddRange(playlistChunk.items);
            }

            return playlist;
        }
    }
}
