using System;
using System.Threading.Tasks;

namespace VkMusicQuizBot
{
    public class PendingQuiz
    {
        private TimeSpan duration;
        public readonly QuizProcess Process;
        public PendingQuiz(QuizProcess process, TimeSpan? duration)
        {
            Process = process ?? throw new ArgumentNullException(nameof(process));
            this.duration = duration ?? throw new ArgumentNullException(nameof(duration));
        }

        public Task Wait() => Task.Delay(duration);
    }
}
