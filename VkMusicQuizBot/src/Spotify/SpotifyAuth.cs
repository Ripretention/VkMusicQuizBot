﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace VkMusicQuizBot
{
    public class SpotifyAuth
    {
        public string AccessToken { get; private set; }
        private string clientId;
        private string clientSecret;
        private string refreshToken;
        public SpotifyAuth(SpotifyAuthConfiguration cfg)
        {
            clientId = cfg.ClientId;
            AccessToken = cfg.AccessToken;
            clientSecret = cfg.ClientSecret;
            refreshToken = cfg.RefreshToken;
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

            var response = await api.Call<SpotifyRefreshTokenResponse>(request);
            AccessToken = response.AccessToken;

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
                result = false;
            }

            return result;
        }

        public delegate void AuthHasRefreshedHandler(SpotifyAuth auth);
        public event AuthHasRefreshedHandler OnRefresh;

        public override string ToString() => AccessToken;
    }
}
