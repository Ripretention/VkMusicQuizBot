using System;
using System.Web;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace VkMusicQuizBot
{
    public class SpotifyClient : ISpotifyClient
    {
        public readonly ISpotifyAPI Api;
        public SpotifyClient(SpotifyConfiguration cfg)
        {
            Api = new SpotifyAPI(cfg);
        }
        public SpotifyClient(ISpotifyAPI api)
        {
            Api = api ?? throw new ArgumentNullException(nameof(api));
        }

        public async Task<IEnumerable<SpotifyPlaylist>> GetCurrentPlaylists(uint limit = 10, uint offset = 0)
        {
            var response = await Api.Get<SpotifyPlaylistCollection>(@"me/playlists", new Dictionary<string, string>
            {
                { "limit", limit.ToString() },
                { "offset", offset.ToString() }
            });

            return response.Items;
        }
        public Task<SpotifyPlaylist> GetPlaylist(string id, string fields = null, string market = null)
        {
            var @params = new Dictionary<string, string>();
            if (fields != null)
                @params.Add("fields", fields);
            if (market != null)
                @params.Add("market", market);

            return Api.Get<SpotifyPlaylist>($@"playlists/{id}", @params);
        }
        public async Task<IEnumerable<SpotifyTrack>> GetPlaylistTracks(string id, uint limit = 10, uint offset = 0) =>
            (await GetPlaylistItems(id, limit, offset)).Select(item => item.Track);

        public async Task<IEnumerable<SpotifyTrackCollectionItem>> GetPlaylistItems(string id, uint limit = 10, uint offset = 0)
        {
            var response = await Api.Get<SpotifyTrackCollection>($@"playlists/{id}/tracks", new Dictionary<string, string>
            {
                { "limit", limit.ToString() },
                { "offset", offset.ToString() }
            });

            return response.Items;
        }
    }

    public interface ISpotifyClient
    {
        public Task<IEnumerable<SpotifyPlaylist>> GetCurrentPlaylists(uint limit = 10, uint offset = 0);
        public Task<SpotifyPlaylist> GetPlaylist(string id, string fields = null, string market = null);
        public Task<IEnumerable<SpotifyTrack>> GetPlaylistTracks(string id, uint limit = 10, uint offset = 0);
        public Task<IEnumerable<SpotifyTrackCollectionItem>> GetPlaylistItems(string id, uint limit = 10, uint offset = 0);
    }
}
