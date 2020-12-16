using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Requestrr.WebApi.RequestrrBot.Music;

namespace Requestrr.WebApi.RequestrrBot.DownloadClients.Lidarr
{
    public class LidarrClient : IArtistSearcher, IArtistRequester
    {
        private IHttpClientFactory _httpClientFactory;
        private readonly ILogger<LidarrClient> _logger;
        private LidarrSettingsProvider _settingsProvider;

        public LidarrClient(IHttpClientFactory httpClientFactory, ILogger<LidarrClient> logger, LidarrSettingsProvider sonarrSettingsProvider)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _settingsProvider = sonarrSettingsProvider;
        }

        public static Task TestConnectionAsync(HttpClient httpClient, ILogger<LidarrClient> logger, LidarrSettings settings)
        {
            return LidarrClientV1.TestConnectionAsync(httpClient, logger, settings);
        }

        public static Task<IList<JSONRootPath>> GetRootPaths(HttpClient httpClient, ILogger<LidarrClient> logger, LidarrSettings settings)
        {
            return LidarrClientV1.GetRootPaths(httpClient, logger, settings);
        }

        public static Task<IList<JSONProfile>> GetProfiles(HttpClient httpClient, ILogger<LidarrClient> logger, LidarrSettings settings)
        {
            return LidarrClientV1.GetProfiles(httpClient, logger, settings);
        }

        public static Task<IList<JSONMetadataProfile>> GetMetadataProfiles(HttpClient httpClient, ILogger<LidarrClient> logger, LidarrSettings settings)
        {
            return LidarrClientV1.GetMetadataProfiles(httpClient, logger, settings);
        }

        public static Task<IList<JSONTag>> GetTags(HttpClient httpClient, ILogger<LidarrClient> logger, LidarrSettings settings)
        {
            return LidarrClientV1.GetTags(httpClient, logger, settings);
        }

        public Task<Artist> SearchArtistByIdAsync(string mbId)
        {
            return CreateInstance<IArtistSearcher>().SearchArtistByIdAsync(mbId);
        }

        public Task<IReadOnlyList<Artist>> SearchArtistAsync(string artistName)
        {
            return CreateInstance<IArtistSearcher>().SearchArtistAsync(artistName);
        }

        public Task<Dictionary<string, Artist>> SearchAvailableArtistsAsync(HashSet<string> mbIds, System.Threading.CancellationToken token)
        {
            return CreateInstance<IArtistSearcher>().SearchAvailableArtistsAsync(mbIds, token);
        }

        public Task<ArtistRequestResult> RequestArtistAsync(MusicUserRequester requester, Artist artist)
        {
            return CreateInstance<IArtistRequester>().RequestArtistAsync(requester, artist);
        }

        private T CreateInstance<T>() where T : class
        {
            return new LidarrClientV1(_httpClientFactory, _logger, _settingsProvider) as T;
        }

        public class JSONRootPath
        {
            public string path { get; set; }
            public int id { get; set; }
        }

        public class JSONProfile
        {
            public string name { get; set; }
            public int id { get; set; }
        }

        public class JSONMetadataProfile
        {
            public string name { get; set; }
            public int id { get; set; }
        }

        public class JSONTag
        {
            public string label { get; set; }
            public int id { get; set; }
        }
    }
}