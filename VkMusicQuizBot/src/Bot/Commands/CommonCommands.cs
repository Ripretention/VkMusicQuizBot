using System;
using System.Linq;
using VkNet.Model;
using VkNetLongpoll;
using VkNet.Model.Keyboard;
using VkNet.Model.RequestParams;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace VkMusicQuizBot
{
    public class CommonCommands : CommandsHandler
    {
        private IFileDatabase db;
        private ILogger<CommonCommands> logger;
        private IAudioTrackDownloader downloader;
        private IAudioTrackExtractor audioTrackExtractor;
        private Dictionary<long, IEnumerable<QuizProcess>> currentQuizSessions = new Dictionary<long, IEnumerable<QuizProcess>>();
        public CommonCommands(
            LongpollEventHandler lpHandler,
            IFileDatabase db,
            IAudioTrackExtractor audioTrackExtractor,
            IAudioTrackDownloader downloader,
            ILogger<CommonCommands> logger = null
        ) : base(lpHandler)
        {
            this.db = db;
            this.logger = logger;
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
                var userAppeal = vkUser == null
                    ? user.GetAppeal("Пользователя")
                    : $"[id{vkUser.Id}|{vkUser.FirstName} {vkUser.LastName}]";

                await context.ReplyAsync(new MessagesSendParams
                {
                    Message = $@"
                       👤 Профиль {userAppeal}:
                       🔑 Доступ: {user.Access}
                       💎 Счёт: {user.Score:N0}
                       🏆 Статистика: {user.Statistic}
                    ",
                    DisableMentions = true
                });

                logger?.LogInformation($"{userAppeal} has received statistic");
            });
            cmdHandler.HearCommand(new Regex(@"^!(?:quiz|викторина|game|play) (?:choose|выбрать|select) (\d+) (\d+)", RegexOptions.IgnoreCase), async context =>
            {
                long ownerId = 0;
                int optionId = 0;
                try
                {
                    ownerId = long.Parse(context.Match.Groups[1].Value.Trim());
                    optionId = int.Parse(context.Match.Groups[2].Value.Trim());
                }
                catch (Exception)
                {
                    await context.ReplyAsync("📛 Неверный интидификатор");
                    return;
                }

                if (await db.Users.FindAsync(context.Body.FromId) == null)
                {
                    db.Users.Add(new User { Id = context.Body.FromId.Value });
                    await db.SaveChangesAsync();
                }

                QuizProcess quiz;
                try
                {
                    quiz = currentQuizSessions[context.Body.PeerId.Value].First(qProcess => qProcess.CreatorId == ownerId);
                }
                catch (Exception)
                {
                    await context.ReplyAsync($"❌ Викторина не существует.");
                    return;
                }

                try
                {
                    quiz.AddAnswer(new QuizAnswer
                    {
                        Owner = context.Body.FromId.Value,
                        Option = quiz.Options.ElementAt(optionId)
                    });
                    await context.ReplyAsync("✔ Ответ принят.");

                    logger?.LogInformation($"User {context.Body.FromId} has voted");
                }
                catch (ExpiredOptionException)
                {
                    await context.ReplyAsync("💯 Викторина заверешена");
                }
                catch (ArgumentException)
                {
                    await context.ReplyAsync("❌ Вы уже сделали выбор.");
                }
                catch (Exception)
                {
                    await context.ReplyAsync("💢 Что-то пошло не так.");
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

                await context.SendAsync($"📣 Викторина #{context.Body.FromId} начата!");
                System.Threading.ThreadPool.QueueUserWorkItem(async _ =>
                {
                    PendingQuiz quiz;
                    try
                    {
                        quiz = (await new QuizSession(downloader).Create(await audioTrackExtractor.Extract(), context.Body.FromId.Value)).Start();
                    }
                    catch (Exception)
                    {
                        await context.ReplyAsync("⛔ К сожалению, что-то пошло не так..");
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
                            Label = new[] { "1⃣", "2⃣", "3⃣", "4⃣" }?.ElementAtOrDefault(i) ?? (i+1).ToString(),
                            Type = VkNet.Enums.SafetyEnums.KeyboardButtonActionType.Text,
                            Payload = System.Text.Json.JsonSerializer.Serialize(new Utils.CommandPayload
                            {
                                Command = $"!quiz select {context.Body.FromId.Value} {i}"
                            }),
                        }, VkNet.Enums.SafetyEnums.KeyboardButtonColor.Default);
                    await context.SendAsync(new MessagesSendParams
                    {
                        Message = $"🔢 Каков трек? \n {String.Join("\n", quiz.Process.Options.Select((opt, i) => $"{i+1}. {opt.Title}"))}",
                        Keyboard = keyboard.Build()
                    });

                    logger?.LogInformation($"Quiz #{context.Body.FromId} has started");

                    await quiz.Wait();
                    var rightAnswer = quiz.Process.Options.First(opt => opt.IsRight).Title;
                    var quizMembersIds = quiz.Process.Answers.Where(answ => answ.Owner > 0).Select(answ => answ.Owner);
                    var winnersIds = quiz.Process.Answers.Where(answ => answ.Option.IsRight && answ.Owner > 0).Select(answ => answ.Owner);

                    foreach (var usr in db.Users.Where(usr => quizMembersIds.Contains(usr.Id)))
                    {
                        if (winnersIds.Contains(usr.Id))
                        {
                            usr.Score++;
                            usr.Statistic.WinCount++;
                        }
                        else
                            usr.Statistic.LoseCount++;
                    }
                    await db.SaveChangesAsync();

                    keyboard = new KeyboardBuilder();
                    keyboard.SetInline(true);
                    keyboard.AddButton(new MessageKeyboardButtonAction 
                    { 
                        Label = "🔄 Играть снова",
                        Type = VkNet.Enums.SafetyEnums.KeyboardButtonActionType.Text,
                        Payload = System.Text.Json.JsonSerializer.Serialize(new Utils.CommandPayload
                        {
                            Command = $"!play"
                        }),
                    });

                    var winners = winnersIds.Count() > 0
                        ? await context.Api.Users.GetAsync(winnersIds)
                        : null;
                    await context.SendAsync(new MessagesSendParams 
                    { 
                        Keyboard = keyboard.Build(),
                        Message = @$"
                            📣 Викторина окончена!
                            🎵 Верный ответ: [id1|{rightAnswer}]
                            🎉 Победители: {( winners != null 
                                ? String.Join(", ", winners.Select(winner => $"[id{winner.Id}|{winner.LastName}]")) 
                                : "никто" 
                            )}
                        ",
                        DisableMentions = true
                    });

                    currentQuizSessions[context.Body.PeerId.Value] = currentQuizSessions[context.Body.PeerId.Value].Where(q => q.CreatorId != quiz.Process.CreatorId);
                    logger?.LogInformation($"Quiz #{context.Body.FromId} has finished");
                });
            });
            cmdHandler.HearCommand(new Regex(@"^!(?:top|топ)$", RegexOptions.IgnoreCase), async context =>
            {
                var topUsers = db.Users.OrderByDescending(u => u.Statistic.WinCount).Take(10);
                var users = await context.Api.Users.GetAsync(topUsers.Select(u => u.Id));

                await context.ReplyAsync(users.Count == 0
                    ? $"⭕ Топ пуст."
                    : $"📜 Топ игроков: \n {String.Join("\n", users.Select((u, i) => $"[id{u.Id}|{i+1}. {u.FirstName} {u.LastName}] ({(topUsers?.ElementAtOrDefault(i)?.Score ?? 0).ToString("N0")}"))}"
                );
            });

            logger?.LogDebug($"{cmdHandler.CommandsCount} commands have initialized");
        }
        private bool checkAccess(Message msg)
        {
            var usr = db.Users.Find(msg.FromId.Value);
            return usr == null || usr.Access > UserAccess.Banned;
        }
    }
}
