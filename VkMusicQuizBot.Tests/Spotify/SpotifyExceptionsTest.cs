using Moq;
using System.Net;
using Moq.Protected;
using NUnit.Framework;
using System.Net.Http;
using System.Threading;
using System.Text.Json;
using System.Threading.Tasks;

namespace VkMusicQuizBot.Tests
{
    [TestFixture]
    public class SpotifyExceptionsTest
    {
        private SpotifyConfiguration cfg;
        private SpotifyAPI api;
        [SetUp]
        public void Setup()
        {
            var messageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            messageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
                {
                    if (request.RequestUri.ToString().EndsWith("access/check"))
                        return new HttpResponseMessage()
                        {
                            StatusCode = HttpStatusCode.Unauthorized,
                            Content = new StringContent(JsonSerializer.Serialize(new SpotifyExceptionResponseBody() { Code = 401, Message = "Invalid access token" }))
                        };

                    return new HttpResponseMessage()
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Content = new StringContent(JsonSerializer.Serialize(new SpotifyExceptionResponseBody() { Code = 404, Message = "Unexpected service" }))
                    };
                });

            cfg = new SpotifyConfiguration { AccessToken = "ACCESS_TOKEN", PlaylistSourceTitle = "PLAYLIST" };
            api = new SpotifyAPI(cfg, null, messageHandlerMock.Object);
        }

        [Test]
        public void AuthorizationExceptionTest() =>
            Assert.ThrowsAsync<SpotifyAuthorizationException>(async () => { await api.Get("access/check"); });
        [Test]
        public void SpotifyRequestExceptionTest() =>
            Assert.ThrowsAsync<SpotifyRequestException>(async () => { await api.Get("wrong/method"); });
    }
}