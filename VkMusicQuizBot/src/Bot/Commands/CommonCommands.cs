using System.Linq;
using VkNet.Model;
using VkNetLongpoll;
using VkNet.Model.RequestParams;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace VkMusicQuizBot
{
    public class CommonCommands : CommandsHandler
    {
        private IFileDatabase db;
        private IAudioTrackDownloader downloader;
        private Dictionary<long, IEnumerable<QuizProcess>> currentQuizSessions = new Dictionary<long, IEnumerable<QuizProcess>>();
        public CommonCommands(
            LongpollEventHandler lpHandler,
            IFileDatabase db,
            IAudioTrackDownloader downloader
        ) : base(lpHandler)
        {
            this.db = db;
            this.downloader = downloader;
        }

        public override void Release()
        {
            var cmdHandler = lpHandler.CreateGroup(checkAccess);
            cmdHandler.HearCommand(new Regex(@"^!(?:stat|стат|профиль|profile) ?(\d*)$", RegexOptions.IgnoreCase), async context =>
            { 
                var memberId = (await new Utils.MemberIdResolver(context.Api).Resolve(context.Match?.Groups[1]?.Value)) ?? context.Body.FromId;
                var user = await db.Users.FindAsync(memberId.Value);
                if (user == null)
                {
                    await context.ReplyAsync(@$"🔭 [{(memberId < 0 ? "club" : "id")}{System.Math.Abs(memberId.Value)}|Пользователь] не авторизован.");
                    return;
                }
                var vkUser = (await context.Api.Users.GetAsync(new[] { user.Id }, null, VkNet.Enums.SafetyEnums.NameCase.Gen)).FirstOrDefault();
                await context.ReplyAsync(new MessagesSendParams 
                {
                    Message = $@"
                       👤 Профиль {(vkUser == null ? user.GetAppeal("Пользователя") : $"[id{vkUser.Id}|{vkUser.FirstName} {vkUser.LastName}]")}:
                       🔑 Доступ: {user.Access}
                       💎 Счёт: {user.Score}
                       🏆 Статистика: {user.Statistic}
                    ",
                    DisableMentions = true 
                });
            });
            cmdHandler.HearCommand(new Regex(@"^!(?:quiz|викторина|game|play)$", RegexOptions.IgnoreCase), async context =>
            {
                var user = await db.Users.FindAsync(context.Body.FromId.Value);
                if (user == null || user.Access < UserAccess.Default)
                {
                    await context.ReplyAsync("❌ У вас недостаточно прав или вы неавторизированы.");
                    return;
                }
                if (currentQuizSessions.Any(sessions => sessions.Value.Any(session => session.CreatorId == context.Body.FromId.Value)))
                {
                    await context.ReplyAsync("❌ Вы можете быть создателем только одной викторины, ожидайте завершения текущей.");
                    return;
                }

                PendingQuiz quiz;
                try
                {
                    quiz = (await new QuizSession(downloader).Create(null, context.Body.FromId.Value)).Start();
                } 
                catch (System.Exception)
                {
                    await context.ReplyAsync("К сожалению, что-то пошло не так..");
                    throw;
                }
                IEnumerable<QuizProcess> currentProccesses;
                if (!currentQuizSessions.TryGetValue(context.Body.PeerId.Value, out currentProccesses))
                    currentQuizSessions.Add(context.Body.PeerId.Value, new List<QuizProcess>());
                currentQuizSessions[context.Body.PeerId.Value] = currentProccesses.Append(quiz.Process);

                await quiz.Wait();
                await context.SendAsync(@$"
                    Викторина окончена!
                    Победители: {quiz.Process.Answers.Where(answ => answ.Option.IsRight).Select(answ => answ.Owner)}
                ");
            });
        }

        private bool checkAccess(Message msg)
        {
            var usr = db.Users.Find(msg.FromId.Value);
            return usr == null || usr.Access > UserAccess.Banned;
        }
    }
}
