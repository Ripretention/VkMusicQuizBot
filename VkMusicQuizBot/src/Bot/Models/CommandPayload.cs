using System.Text.Json.Serialization;

namespace VkMusicQuizBot.Utils
{
    public class CommandPayload
    {
        [JsonPropertyName("cmd")]
        public string Command { get; set; }
    }
}
