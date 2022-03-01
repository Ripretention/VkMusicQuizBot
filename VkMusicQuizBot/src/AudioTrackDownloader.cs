using System;
using System.Linq;
using YoutubeExplode;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace VkMusicQuizBot
{
    public class AudioTrackDownloader : IAudioTrackDownloader
    {
        private ProcessStartInfo ffmpegInfo;
        private ILogger<AudioTrackDownloader> logger;
        private YoutubeClient youtube = new YoutubeClient();
        private string outputFormat;
        public AudioTrackDownloader(FFMpegConfiguration cfg, string outputFormat = "ogg", ILogger<AudioTrackDownloader> logger = null)
        {
            ffmpegInfo = new ProcessStartInfo(cfg.Path);
            ffmpegInfo.UseShellExecute = false;
            ffmpegInfo.RedirectStandardInput = true;
            ffmpegInfo.RedirectStandardOutput = true;

            this.logger = logger;
            this.outputFormat = outputFormat;
        }

        public async Task<byte[]> Download(AudioTrack track, TimeSpan start, TimeSpan? duration = null)
        {
            var trackUrl = await searchTrackUrl(track);
            var trackDownloadUrl = trackUrl != null 
                ? await getTrackDownloadUrl(trackUrl) 
                : null;

            return trackDownloadUrl != null
                ? await getBuffer(trackDownloadUrl, start, duration)
                : null;
        }

        private async Task<byte[]> getBuffer(string url, TimeSpan start, TimeSpan? duration = null)
        {
            var chunks = new List<byte[]>();

            prepareFFmpeg(url, start, duration);
            var ffmpegProcess = new Process();
            ffmpegProcess.StartInfo = ffmpegInfo;
            ffmpegProcess.Start();
            while (true)
            {
                var bytes = new byte[1024];
                var result = await ffmpegProcess.StandardOutput.BaseStream.ReadAsync(bytes);
                if (result == 0)
                    break;
                chunks.Add(bytes);
            }

            logger?.LogDebug($"Recieved {chunks.Count} chunks by [{url.Take(20)}...]");
            return chunks.SelectMany(c => c).ToArray();
        }
        private void prepareFFmpeg(string url, TimeSpan start, TimeSpan? duration)
        {
            ffmpegInfo.Arguments = $"-loglevel panic -i {url} -ss {start} -t {duration ?? TimeSpan.FromSeconds(10)} -vn -f {outputFormat} pipe:1";
        }

        private async Task<string> getTrackDownloadUrl(string trackUrl)
        {
            var youtube = new YoutubeClient();
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(trackUrl);

            return streamManifest == null 
                ? null
                : streamManifest.GetAudioStreams().OrderBy(a => a.Bitrate).First().Url;
        }

        private async Task<string> searchTrackUrl(AudioTrack track)
        {
            var searchResults = youtube.Search.GetVideosAsync(track.Title);
            string trackUrl = null;
            await foreach (var result in searchResults)
            {
                if (result.Duration != track.Duration && result.Title != track.Title && result.Title != track.Name) continue;
                trackUrl = result.Url;
                break;
            }

            logger?.LogDebug($"SourceUrl for [{track.Title}] has{(trackUrl == null ? "n't" : "")} founded");
            return trackUrl;
        }
    }
    public interface IAudioTrackDownloader
    {
        public Task<byte[]> Download(AudioTrack track, TimeSpan start, TimeSpan? duration = null);
    }
    public class AudioTrack
    {
        public string Name;
        public string Artist;
        public TimeSpan Duration;
        public string Title
        {
            get => $"{Artist} - {Name}";
        }
    }
}
