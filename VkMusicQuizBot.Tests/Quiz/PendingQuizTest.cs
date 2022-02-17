using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace VkMusicQuizBot.Tests.Quiz
{
    [TestFixture]
    public class PendingQuizTest
    {
        public void ExceptionsTest()
        {

        }
        [Test]
        public async Task WaitTest()
        {
            var timeNow = DateTime.Now;
            var pendingQuiz = new PendingQuiz(new QuizProcess(), TimeSpan.FromMilliseconds(100));
            
            await pendingQuiz.Wait();

            Assert.IsTrue(DateTime.Now >= timeNow.AddMilliseconds(100));
        }
    }
}
