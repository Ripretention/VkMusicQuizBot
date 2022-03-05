using VkNet;
using VkNetLongpoll;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
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
            var configuration = new ConfigurationBuilder();
            Configure(configuration, "appsettings.json");
            ConfigureServices(services, configuration.Build());

            using var servicesProvider = services.BuildServiceProvider();

            var amdCommands = servicesProvider.GetService<AdministrationCommands>();
            var commonCommands = servicesProvider.GetService<CommonCommands>();
            var newMessageHandler = servicesProvider.GetService<NewMessageHandler>();
            amdCommands.Release();
            commonCommands.Release();
            newMessageHandler.Release();

            var longpoll = servicesProvider.GetService<Longpoll>();
            await longpoll.Start();
        }
        public static void Configure(ConfigurationBuilder builder, string path)
        {
            builder.AddJsonFile(path);
        }
        public static void ConfigureServices(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddLogging(builder => builder.AddSimpleConsole());
            services
                .Configure<BaseConfiguration>(configuration)
                .Configure<VkConfiguration>(configuration.GetSection("Vk"))
                .Configure<FFMpegConfiguration>(configuration.GetSection("FFmpeg"))
                .Configure<SpotifyConfiguration>(configuration.GetSection("Spotify"))
                .Configure<DatabaseConfiguration>(configuration.GetSection("Database"))
                .Configure<SpotifyAudioTrackExtractor>(configuration.GetSection("Spotify:Auth"));
            services
                .AddSingleton<IFileDatabase>(p => new FileDatabase(p.GetService<IOptions<DatabaseConfiguration>>().Value.PathFolder))
                .AddSingleton<IAudioTrackDownloader, AudioTrackDownloader>()
                .AddSingleton<VkNet.Abstractions.IVkApi>(p =>
                {
                    var vkApi = new VkApi();
                    vkApi.Authorize(new VkNet.Model.ApiAuthParams { AccessToken = p.GetService<IOptions<VkConfiguration>>().Value.AccessToken });
                    return vkApi;
                })
                .AddSingleton(p => new Longpoll(p.GetService<VkNet.Abstractions.IVkApi>(), (long)p.GetService<IOptions<VkConfiguration>>().Value.GroupId))
                .AddSingleton(p => p.GetService<Longpoll>().Handler)
                .AddSingleton<IAudioTrackExtractor>(p =>
                {
                    var spotifyAuth = new SpotifyAuth(p.GetService<IOptionsMonitor<SpotifyAuthConfiguration>>(), p.GetService<ILogger<SpotifyAuth>>());
                    var spotify = new SpotifyClient(new SpotifyAPI(spotifyAuth, null, null, p.GetService<ILogger<SpotifyAPI>>()));
                    return new SpotifyAudioTrackExtractor(spotify, p.GetService<IOptions<SpotifyConfiguration>>().Value.PlaylistSourceId);
                })
                .AddSingleton<CommonCommands>()
                .AddSingleton<AdministrationCommands>()
                .AddSingleton<NewMessageHandler>();
        }
    }
}
