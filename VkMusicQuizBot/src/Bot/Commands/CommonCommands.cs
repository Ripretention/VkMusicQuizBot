using System;
using System.Linq;
using VkNet.Model;
using VkNetLongpoll;
using VkNet.Model.Keyboard;
using VkNet.Model.RequestParams;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace VkMusicQuizBot
{
    public class CommonCommands : CommandsHandler
    {
        private IFileDatabase db;
        private IAudioTrackDownloader downloader;
        private IAudioTrackExtractor audioTrackExtractor;
        private Dictionary<long, IEnumerable<QuizProcess>> currentQuizSessions = new Dictionary<long, IEnumerable<QuizProcess>>();
        public CommonCommands(
            LongpollEventHandler lpHandler,
            IFileDatabase db,
            IAudioTrackExtractor audioTrackExtractor,
            IAudioTrackDownloader downloader
        ) : base(lpHandler)
        {
            this.db = db;
            this.downloader = downloader;
            this.audioTrackExtractor = audioTrackExtractor;
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
            cmdHandler.HearCommand(new Regex(@"^!(?:quiz|викторина|game|play) (?:choose|выбрать|select) (\d+)-(\d+) (\d+)", RegexOptions.IgnoreCase), async context =>
            {
                long peerId = 0;
                long ownerId = 0;
                int optionId = 0;
                try
                {
                    peerId = long.Parse(context.Match.Groups[1].Value.Trim());
                    ownerId = long.Parse(context.Match.Groups[2].Value.Trim());
                    optionId = int.Parse(context.Match.Groups[3].Value.Trim());
                } 
                catch (Exception)
                {
                    await context.ReplyAsync("Неверный интидификатор");
                    return;
                }

                QuizProcess quiz;
                try
                {
                    quiz = currentQuizSessions[peerId].First(qProcess => qProcess.CreatorId == ownerId);
                }
                catch (Exception)
                {
                    await context.ReplyAsync($"Викторина не существует.");
                    return;
                }

                try
                {
                    quiz.AddAnswer(new QuizAnswer
                    {
                        Owner = context.Body.FromId.Value,
                        Option = quiz.Options.ElementAt(optionId)
                    });
                    await context.ReplyAsync("Ответ принят.");
                }
                catch (ExpiredOptionException)
                {
                    await context.ReplyAsync("Викторина заверешена");
                }
                catch (ArgumentException)
                {
                    await context.ReplyAsync("Вы уже сделали выбор.");
                }
                catch (Exception)
                {
                    await context.ReplyAsync("Что-то пошло не так.");
                    throw;
                }
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
                    quiz = (await new QuizSession(downloader).Create(await audioTrackExtractor.Extract(), context.Body.FromId.Value)).Start();
                } 
                catch (Exception)
                {
                    await context.ReplyAsync("К сожалению, что-то пошло не так..");
                    throw;
                }
                IEnumerable<QuizProcess> currentProccesses;
                if (!currentQuizSessions.TryGetValue(context.Body.PeerId.Value, out currentProccesses))
                {
                    currentProccesses = new List<QuizProcess>();
                    currentQuizSessions.Add(context.Body.PeerId.Value, new List<QuizProcess>());
                }
                currentQuizSessions[context.Body.PeerId.Value] = currentProccesses.Append(quiz.Process);

                await context.SendAudioMessage(quiz.Process.QuestionBody, "ogg");
                
                KeyboardBuilder keyboard = new KeyboardBuilder();
                keyboard.SetInline(true);
                for (int i = 0; i < quiz.Process.Options.Count(); i++)
                    keyboard.AddButton(new MessageKeyboardButtonAction
                    {
                        Label = quiz.Process.Options.ElementAt(i).Title,
                        Type = VkNet.Enums.SafetyEnums.KeyboardButtonActionType.Callback,
                        Payload = System.Text.Json.JsonSerializer.Serialize(new Utils.CommandPayload 
                        { 
                            Command = $"!quiz select {context.Body.PeerId}-{context.Body.FromId.Value} {i}"
                        }),
                    }, VkNet.Enums.SafetyEnums.KeyboardButtonColor.Default);
                await context.SendAsync(new MessagesSendParams
                {
                    Message = @"Каков трек?",
                    Keyboard = keyboard.Build()
                });

                await quiz.Wait();
                await context.SendAsync(@$"
                    Викторина окончена!
                    Победители: {String.Join(", ", quiz.Process.Answers.Where(answ => answ.Option.IsRight).Select(answ => $"[{(answ.Owner < 0 ? "club" : "id")}{answ.Owner}|member]"))}
                ");
            });
            cmdHandler.HearCommand(new Regex(@"^!(?:top|топ) (\d{1,2})", RegexOptions.IgnoreCase), async context =>
            {
                var topUsers = db.Users.OrderBy(u => u.Statistic.WinCount).Take(10);
                var users = await context.Api.Users.GetAsync(topUsers.Select(u => u.Id));

                await context.ReplyAsync(users.Count == 0
                    ? $"Топ пуст."
                    : $"Топ игроков: {String.Join("\n", users.Select(u => $"[id{u.Id}|{u.FirstName} {u.LastName}]"))}"
                );
            });
        }

        private bool checkAccess(Message msg)
        {
            var usr = db.Users.Find(msg.FromId.Value);
            return usr == null || usr.Access > UserAccess.Banned;
        }
    }
}
