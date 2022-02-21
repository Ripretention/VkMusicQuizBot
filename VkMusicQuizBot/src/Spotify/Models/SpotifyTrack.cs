using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VkMusicQuizBot
{
    public class SpotifyTrackCollection
    {
        [JsonPropertyName("items")]
        public IEnumerable<SpotifyTrackCollectionItem> Items { get; set; }
    }
    public class SpotifyTrackCollectionItem
    {
        [JsonPropertyName("added_at")]
        public string AddedAt { get; set; }
        [JsonPropertyName("track")]
        public SpotifyTrack Track { get; set; }
        [JsonPropertyName("added_by")]
        public SpotifyModel AddedBy { get; set; }
    }
    public class SpotifyTrack : SpotifyModel
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("album")]
        public SpotifyAlbum Album { get; set; }
        [JsonPropertyName("artists")]
        public IEnumerable<SpotifyArtist> Artists { get; set; }

        [JsonPropertyName("duration_ms")]
        public long DurationMs { get; set; }
        [JsonPropertyName("disc_number")]
        public int DiscNumber { get; set; }
        [JsonPropertyName("episode")]
        public bool Episode { get; set; }
        [JsonPropertyName("explicit")]
        public bool Explicit { get; set; }
        [JsonPropertyName("popularity")]
        public int Popularity { get; set; }
        [JsonPropertyName("is_playable")]
        public bool IsPlayable { get; set; }
        [JsonPropertyName("preview_url")]
        public string PreviewUrl { get; set; }

        public TimeSpan Duration { get => TimeSpan.FromMilliseconds(DurationMs); }
    }

    public class SpotifyArtist : SpotifyModel
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
