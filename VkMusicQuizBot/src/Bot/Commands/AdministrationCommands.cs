using System.Linq;
using VkNet.Model;
using VkNetLongpoll;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace VkMusicQuizBot
{
    public class AdministrationCommands : CommandsHandler
    {
        private IFileDatabase db;
        private IOptionsMonitor<BaseConfiguration> cfg;
        private ILogger<AdministrationCommands> logger;
        public AdministrationCommands(
            LongpollEventHandler lpHandler, 
            IFileDatabase db,
            IOptionsMonitor<BaseConfiguration> cfg, 
            ILogger<AdministrationCommands> logger = null
        ) : base(lpHandler) 
        {
            this.db = db;
            this.logger = logger;
            this.cfg = cfg;
        }
        public override void Release()
        {
            var cmdHandler = lpHandler.CreateGroup(checkAccess);
            cmdHandler.HearCommand(new[] { "!state", "!test", "!тест" }, context => context.ReplyAsync($"Work 🔌"));
            cmdHandler.HearCommand(new Regex(@"^!(?:access|доступ)$", RegexOptions.IgnoreCase), context => 
                context.ReplyAsync($"👤 Уровень ваших прав: {db.Users.Find(context.Body.FromId)?.Access.ToString() ?? "неавторизован"}"));
            cmdHandler.HearCommand(new Regex(@"^!(?:up|ап|update|auth[a-z]*)$", RegexOptions.IgnoreCase), async context =>
            {
                if (db.Users.Any(usr => usr.Id == context.Body.FromId))
                {
                    await context.ReplyAsync(@"👣 Вы уже авторизованы.");
                    return;
                }

                var result = db.Users.Add(new User 
                { 
                    Id = context.Body.FromId.Value,
                    Access = UserAccess.Administration
                });

                await db.SaveChangesAsync();
                await context.ReplyAsync(@"👤 Вы успешно авторизовались.");

                logger?.LogInformation($"User #{context.Body.FromId} has gotten [{UserAccess.Administration}] AcessLevel");
            });
            cmdHandler.HearCommand(new Regex(@"^!(?:conf.*|разрешение|add user|разрешить) ?(\d*)$", RegexOptions.IgnoreCase), async context =>
            {
                long? memberId = await new Utils.MemberIdResolver(context.Api).Resolve(context.Body, context.Match.Groups[1].Value);

                if (memberId == null)
                {
                    await context.ReplyAsync(@"🔭 Мне не удалось распознать пользователя.");
                    return;
                }
                var user = await db.Users.FindAsync(memberId.Value);
                if (user != null)
                {
                    await context.ReplyAsync(@$"👣 {user.GetAppeal()} уже авторизован.");
                    return;
                }

                user = db.Users.Add(new User
                {
                    Id = memberId.Value,
                    Access = UserAccess.Default
                }).Entity;
                await db.SaveChangesAsync();
                await context.ReplyAsync(@$"👤 {user.GetAppeal()} успешно авторизован.");

                logger?.LogInformation($"User #{context.Body.FromId} has authorized");
            });
            cmdHandler.HearCommand(new Regex(@"^!(?:down.*|понизить|ban|unperm) ?(\d*)$", RegexOptions.IgnoreCase), async context =>
            {
                long? memberId = await new Utils.MemberIdResolver(context.Api).Resolve(context.Body, context.Match.Groups[1].Value);

                if (memberId == null)
                {
                    await context.ReplyAsync(@"🔭 Мне не удалось распознать пользователя.");
                    return;
                }
                var user = await db.Users.FindAsync(memberId);
                if (user == null)
                {
                    await context.ReplyAsync(@$"👣 [{(memberId > 0 ? "id" : "club")}{System.Math.Abs(memberId.Value)}|Пользователь] не авторизован.");
                    return;
                }
                var sender = await db.Users.FindAsync(context.Body.FromId);
                if (sender == null || sender.Access < user.Access)
                {
                    await context.ReplyAsync($"❌ У Вас недостаточно прав, чтобы понизить права {user.GetAppeal()}");
                    return;
                }

                if (user.Access > 0)
                {
                    --user.Access;
                    await db.SaveChangesAsync();
                }
                await context.ReplyAsync(@$"👤 Уровень прав {user.GetAppeal()} понижен до {user.Access}.");

                logger?.LogInformation($"User's #{context.Body.FromId} access level has downgraded to {user.Access}");
            });

            logger?.LogDebug($"{cmdHandler.CommandsCount} admCommands have initialized");
        }
        private bool checkAccess(Message msg) => 
            cfg.CurrentValue.Developers.Any(id => id == msg.FromId) || db.Users.Any(usr => usr.Id == msg.FromId && usr.Access > UserAccess.Default);
    }
}
