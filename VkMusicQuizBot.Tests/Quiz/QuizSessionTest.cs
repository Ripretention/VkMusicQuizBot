using Moq;
using System;
using System.Linq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace VkMusicQuizBot.Tests.Quiz
{
    [TestFixture]
    public class QuizSessionTest
    {
        public AudioTrack[] tracks;
        public Mock<IAudioTrackDownloader> audioDownloaderMock = new Mock<IAudioTrackDownloader>();
        
        [SetUp]
        public void Setup()
        {
            var rnd = new Random();
            tracks = new AudioTrack[]
            {
                new AudioTrack
                {
                    Artist = "Rammstein",
                    Name = "Mein Herz brennt",
                    Duration = TimeSpan.FromSeconds(rnd.Next(30, 480))
                },
                new AudioTrack
                {
                    Artist = "Queen",
                    Name = "I want it all",
                    Duration = TimeSpan.FromSeconds(rnd.Next(30, 480))
                },
                new AudioTrack
                {
                    Artist = "Frederic Chopin",
                    Name = "Nocturne Op.9 No.2",
                    Duration = TimeSpan.FromSeconds(rnd.Next(30, 480))
                },
                new AudioTrack
                {
                    Artist = "Rick Astley",
                    Name = "Never Gonna Give You Up",
                    Duration = TimeSpan.FromSeconds(rnd.Next(30, 480))
                }
            };

            audioDownloaderMock
                .Setup(fd => fd.Download(It.IsAny<AudioTrack>()))
                .Returns(Task.FromResult(new byte[] { 0, 0, 0 }));
        }

        [Test]
        public async Task TrackRandomPickTest()
        {

            for (int i = 1; i < 3; i++)                
                Assert.AreEqual(i, (await new QuizSession(audioDownloaderMock.Object).Create(tracks.Take(i), 0)).Start().Process.Options.Count());

            Assert.ThrowsAsync<ArgumentException>(async () => await new QuizSession(audioDownloaderMock.Object).Create(null, 0));
            Assert.ThrowsAsync<ArgumentException>(async () => await new QuizSession(audioDownloaderMock.Object).Create(new AudioTrack[0], 0));
        }
    }
}
