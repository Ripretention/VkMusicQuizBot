using System;
using System.Linq;
using YoutubeExplode;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;

namespace VkMusicQuizBot
{
    public class AudioTrackDownloader
    {
        private YoutubeClient youtube = new YoutubeClient();
        private ProcessStartInfo ffmpegInfo;
        public AudioTrackDownloader(FFMpegConfiguration cfg)
        {
            ffmpegInfo = new ProcessStartInfo(cfg.Path);
            ffmpegInfo.RedirectStandardInput = true;
            ffmpegInfo.RedirectStandardOutput = true;
            ffmpegInfo.UseShellExecute = false;
        }

        public async Task<byte[]> Download(AudioTrack track)
        {
            var trackUrl = await searchTrackUrl(track);
            var trackDownloadUrl = trackUrl != null 
                ? await getTrackDownloadUrl(trackUrl) 
                : null;

            return trackDownloadUrl != null
                ? await getBuffer(trackDownloadUrl)
                : null;
        }

        private async Task<byte[]> getBuffer(string url)
        {
            var chunks = new List<byte[]>();

            prepareFFmpeg(url);
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

            return chunks.SelectMany(c => c).ToArray();
        }
        private void prepareFFmpeg(string url)
        {
            ffmpegInfo.Arguments = $"-loglevel panic -i {url} -ss 00:01:00 -t 10 -vn -f ogg pipe:1";
        }

        private async Task<string> getTrackDownloadUrl(string trackUrl)
        {
            var youtube = new YoutubeClient();
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(trackUrl);

            return streamManifest?.GetAudioOnlyStreams().OrderBy(a => a.Bitrate).First().Url;
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

            return trackUrl;
        }
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
