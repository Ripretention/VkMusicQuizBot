using Moq;
using System.Web;
using System.Net;
using System.Linq;
using Moq.Protected;
using System.Net.Http;
using NUnit.Framework;
using System.Threading;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace VkMusicQuizBot.Tests
{
    [TestFixture]
    public class SpotifyAPITest
    {
        private SpotifyAPI api;
        private SpotifyConfiguration cfg;
        private TrackNameUpdateRequest postReqData = new TrackNameUpdateRequest { Id = "f1", NewName = "NotHazy" };
        [SetUp]
        public void Setup()
        {
            var messageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            messageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns((HttpRequestMessage request, CancellationToken token) => httpMessageHandle(request));

            cfg = new SpotifyConfiguration { AccessToken = "ACCESS_TOKEN", PlaylistSourceTitle = "PLAYLIST" };
            api = new SpotifyAPI(cfg, null, messageHandlerMock.Object);
        }

        [Test]
        public async Task MethodGetTest()
        {
            var response = await api.Get("track/get");
           
            var json = await response.ReadAsStreamAsync();

            Assert.DoesNotThrowAsync(async () => { await JsonDocument.ParseAsync(json); });
        }
        [Test]
        public async Task MethodGetWithDeserializationTest()
        {
            var response = await api.Get<TrackTestSample>("track/get");

            Assert.AreEqual("a1", response.Id);
            Assert.AreEqual("Hazy", response.Name);
        }
        [Test]
        public async Task MethodPostTest()
        {
            var requestData = new StringContent(JsonSerializer.Serialize(postReqData), System.Text.Encoding.UTF8, "application/json");
            
            var response = await api.Post("track/post", requestData);
            var json = await response.ReadAsStreamAsync();

            Assert.DoesNotThrowAsync(async () => { await JsonDocument.ParseAsync(json); });
        }
        [Test]
        public async Task MethodPostWithSerializationTest()
        {
            var response = await api.Post("track/post", postReqData);
            var json = await response.ReadAsStreamAsync();

            Assert.DoesNotThrowAsync(async () => { await JsonDocument.ParseAsync(json); });
        }
        [Test]
        public async Task MethodPostWithSerializationAndDeserializationTest()
        {
            var response = await api.Post<TrackTestSample, TrackNameUpdateRequest>("track/post", postReqData);

            Assert.AreEqual(postReqData.Id, response.Id);
            Assert.AreEqual(postReqData.NewName, response.Name);
        }

        [Test]
        public async Task MethodDeleteTest()
        {
            var response = await api.Delete("track/delete");

            var json = await response.ReadAsStreamAsync();

            Assert.DoesNotThrowAsync(async () => { await JsonDocument.ParseAsync(json); });
        }
        [Test]
        public async Task MethodDeketeWithDeserializationTest([Values("q1", "q2", "tfsdfsdSD231")] string id)
        {
            var response = await api.Delete<TrackDeleteResponse>("track/delete", new Dictionary<string, string> { { "id", id } });

            Assert.AreEqual(id, response.Id);
            Assert.IsTrue(response.Successfully);
        }

        private async Task<HttpResponseMessage> httpMessageHandle(HttpRequestMessage request)
        {
            var response = new HttpResponseMessage();
            response.StatusCode = HttpStatusCode.OK;

            var url = HttpUtility.UrlDecode(request.RequestUri.ToString());
            if (url.EndsWith("track/get") && request.Method == HttpMethod.Get)
                response.Content = new StringContent(JsonSerializer.Serialize(new TrackTestSample() { Id = "a1", Name = "Hazy" }), System.Text.Encoding.UTF8, "application/json");
            if (url.EndsWith("track/post") && request.Method == HttpMethod.Post)
            {
                var @params = await JsonSerializer.DeserializeAsync<TrackNameUpdateRequest>(await request.Content.ReadAsStreamAsync());
                response.Content = new StringContent(JsonSerializer.Serialize(new TrackTestSample() { Id = @params.Id, Name = @params.NewName }), System.Text.Encoding.UTF8, "application/json");
            }
            if (url.Contains("track/delete") && request.Method == HttpMethod.Delete)
                response.Content = new StringContent(JsonSerializer.Serialize(new TrackDeleteResponse() 
                    { 
                        Id = HttpUtility.ParseQueryString(System.String.Concat(url.SkipWhile(c => c != '?')))?.Get("id") ?? "defId", 
                        Successfully = true 
                    }
                ), System.Text.Encoding.UTF8, "application/json");

            return response;
        }
    }

    class TrackTestSample
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
    class TrackNameUpdateRequest
    {
        public string Id { get; set; }
        public string NewName { get; set; }
    }
    class TrackDeleteResponse
    {
        public string Id { get; set; }
        public bool Successfully { get; set; }
    }
}
