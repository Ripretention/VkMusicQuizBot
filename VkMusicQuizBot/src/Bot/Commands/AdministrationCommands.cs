using System;
using System.Linq;
using VkNet.Model;
using VkNetLongpoll;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace VkMusicQuizBot
{
    public class AdministrationCommands : CommandsHandler
    {
        private IFileDatabase db;
        private IEnumerable<int> developers;
        public AdministrationCommands(LongpollEventHandler lpHandler, IFileDatabase db, IEnumerable<int> developers) : base(lpHandler) 
        {
            this.db = db;
            this.developers = developers;
        }
        public override void Release()
        {
            var cmdHandler = lpHandler.CreateGroup(checkAccess);
            cmdHandler.HearCommand(new[] { "!state", "!test", "!тест" }, context => context.ReplyAsync($"Work 🔌"));
            cmdHandler.HearCommand(new Regex(@"^!(?:access|доступ)$"), context => 
                context.ReplyAsync($"👤 Уровень ваших прав: {db.Users.Find((int)context.Body.FromId)?.Access.ToString() ?? "неавторизован"}"));
            cmdHandler.HearCommand(new Regex(@"^!(?:up|ап|update|auth[a-z]*)$"), async context =>
            {
                if (db.Users.Any(usr => usr.Id == context.Body.FromId))
                {
                    await context.ReplyAsync(@"👣 Вы уже авторизованы.");
                    return;
                }

                db.Users.Add(new User 
                { 
                    Id = (int)context.Body.FromId,
                    Access = UserAccess.Administration
                });
                await db.SaveChangesAsync();
                await context.ReplyAsync(@"👤 Вы успешно авторизовались.");
            });
            cmdHandler.HearCommand(new Regex(@"^!(?:conf.*|разрешение|add user|разрешить) ?(\d*)$", RegexOptions.IgnoreCase), async context =>
            {
                long? memberId = await new Utils.MemberIdResolver(context.Api).Resolve(context.Body, context.Match.Groups[1].Value);

                if (memberId == null)
                {
                    await context.ReplyAsync(@"🔭 Мне не удалось распознать пользователя.");
                    return;
                }
                var user = await db.Users.FindAsync((int)memberId);
                if (user != null)
                {
                    await context.ReplyAsync(@$"👣 {user.GetAppeal()} уже авторизован.");
                    return;
                }

                user = db.Users.Add(new User
                {
                    Id = (int)memberId,
                    Access = UserAccess.Default
                }).Entity;
                await db.SaveChangesAsync();
                await context.ReplyAsync(@$"👤 {user.GetAppeal()} успешно авторизован.");
            });
        }
        private bool checkAccess(Message msg) => developers.Any(id => id == msg.FromId) || db.Users.Any(usr => usr.Id == msg.FromId && usr.Access > UserAccess.Default);
    }
}
