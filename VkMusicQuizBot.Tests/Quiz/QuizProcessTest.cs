using System;
using System.Linq;
using NUnit.Framework;

namespace VkMusicQuizBot.Tests.Quiz
{
    [TestFixture]
    public class QuizProcessTest
    {
        private QuizOption[] options = new QuizOption[] 
        { 
            new QuizOption { Title = "First", IsRight = false },
            new QuizOption { Title = "Second", IsRight = true },
            new QuizOption { Title = "Third", IsRight = false }
        };

        [Test]
        public void AddAnswerInExpiredQuizTest()
        {
            var answer = new QuizAnswer();
            var process = new QuizProcess()
            {
                ExpiredAt = DateTime.Now - new TimeSpan(100)
            };

            Assert.Throws<ExpiredOptionException>(() => process.AddAnswer(answer));
        }
        [Test]
        public void AddAnswerWithInvalidOptionInQuizTest()
        {
            var answer = new QuizAnswer()
            {
                Owner = 1,
                Option = new QuizOption 
                { 
                    Title = "Secret Option",
                    IsRight = true
                }
            };
            var process = new QuizProcess();

            Assert.Throws<UnexpectedOptionException>(() => process.AddAnswer(answer));
            process.Options = options;
            Assert.Throws<UnexpectedOptionException>(() => process.AddAnswer(answer));
            answer.Option = null;
            Assert.Throws<ArgumentNullException>(() => process.AddAnswer(answer));
        }
        [Test]
        public void AddAnswerTest()
        {
            var rnd = new Random();
            var process = new QuizProcess
            {
                ExpiredAt = DateTime.Now + TimeSpan.FromMinutes(10),
                Options = options
            };
            var answers = Enumerable.Range(rnd.Next(0, 100), 4).Select(id => new QuizAnswer
            {
                Owner = id,
                Option = options[rnd.Next(0, options.Length)]
            });

            Assert.DoesNotThrow(() =>
            {
                foreach (var answer in answers)
                    process.AddAnswer(answer);
            });
            Assert.Throws<ArgumentException>(() => process.AddAnswer(answers.First()));
        }
    }
}
