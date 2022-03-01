using VkNet;
using VkNetLongpoll;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace VkMusicQuizBot
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            using var servicesProvider = services.BuildServiceProvider();

            var db = servicesProvider.GetService<IFileDatabase>();
            

            var amdCommands = servicesProvider.GetService<AdministrationCommands>();
            var commonCommands = servicesProvider.GetService<CommonCommands>();
            var newMessageHandler = servicesProvider.GetService<NewMessageHandler>();
            amdCommands.Release();
            commonCommands.Release();
            newMessageHandler.Release();

            var longpoll = servicesProvider.GetService<Longpoll>();
            await longpoll.Start();
        }
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder => builder.AddSimpleConsole());
            services.AddSingleton(p => new BotConfigurationBuilder().Build().Get<BotConfiguration>());
            services.AddSingleton<IFileDatabase>(p => new FileDatabase(p.GetService<BotConfiguration>().Database.PathFolder));
            services.AddSingleton<IAudioTrackDownloader>(p => new AudioTrackDownloader(p.GetService<BotConfiguration>().FFMpeg));
            services.AddSingleton<VkNet.Abstractions.IVkApi>(p =>
            {
                var vkApi = new VkApi();
                vkApi.Authorize(new VkNet.Model.ApiAuthParams { AccessToken = p.GetService<BotConfiguration>().Vk.AccessToken });
                return vkApi;
            });
            services.AddSingleton(p => new Longpoll(p.GetService<VkNet.Abstractions.IVkApi>(), (long)p.GetService<BotConfiguration>().Vk.GroupId));
            services.AddSingleton(p => p.GetService<Longpoll>().Handler);
            services.AddSingleton<IAudioTrackExtractor>(p =>
            {
                var spotifyAuth = new SpotifyAuth(p.GetService<BotConfiguration>().Spotify.Auth, p.GetService<ILogger<SpotifyAuth>>());
                var spotify = new SpotifyClient(new SpotifyAPI(spotifyAuth, null, null, p.GetService<ILogger<SpotifyAPI>>()));
                return new SpotifyAudioTrackExtractor(spotify, p.GetService<BotConfiguration>().Spotify.PlaylistSourceId);
            });
            services.AddSingleton<CommonCommands>();
            services.AddSingleton(p => new AdministrationCommands(
                p.GetService<LongpollEventHandler>(), 
                p.GetService<IFileDatabase>(), 
                p.GetService<BotConfiguration>().Developers,
                p.GetService<ILogger<AdministrationCommands>>()
            ));
            services.AddSingleton<NewMessageHandler>();
        }
    }
}
