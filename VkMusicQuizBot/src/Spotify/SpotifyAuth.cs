using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace VkMusicQuizBot
{
    public class SpotifyAuth
    {
        public string AccessToken { get; private set; }
        private string refreshToken;
        public SpotifyAuth(SpotifyAuthConfiguration cfg)
        {
            AccessToken = cfg.AccessToken;
            refreshToken = cfg.RefreshToken;
        }

        public async Task Refresh(ISpotifyAPI api)
        {
            var data = new[]
            {
                new KeyValuePair<string, string>("refresh_token", refreshToken),
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
            };
            var response = await api.Call<SpotifyRefreshTokenResponse>(new Uri("https://accounts.spotify.com/api/token"), HttpMethod.Get, new FormUrlEncodedContent(data));
            AccessToken = response.AccessToken;
        }

        public override string ToString() => AccessToken;
    }
}
