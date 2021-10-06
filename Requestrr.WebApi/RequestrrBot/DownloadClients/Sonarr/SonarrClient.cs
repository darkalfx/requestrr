using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Requestrr.WebApi.RequestrrBot.TvShows;

namespace Requestrr.WebApi.RequestrrBot.DownloadClients.Sonarr
{
    public class SonarrClient : ITvShowSearcher, ITvShowRequester
    {
        private IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SonarrClient> _logger;
        private SonarrSettingsProvider _settingsProvider;

        public SonarrClient(IHttpClientFactory httpClientFactory, ILogger<SonarrClient> logger, SonarrSettingsProvider sonarrSettingsProvider)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _settingsProvider = sonarrSettingsProvider;
        }

        public static Task TestConnectionAsync(HttpClient httpClient, ILogger<SonarrClient> logger, SonarrSettings settings)
        {
            if(settings.Version == "2")
            {
                return SonarrClientV2.TestConnectionAsync(httpClient, logger, settings);
            }
            else 
            {
                return SonarrClientV3.TestConnectionAsync(httpClient, logger, settings);
            }
        }

        public static Task<IList<JSONRootPath>> GetRootPaths(HttpClient httpClient, ILogger<SonarrClient> logger, SonarrSettings settings)
        {
            if(settings.Version == "2")
            {
                return SonarrClientV2.GetRootPaths(httpClient, logger, settings);
            }
            else 
            {
                return SonarrClientV3.GetRootPaths(httpClient, logger, settings);
            }
        }

        public static Task<IList<JSONProfile>> GetProfiles(HttpClient httpClient, ILogger<SonarrClient> logger, SonarrSettings settings)
        {
            if(settings.Version == "2")
            {
                return SonarrClientV2.GetProfiles(httpClient, logger, settings);
            }
            else 
            {
                return SonarrClientV3.GetProfiles(httpClient, logger, settings);
            }
        }

        public static Task<IList<JSONLanguageProfile>> GetLanguages(HttpClient httpClient, ILogger<SonarrClient> logger, SonarrSettings settings)
        {
            if(settings.Version == "2")
            {
                return Task.FromResult((IList<JSONLanguageProfile>)new List<JSONLanguageProfile>());
            }
            else 
            {
                return SonarrClientV3.GetLanguages(httpClient, logger, settings);
            }
        }

        public static Task<IList<JSONTag>> GetTags(HttpClient httpClient, ILogger<SonarrClient> logger, SonarrSettings settings)
        {
            if(settings.Version == "2")
            {
                return Task.FromResult((IList<JSONTag>)new List<JSONTag>());
            }
            else 
            {
                return SonarrClientV3.GetTags(httpClient, logger, settings);
            }
        }

        public Task<SearchedTvShow> SearchTvShowAsync(int tvDbId)
        {
            return CreateInstance<ITvShowSearcher>().SearchTvShowAsync(tvDbId);
        }

        public Task<IReadOnlyList<SearchedTvShow>> SearchTvShowAsync(string tvShowName)
        {
            return CreateInstance<ITvShowSearcher>().SearchTvShowAsync(tvShowName);
        }

        public Task<TvShow> GetTvShowDetailsAsync(int theTvDbId)
        {
            return CreateInstance<ITvShowSearcher>().GetTvShowDetailsAsync(theTvDbId);
        }

        public Task<IReadOnlyList<TvShow>> GetTvShowDetailsAsync(HashSet<int> theTvDbIds, CancellationToken token)
        {
            return CreateInstance<ITvShowSearcher>().GetTvShowDetailsAsync(theTvDbIds, token);
        }

        public Task<TvShowRequestResult> RequestTvShowAsync(TvShowRequest request, TvShow tvShow, TvSeason season)
        {
            return CreateInstance<ITvShowRequester>().RequestTvShowAsync(request, tvShow, season);
        }

        private T CreateInstance<T>() where T : class
        {
            if (_settingsProvider.Provide().Version == "2")
            {
                return new SonarrClientV2(_httpClientFactory, _logger, _settingsProvider) as T;
            }
            else
            {
                return new SonarrClientV3(_httpClientFactory, _logger, _settingsProvider) as T;
            }
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

        public class JSONLanguageProfile
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