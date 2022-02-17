using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace VkMusicQuizBot
{
    public class QuizSession
    {
        private QuizProcess process;
        private IAudioTrackDownloader downloader;
        public TimeSpan Duration = TimeSpan.FromSeconds(30);
        public QuizSession(IAudioTrackDownloader downloader)
        {
            this.downloader = downloader ?? throw new ArgumentNullException(nameof(downloader));
        }
        
        public async Task<QuizSession> Create(IEnumerable<AudioTrack> tracks, long creatorId)
        {
            if (tracks == null || tracks.Count() == 0)
                throw new ArgumentException("Tracks can't be empty.");
            if (process != null)
                throw new InvalidOperationException("QuizProcess already created");

            var pickedTracks = pickRandomTracks(tracks);
            var questionTrackBody = await downloader.Download(pickedTracks.First());

            process = new QuizProcess
            {
                CreatorId = creatorId,
                QuestionBody = questionTrackBody,
                Options = pickedTracks.Select(track => new QuizOption
                {
                    Title = track.Title,
                    IsRight = track.Title == pickedTracks.First().Title
                })
            };
            return this;
        }
        public PendingQuiz Start()
        {
            if (process == null)
                throw new InvalidOperationException("QuizProcess must be created");
            process.ExpiredAt = DateTime.Now + Duration;
            return new PendingQuiz(process, Duration);
        }
        private IEnumerable<AudioTrack> pickRandomTracks(IEnumerable<AudioTrack> tracks)
        {
            if (tracks.Count() <= 2)
                return tracks;

            var rnd = new Random();
            var pickedTracks = new List<AudioTrack>();
            for (int randomIndex = rnd.Next(0, tracks.Count()); pickedTracks.Count < 3; randomIndex = rnd.Next(0, tracks.Count()))
                if (pickedTracks.All(track => track.Title != tracks.ElementAt(randomIndex).Title))
                    pickedTracks.Add(tracks.ElementAt(randomIndex));

            return pickedTracks;
        }
    }
}
