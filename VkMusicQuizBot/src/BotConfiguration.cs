namespace VkMusicQuizBot
{
    public class BaseConfiguration
    {
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
