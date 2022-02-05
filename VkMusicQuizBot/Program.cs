using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace VkMusicQuizBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var cfg = new BotConfigurationBuilder().Build().Get<BotConfiguration>();
            var spotify = new SpotifyClient(cfg.Spotify);
            var respone = await spotify.GetPlaylistTracks("succ");

            Console.WriteLine(respone.First().Name);
        }
    }
}
