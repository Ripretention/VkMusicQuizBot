using Microsoft.Extensions.Configuration;

namespace VkMusicQuizBot
{
    public class BotConfigurationBuilder
    {
        private readonly string configFilePath;
        public BotConfigurationBuilder(string configFilePath = "appsettings.json") 
        {
            this.configFilePath = configFilePath;
        }
        public IConfiguration Build() => new ConfigurationBuilder().AddJsonFile(configFilePath).Build();
    }

    public class BotConfiguration
    {
        public VkConfiguration Vk { get; set; }
        public FFMpegConfiguration FFMpeg { get; set; }
        public SpotifyConfiguration Spotify { get; set; }
        public DatabaseConfiguration Database { get; set; }
        public System.Collections.Generic.IEnumerable<int> Developers { get; set; }
    }
    public class VkConfiguration
    {
        public string AccessToken { get; set; }
        public string Version { get; set; }
        public ulong GroupId { get; set; }
    }
    public class FFMpegConfiguration
    {
        public string Path { get; set; }
    }
    public class SpotifyConfiguration
    {
        public string PlaylistSourceId { get; set; }
        public SpotifyAuthConfiguration Auth { get; set; }
    }
    public class SpotifyAuthConfiguration
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
    public class DatabaseConfiguration
    {
        public string PathFolder { get; set; }
    }
}
