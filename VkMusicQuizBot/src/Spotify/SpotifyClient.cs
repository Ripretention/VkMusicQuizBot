using System;
using System.Web;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace VkMusicQuizBot
{
    public class SpotifyClient
    {
        public readonly ISpotifyAPIClient Api;
        public SpotifyClient(SpotifyConfiguration cfg)
        {
            Api = new SpotifyAPIClient(cfg);
        }
        public SpotifyClient(ISpotifyAPIClient api)
        {
            Api = api ?? throw new ArgumentNullException(nameof(api));
        }
    }

    public class SpotifyAPIClient : ISpotifyAPIClient
    {
        private HttpClient client;
        public uint Version { get; set; } = 1;
        public SpotifyAPIClient(SpotifyConfiguration cfg, IDictionary<string, string> headers = null)
        {
            if (cfg == null)
                throw new ArgumentNullException(nameof(cfg));
            client = new HttpClient();
            foreach (var param in (headers ?? new Dictionary<string, string>()).Append(new KeyValuePair<string, string>("Authorization", $"Bearer {cfg.AccessToken}")))
                client.DefaultRequestHeaders.Add(param.Key, param.Value);
        }

        public async Task<T> Post<T, R>(string method, R data)
        {
            var response = await Post(method, new StringContent(JsonSerializer.Serialize(data)));
            return await JsonSerializer.DeserializeAsync<T>(await response.ReadAsStreamAsync());
        }
        public Task<HttpContent> Post<T>(string method, T data) =>
            Post(method, new StringContent(JsonSerializer.Serialize(data)));

        public Task<HttpContent> Post(string method, HttpContent data) =>
            Call(method, HttpMethod.Post, data);

        public async Task<T> Get<T>(string method, IDictionary<string, string> urlParams = null)
        {
            var response = await Get(method, urlParams);
            return await JsonSerializer.DeserializeAsync<T>(await response.ReadAsStreamAsync());
        }
        public Task<HttpContent> Get(string method, IDictionary<string, string> urlParams = null)
        {
            method += urlParams != null && urlParams.Any()
                 ? $"?{String.Join("&", urlParams.Select(p => HttpUtility.UrlEncode($"{p.Key}={p.Value}")).ToArray())}"
                 : String.Empty;

            return Call(method, HttpMethod.Get, null);
        }

        public async Task<T> Call<T>(string method, HttpMethod reqMethod, HttpContent data = null)
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
    public interface ISpotifyAPIClient
    {
        public uint Version { get; set; }
        public Task<T> Get<T>(string method, IDictionary<string, string> urlParams = null);
        public Task<HttpContent> Get(string method, IDictionary<string, string> urlParams = null);
        public Task<T> Call<T>(string method, HttpMethod reqMethod, HttpContent data = null);
        public Task<HttpContent> Call(string method, HttpMethod reqMethod, HttpContent data = null);
    }
}
