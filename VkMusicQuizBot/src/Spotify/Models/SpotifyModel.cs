using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VkMusicQuizBot
{
    public class SpotifyModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("href")]
        public string Href { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("uri")]
        public string Uri { get; set; }
        [JsonPropertyName("external_urls")]
        public IDictionary<string, string> ExternalUrls { get; set; }

        public override string ToString() => $"{GetType().Name}: {Type}-{Id}";
    }

    public class SpotifyImage
    {
        [JsonPropertyName("width")]
        public int? Width { get; set; }
        [JsonPropertyName("height")]
        public int? Height { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
