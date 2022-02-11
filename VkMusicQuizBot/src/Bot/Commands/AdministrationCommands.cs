using System;
using VkNetLongpoll;

namespace VkMusicQuizBot
{
    public class AdministrationCommands : CommandsHandler
    {
        public AdministrationCommands(LongpollEventHandler lpHandler) : base(lpHandler) { }
        public override void Release()
        {
            lpHandler.HearCommand(new[] { "!state", "!test", "!тест" }, context => context.ReplyAsync($"🔌 Work"));
        }
    }
}
