﻿using System;
using System.Web;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace VkMusicQuizBot
{
    public class SpotifyAPI : ISpotifyAPI
    {
        private HttpClient client;
        public uint Version { get; set; } = 1;
        public SpotifyAPI(SpotifyConfiguration cfg, IDictionary<string, string> headers = null)
        {
            if (cfg == null)
                throw new ArgumentNullException(nameof(cfg));
            client = new HttpClient();
            foreach (var param in (headers ?? new Dictionary<string, string>()).Append(new KeyValuePair<string, string>("Authorization", $"Bearer {cfg.AccessToken}")))
                client.DefaultRequestHeaders.Add(param.Key, param.Value);
        }

        public async Task<T> Delete<T>(string method, IDictionary<string, string> urlParams = null) where T : class
        {
            var response = await Delete(method, urlParams);
            return await JsonSerializer.DeserializeAsync<T>(await response.ReadAsStreamAsync());
        }
        public Task<HttpContent> Delete(string method, IDictionary<string, string> urlParams = null)
        {
            method += urlParams != null && urlParams.Any()
                 ? encodeQueryData(urlParams)
                 : String.Empty;

            return Call(method, HttpMethod.Delete, null);
        }

        public async Task<T> Put<T, R>(string method, R data)
            where T : class
            where R : class
        {
            var response = await Put(method, new StringContent(JsonSerializer.Serialize(data)));
            return await JsonSerializer.DeserializeAsync<T>(await response.ReadAsStreamAsync());
        }
        public Task<HttpContent> Put<T>(string method, T data) where T : class =>
            Put(method, new StringContent(JsonSerializer.Serialize(data)));
        public Task<HttpContent> Put(string method, HttpContent data) =>
            Call(method, HttpMethod.Put, data);

        public async Task<T> Post<T, R>(string method, R data) 
            where T : class
            where R : class
        {
            var response = await Post(method, new StringContent(JsonSerializer.Serialize(data)));
            return await JsonSerializer.DeserializeAsync<T>(await response.ReadAsStreamAsync());
        }
        public Task<HttpContent> Post<T>(string method, T data) where T : class =>
            Post(method, new StringContent(JsonSerializer.Serialize(data)));
        public Task<HttpContent> Post(string method, HttpContent data) =>
            Call(method, HttpMethod.Post, data);

        public async Task<T> Get<T>(string method, IDictionary<string, string> urlParams = null) where T : class
        {
            var response = await Get(method, urlParams);
            return await JsonSerializer.DeserializeAsync<T>(await response.ReadAsStreamAsync());
        }
        public Task<HttpContent> Get(string method, IDictionary<string, string> urlParams = null)
        {
            method += urlParams != null && urlParams.Any()
                 ? encodeQueryData(urlParams)
                 : String.Empty;

            return Call(method, HttpMethod.Get, null);
        }

        private string encodeQueryData(IDictionary<string, string> urlParams = null) => 
            $"?{String.Join("&", urlParams.Select(p => HttpUtility.UrlEncode($"{p.Key}={p.Value}")).ToArray())}";

        public async Task<T> Call<T>(string method, HttpMethod reqMethod, HttpContent data = null) where T : class
        {
            var response = await Call(method, reqMethod, data);
            return await JsonSerializer.DeserializeAsync<T>(await response.ReadAsStreamAsync());
        }
        public async Task<HttpContent> Call(string method, HttpMethod reqMethod, HttpContent data = null)
        {
            var response = await client.SendAsync(new HttpRequestMessage
            {
                Method = reqMethod,
                Content = data,
                RequestUri = new Uri(constructMethorUrl(method))
            });

            if (!response.IsSuccessStatusCode)
                await handleErrorSpotifyResponse(response);

            return response.Content;
        }
        private async Task handleErrorSpotifyResponse(HttpResponseMessage response)
        {
            var status = response.StatusCode.ToString();
            string message = String.Empty;
            SpotifyExceptionResponseBody responseBody = null;

            try
            {
                responseBody = (await JsonSerializer.DeserializeAsync<SpotifyExceptionResponse>(await response.Content.ReadAsStreamAsync()))?.Body;
                message = responseBody?.ToString() ?? String.Empty;
            }
            catch (Exception)
            {
                message = "JSON-parse has failed";
            }

            if (status == "Unauthorized")
                throw new SpotifyAuthorizationException(message);
            else if (responseBody != null)
                throw new SpotifyRequestException(responseBody);

            throw new SpotifyRequestException(message);
        }

        private string constructMethorUrl(string method) => $@"https://api.spotify.com/v{Version}/{method}";
    }
    public interface ISpotifyAPI
    {
        public uint Version { get; set; }
        public Task<T> Delete<T>(string method, IDictionary<string, string> urlParams = null) where T : class;
        public Task<HttpContent> Delete(string method, IDictionary<string, string> urlParams = null);
        public Task<T> Put<T, R>(string method, R data)
            where T : class
            where R : class;
        public Task<HttpContent> Put<T>(string method, T data) where T : class;
        public Task<HttpContent> Put(string method, HttpContent data);
        public Task<T> Post<T, R>(string method, R data)
            where T : class
            where R : class;
        public Task<HttpContent> Post<T>(string method, T data) where T : class;
        public Task<HttpContent> Post(string method, HttpContent data);
        public Task<T> Get<T>(string method, IDictionary<string, string> urlParams = null) where T : class;
        public Task<HttpContent> Get(string method, IDictionary<string, string> urlParams = null);
        public Task<T> Call<T>(string method, HttpMethod reqMethod, HttpContent data = null) where T : class;
        public Task<HttpContent> Call(string method, HttpMethod reqMethod, HttpContent data = null);
    }
}