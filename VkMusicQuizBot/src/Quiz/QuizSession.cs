using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace VkMusicQuizBot
{
    public class QuizSession
    {
        private IAudioTrackDownloader downloader;
        public QuizSession(IAudioTrackDownloader downloader)
        {
            this.downloader = downloader ?? throw new ArgumentNullException(nameof(downloader));
        }
        
        public async Task<QuizProcess> Start(IEnumerable<AudioTrack> tracks, TimeSpan? duration = null)
        {
            if (tracks == null || tracks.Count() == 0)
                throw new ArgumentException("Tracks can't be empty.");

            var pickedTracks = pickRandomTracks(tracks);
            var questionTrackBody = await downloader.Download(pickedTracks.First());

            return new QuizProcess
            {
                QuestionBody = questionTrackBody,
                Options = pickedTracks.Select(track => new QuizOption
                {
                    Title = track.Title,
                    IsRight = track.Title == pickedTracks.First().Title
                }),
                ExpiredAt = DateTime.Now + (duration ?? new TimeSpan(0, 0, 30)),
            };
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
