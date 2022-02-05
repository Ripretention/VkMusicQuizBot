using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VkMusicQuizBot
{
    public class SpotifyAlbum : SpotifyModel
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("total_tracks")]
        public uint TotalTracks { get; set; }
        [JsonPropertyName("album_type")]
        public string AlbumType { get; set; }
        [JsonPropertyName("release_date")]
        public string ReleaseData { get; set; }
        [JsonPropertyName("release_date_precision")]
        public string ReleaseDatePrecision { get; set; }
        [JsonPropertyName("images")]
        public IEnumerable<SpotifyImage> Images { get; set; }
        [JsonPropertyName("artists")]
        public IEnumerable<SpotifyArtist> Artists { get; set; }
    }
}