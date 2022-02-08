using System;
using System.Linq;
using System.Collections.Generic;

namespace VkMusicQuizBot
{
    public class QuizProcess
    {
        public DateTime ExpiredAt { get; set; }
        public byte[] QuestionBody { get; set; }
        public IEnumerable<QuizOption> Options { get; set; }
        public IReadOnlyList<QuizAnswer> Answers { get => answers.AsReadOnly(); }

        private List<QuizAnswer> answers = new List<QuizAnswer>();
        public void AddAnswer(QuizAnswer answer)
        {
            if (ExpiredAt != null && DateTime.Now > ExpiredAt)
                throw new ExpiredOptionException("Quiz is expired.");
            if (answer?.Option?.Title == null)
                throw new ArgumentNullException(nameof(answer));
            if (answers.Any(answ => answ.Owner == answer.Owner))
                throw new ArgumentException("Answer already has been added.");
            if (Options.All(opt => opt.Title != answer.Option.Title))
                throw new UnexpectedOptionException("Undefined option.");

            answers.Add(answer);
        }
    }
    public class QuizAnswer
    {
        public int Owner { get; set; }
        public QuizOption Option { get; set; }
    }
    public class QuizOption
    {
        public string Title { get; set; }
        public bool IsRight { get; set; }
    }
}
