using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
            var spotifyClient = "a399fad8e81842ff8e7c5988e725ac77";
            var spotifySecret = "de677cae72d74219a634932b43bbe9b3";

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

        public async Task<string> GetPublicPlaylist(string userId, string playlistId, int offset, int limit)
        {
            var endpoint = $"/v1/users/{userId}/playlists/{playlistId}/tracks?offset={offset}&limit={limit}";

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken.access_token);
            var response = await _httpClient.GetAsync($"{_baseUrl}{endpoint}");

            var result = await response.Content.ReadAsStringAsync();
            return result;
        }
    }
}
