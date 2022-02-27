using System;
using System.Web;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace VkMusicQuizBot
{
    public class SpotifyAPI : ISpotifyAPI
    {
        private SpotifyAuth auth;
        private ILogger<SpotifyAPI> logger;
        private HttpClient client;
        public uint Version { get; set; } = 1;
        public SpotifyAPI(
            SpotifyAuth auth, 
            IDictionary<string, string> headers = null, 
            HttpMessageHandler clientHandler = null,
            ILogger<SpotifyAPI> logger = null
        )
        {
            this.logger = logger;
            this.auth = auth ?? throw new ArgumentNullException(nameof(auth));
            client = clientHandler != null ? new HttpClient(clientHandler) : new HttpClient();
        }
        public ISpotifyExceptionResponseHandler ExceptionHandler = new SpotifyExceptionResponseHandler();

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
            var response = await Put(method, new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json"));
            return await JsonSerializer.DeserializeAsync<T>(await response.ReadAsStreamAsync());
        }
        public Task<HttpContent> Put<T>(string method, T data) where T : class =>
            typeof(HttpContent).IsAssignableFrom(data.GetType())
                ? Call(method, HttpMethod.Put, data as HttpContent)
                : Put(method, new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json"));

        public async Task<T> Post<T, R>(string method, R data)
            where T : class
            where R : class
        {
            var response = await Post(method, new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json"));
            return await JsonSerializer.DeserializeAsync<T>(await response.ReadAsStreamAsync());
        }
        public Task<HttpContent> Post<T>(string method, T data) where T : class =>
            typeof(HttpContent).IsAssignableFrom(data.GetType())
                ? Call(method, HttpMethod.Post, data as HttpContent)
                : Post(method, new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json"));

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

        public Task<T> Call<T>(string method, HttpMethod reqMethod, HttpContent data = null) where T : class =>
            Call<T>(new Uri(constructMethorUrl(method)), reqMethod, data);
        public Task<HttpContent> Call(string method, HttpMethod reqMethod, HttpContent data = null) =>
            Call(new Uri(constructMethorUrl(method)), reqMethod, data);
        public Task<T> Call<T>(Uri uri, HttpMethod reqMethod, HttpContent data = null) where T : class =>
            Call<T>(constructDefaultRequest(uri, reqMethod, data));
        public Task<HttpContent> Call(Uri uri, HttpMethod reqMethod, HttpContent data = null) =>
            Call(constructDefaultRequest(uri, reqMethod, data));
        private HttpRequestMessage constructDefaultRequest(Uri uri, HttpMethod reqMethod, HttpContent data = null)
        {
            var request = new HttpRequestMessage
            {
                Method = reqMethod,
                Content = data,
                RequestUri = uri
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.AccessToken);
            return request;
        }
            
        public async Task<T> Call<T>(HttpRequestMessage request) where T : class
        {
            var response = await Call(request);
            return await JsonSerializer.DeserializeAsync<T>(await response.ReadAsStreamAsync());
        }
        public async Task<HttpContent> Call(HttpRequestMessage request)
        {
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var handlingResult = await ExceptionHandler.Handle(this, auth, response);
                if (handlingResult)
                {
                    var clonedRequest = new HttpRequestMessage
                    {
                        Content = request.Content,
                        Method = request.Method,
                        RequestUri = request.RequestUri
                    };
                    foreach (var header in request.Headers)
                        clonedRequest.Headers.Add(header.Key, header.Value);
                    clonedRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.AccessToken);
                    return await Call(clonedRequest);
                }
            }

            logger?.LogInformation($"[{request.Method}] {request.RequestUri.LocalPath}: {(response.IsSuccessStatusCode ? "OK" : "Exception")}");
            return response.Content;
        }

        private string constructMethorUrl(string method) => $@"https://api.spotify.com/v{Version}/{method}";
    }
    class SpotifyExceptionResponseHandler : ISpotifyExceptionResponseHandler
    {
        public async Task<bool> Handle(ISpotifyAPI api, SpotifyAuth auth, HttpResponseMessage response)
        {
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

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                bool isSuccessfullyRefreshed = await auth.TryRefresh(api);
                if (isSuccessfullyRefreshed)
                    return true;
                throw new SpotifyAuthorizationException(message);
            }
            else if (responseBody != null)
                throw new SpotifyRequestException(responseBody);

            throw new SpotifyRequestException(message);
        }
    }
    public interface ISpotifyExceptionResponseHandler
    {
        public Task<bool> Handle(ISpotifyAPI api, SpotifyAuth auth, HttpResponseMessage response);
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
        public Task<T> Post<T, R>(string method, R data)
            where T : class
            where R : class;
        public Task<HttpContent> Post<T>(string method, T data) where T : class;
        public Task<T> Get<T>(string method, IDictionary<string, string> urlParams = null) where T : class;
        public Task<HttpContent> Get(string method, IDictionary<string, string> urlParams = null);
        public Task<T> Call<T>(Uri uri, HttpMethod reqMethod, HttpContent data = null) where T : class;
        public Task<HttpContent> Call(Uri uri, HttpMethod reqMethod, HttpContent data = null);
        public Task<T> Call<T>(string method, HttpMethod reqMethod, HttpContent data = null) where T : class;
        public Task<HttpContent> Call(string method, HttpMethod reqMethod, HttpContent data = null);
        public Task<T> Call<T>(HttpRequestMessage request) where T : class;
        public Task<HttpContent> Call(HttpRequestMessage request);
    }
}
