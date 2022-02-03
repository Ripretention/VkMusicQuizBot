using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace VkMusicQuizBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var cfg = new BotConfigurationBuilder().Build().Get<BotConfiguration>();
            var track = new AudioTrack
            {
                Artist = "Rick Astley",
                Name = "Never Gonna Give You Up (Official Music Video)",
                Duration = new TimeSpan(0, 3, 32)
            };

            System.IO.File.WriteAllBytes("test.ogg", await new AudioTrackDownloader(cfg.FFMpeg).Download(track));
        }
    }
}
