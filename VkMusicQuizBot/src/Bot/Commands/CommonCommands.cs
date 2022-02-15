using System.Linq;
using VkNet.Model;
using VkNetLongpoll;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace VkMusicQuizBot.src.Bot.Commands
{
    public class CommonCommands : CommandsHandler
    {
        private IFileDatabase db;
        public CommonCommands(LongpollEventHandler lpHandler, IFileDatabase db) : base(lpHandler)
        {
            this.db = db;
        }

        public override void Release()
        {
            lpHandler.HearCommand(new Regex(@"!(?:stat|стат|профиль|profile) ?(\d*)$", RegexOptions.IgnoreCase), async context =>
            {
                var memberId = (await new Utils.MemberIdResolver(context.Api).Resolve(context.Match.Groups[1].Value)) ?? context.Body.FromId;
                var user = await db.Users.FindAsync(memberId);
                if (user == null)
                {
                    await context.ReplyAsync(@$"🔭 [{(memberId < 0 ? "club" : "id")}{System.Math.Abs(memberId.Value)}|Пользователь] не авторизован.");
                    return;
                }

                var vkUser = (await context.Api.Users.GetAsync(new[] { (long)user.Id })).DefaultIfEmpty(null).FirstOrDefault();
                await context.ReplyAsync($@"
                    👤 Профиль {(vkUser == null ? user.GetAppeal("Пользователя") : $"[id{vkUser.Id}|{vkUser.FirstNameIns} {vkUser.LastNameIns}]")}:
                    🔑 Доступ: {user.Access}
                    🪙 Счёт: {user.Score}
                ");
            });
        }
    }
}
