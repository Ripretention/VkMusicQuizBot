﻿using Microsoft.Extensions.Configuration;

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
        public string AccessToken { get; set; }
        public string PlaylistSourceTitle { get; set; }
    }
}
