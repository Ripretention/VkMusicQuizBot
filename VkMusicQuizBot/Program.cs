using System;
using System.Threading.Tasks;
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
            var respone = await spotify.Api.Get(@"me/playlists", new Dictionary<string, string> { { "limit", "10" }, { "offset", "0" } });

            Console.WriteLine(await respone.ReadAsStringAsync());
        }
    }
}
