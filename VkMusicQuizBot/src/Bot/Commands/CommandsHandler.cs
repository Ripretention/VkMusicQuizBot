using System;
using VkNetLongpoll;

namespace VkMusicQuizBot
{
    public abstract class CommandsHandler : ICommandsHandler
    {
        protected LongpollEventHandler lpHandler;
        public CommandsHandler(LongpollEventHandler lpHandler)
        {
            this.lpHandler = lpHandler ?? throw new ArgumentNullException(nameof(lpHandler));
        }

        public virtual void Release() =>
            throw new NotImplementedException();
    }
    public interface ICommandsHandler
    {
        public void Release();
    }
}
