using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace VkMusicQuizBot
{
    public class SpotifyAudioTrackExtractor : IAudioTrackExtractor
    {
        private string playlistId;
        private ISpotifyClient spotify;
        private IEnumerable<AudioTrack> extractedAudioTracks;
        public SpotifyAudioTrackExtractor(ISpotifyClient spotify, string playlistId)
        {
            this.spotify = spotify;
            this.playlistId = playlistId;
        }

        public Task<IEnumerable<AudioTrack>> Extract() =>
            extractedAudioTracks == null
                ? Refresh()
                : Task.FromResult(extractedAudioTracks);
        public async Task<IEnumerable<AudioTrack>> Refresh()
        {
            var playlist = await spotify.GetPlaylistTracks(playlistId, 100);
            List<AudioTrack> tracks = new List<AudioTrack>();

            foreach (var track in playlist)
                tracks.Add(new AudioTrack
                {
                    Name = track.Name,
                    Duration = track.Duration,
                    Artist = track.Artists?.First()?.Name
                });

            extractedAudioTracks = tracks;
            return extractedAudioTracks;
        }
    }

    public interface IAudioTrackExtractor
    {
        public Task<IEnumerable<AudioTrack>> Extract();
        public Task<IEnumerable<AudioTrack>> Refresh();
    }
}
