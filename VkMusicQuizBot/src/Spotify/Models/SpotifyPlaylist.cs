using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VkMusicQuizBot
{
    public class SpotifyPlaylistCollection
    {
        [JsonPropertyName("items")]
        public IEnumerable<SpotifyPlaylist> Items { get; set; }
    }
    public class SpotifyPlaylist : SpotifyModel
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("public")]
        public bool Public { get; set; }
        [JsonPropertyName("snapshot_id")]
        public string SnapshotId { get; set; }
        [JsonPropertyName("collaborative")]
        public bool Collaborative { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("primary_color")]
        public string PrimaryColor { get; set; }
        [JsonPropertyName("owner")]
        public SpotifyPlaylistOwner Owner { get; set; }
        [JsonPropertyName("tracks")]
        public SpotifyPlaylistTracks Tracks { get; set; }
        [JsonPropertyName("images")]
        public IEnumerable<SpotifyImage> Images { get; set; }
    }

    public class SpotifyPlaylistOwner : SpotifyModel
    {
        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }
    }
    public class SpotifyPlaylistTracks
    {
        [JsonPropertyName("href")]
        public string Href { get; set; }
        [JsonPropertyName("total")]
        public int Total { get; set; }
    }
}
