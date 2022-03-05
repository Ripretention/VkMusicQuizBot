using Moq;
using System.Net.Http;
using NUnit.Framework;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace VkMusicQuizBot.Tests.Spotify
{
    public class SpotifyAuthTest
    {
        private Mock<ISpotifyAPI> spotifyApiMock = new Mock<ISpotifyAPI>();
        private Mock<IOptionsMonitor<SpotifyAuthConfiguration>> spotifyAuthCfgMock = new Mock<IOptionsMonitor<SpotifyAuthConfiguration>>();
        
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
            spotifyAuthCfgMock
                .Setup(ld => ld.CurrentValue)
                .Returns(new SpotifyAuthConfiguration 
                { 
                    AccessToken = "1232dfsa",
                    ClientId = "123dsda",
                    ClientSecret = "dqdqwedqw",
                    RefreshToken = "wfwefwef"
                });
        }

        [Test]
        public async Task RefreshTest()
        {
            var refreshed = false;
            var auth = new SpotifyAuth(spotifyAuthCfgMock.Object);
            auth.OnRefresh += _ =>
            {
                refreshed = true;
            };

            await auth.Refresh(spotifyApiMock.Object);

            Assert.IsTrue(refreshed);
        }
    }
}
