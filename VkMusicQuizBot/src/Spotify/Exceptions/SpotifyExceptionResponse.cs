using System.Text.Json.Serialization;

namespace VkMusicQuizBot
{
    public class SpotifyExceptionResponse : ISpotifyExpectionResponse
    {
        [JsonPropertyName("error")]
        public SpotifyExceptionResponseBody Body { get; set; }
        public override string ToString() => Body?.ToString() ?? base.ToString();
    }
    public class SpotifyExceptionResponseBody
    {
        [JsonPropertyName("status")]
        public int Code { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; }
        public override string ToString() => $"{Message} (№{Code})";
    }
    public interface ISpotifyExpectionResponse
    {
        public SpotifyExceptionResponseBody Body { get; set; }
    }
}
