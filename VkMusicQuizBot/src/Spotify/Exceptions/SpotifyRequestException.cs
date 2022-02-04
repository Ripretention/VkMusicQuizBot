using System;
using System.Runtime.Serialization;

namespace VkMusicQuizBot
{
    [Serializable]
    public class SpotifyRequestException : Exception
    {
        public SpotifyExceptionResponseBody Response;

        public SpotifyRequestException()
        {
        }

        public SpotifyRequestException(SpotifyExceptionResponseBody response)
            : base(response.ToString())
        {
            Response = response;
        }

        public SpotifyRequestException(string message)
            : base(message)
        {
        }

        public SpotifyRequestException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected SpotifyRequestException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
