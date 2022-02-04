using System;
using System.Runtime.Serialization;

namespace VkMusicQuizBot
{
    [Serializable]
    public class SpotifyAuthorizationException : Exception
    {
        public SpotifyAuthorizationException()
        {
        }

        public SpotifyAuthorizationException(string message)
            : base(message)
        {
        }

        public SpotifyAuthorizationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected SpotifyAuthorizationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
