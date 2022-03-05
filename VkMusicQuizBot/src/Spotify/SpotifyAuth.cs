using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Options; 
using Microsoft.Extensions.Logging;

namespace VkMusicQuizBot
{
    public class SpotifyAuth
    {
        private string clientId { get => cfg.CurrentValue.ClientId; }
        private string clientSecret { get => cfg.CurrentValue.ClientSecret; }
        private string refreshToken { get => cfg.CurrentValue.RefreshToken; }
        public string AccessToken { get => cfg.CurrentValue.AccessToken; }

        private ILogger<SpotifyAuth> logger;
        private IOptionsMonitor<SpotifyAuthConfiguration> cfg;
        public SpotifyAuth(IOptionsMonitor<SpotifyAuthConfiguration> cfg, ILogger<SpotifyAuth> logger = null)
        {
            this.cfg = cfg;
            this.logger = logger;
        }

        public async Task Refresh(ISpotifyAPI api)
        {
            var data = new[]
            {
                new KeyValuePair<string, string>("refresh_token", refreshToken),
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret)
            };
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri("https://accounts.spotify.com/api/token"),
                Method = HttpMethod.Post,
                Content = new FormUrlEncodedContent(data)
            };
            request.Headers.Authorization = null;

            logger?.LogInformation("SpotifyAuth has been refreshing");

            var response = await api.Call<SpotifyRefreshTokenResponse>(request);
            cfg.CurrentValue.AccessToken = response.AccessToken;

            OnRefresh?.Invoke(this);
        }
        public async Task<bool> TryRefresh(ISpotifyAPI api)
        {
            bool result = true;
            try
            {
                await Refresh(api);
            }
            catch (Exception)
            {
                logger?.LogWarning("SpotifyAuth refresh has failed");
                result = false;
            }

            return result;
        }

        public delegate void AuthHasRefreshedHandler(SpotifyAuth auth);
        public event AuthHasRefreshedHandler OnRefresh;

        public override string ToString() => AccessToken;
    }
}
