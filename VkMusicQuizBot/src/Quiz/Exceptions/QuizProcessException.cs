using System;
using System.Runtime.Serialization;

namespace VkMusicQuizBot
{
    [Serializable]
    public class UnexpectedOptionException : Exception
    {
        public UnexpectedOptionException()
        {
        }

        public UnexpectedOptionException(string message)
            : base(message)
        {
        }

        public UnexpectedOptionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected UnexpectedOptionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable]
    public class ExpiredOptionException : Exception
    {
        public ExpiredOptionException()
        {
        }

        public ExpiredOptionException(string message)
            : base(message)
        {
        }

        public ExpiredOptionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ExpiredOptionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
