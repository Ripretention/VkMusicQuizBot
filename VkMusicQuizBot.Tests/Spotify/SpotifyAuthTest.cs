using Moq;
using System.Net.Http;
using NUnit.Framework;
using System.Text.Json;
using System.Threading.Tasks;

namespace VkMusicQuizBot.Tests.Spotify
{
    public class SpotifyAuthTest
    {
        private Mock<ISpotifyAPI> spotifyApiMock = new Mock<ISpotifyAPI>();
        private SpotifyAuthConfiguration authCfg = new SpotifyAuthConfiguration
        {
            AccessToken = "1232dfsa",
            ClientId = "123dsda",
            ClientSecret = "dqdqwedqw",
            RefreshToken = "wfwefwef"
        };
        [SetUp]
        public void Setup()
        {
            spotifyApiMock
                .Setup(ld => ld.Call<SpotifyRefreshTokenResponse>(It.IsAny<HttpRequestMessage>()))
                .Returns(Task.FromResult(new SpotifyRefreshTokenResponse
                {
                    AccessToken = "123",
                    ExpiresIn = 3000,
                    TokenType = "Bearer"
                }));
        }

        [Test]
        public async Task RefreshTest()
        {
            var refreshed = false;
            var auth = new SpotifyAuth(authCfg);
            auth.OnRefresh += _ =>
            {
                refreshed = true;
            };

            await auth.Refresh(spotifyApiMock.Object);

            Assert.IsTrue(refreshed);
        }
    }
}
