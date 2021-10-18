using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly TvShowsSettingsProvider _tvShowsSettingsProvider;

        public SonarrClient(IHttpClientFactory httpClientFactory, ILogger<SonarrClient> logger, SonarrSettingsProvider sonarrSettingsProvider, TvShowsSettingsProvider tvShowsSettingsProvider)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _settingsProvider = sonarrSettingsProvider;
            _tvShowsSettingsProvider = tvShowsSettingsProvider;
        }

        public static Task TestConnectionAsync(HttpClient httpClient, ILogger<SonarrClient> logger, SonarrDownloadClientSettings settings)
        {
            if (settings.Version == "2")
            {
                return SonarrClientV2.TestConnectionAsync(httpClient, logger, settings);
            }
            else
            {
                return SonarrClientV3.TestConnectionAsync(httpClient, logger, settings);
            }
        }

        public static Task<IList<JSONRootPath>> GetRootPaths(HttpClient httpClient, ILogger<SonarrClient> logger, SonarrDownloadClientSettings settings)
        {
            if (settings.Version == "2")
            {
                return SonarrClientV2.GetRootPaths(httpClient, logger, settings);
            }
            else
            {
                return SonarrClientV3.GetRootPaths(httpClient, logger, settings);
            }
        }

        public static Task<IList<JSONProfile>> GetProfiles(HttpClient httpClient, ILogger<SonarrClient> logger, SonarrDownloadClientSettings settings)
        {
            if (settings.Version == "2")
            {
                return SonarrClientV2.GetProfiles(httpClient, logger, settings);
            }
            else
            {
                return SonarrClientV3.GetProfiles(httpClient, logger, settings);
            }
        }

        public static Task<IList<JSONLanguageProfile>> GetLanguages(HttpClient httpClient, ILogger<SonarrClient> logger, SonarrDownloadClientSettings settings)
        {
            if (settings.Version == "2")
            {
                return Task.FromResult((IList<JSONLanguageProfile>)new List<JSONLanguageProfile>());
            }
            else
            {
                return SonarrClientV3.GetLanguages(httpClient, logger, settings);
            }
        }

        public static Task<IList<JSONTag>> GetTags(HttpClient httpClient, ILogger<SonarrClient> logger, SonarrDownloadClientSettings settings)
        {
            if (settings.Version == "2")
            {
                return Task.FromResult((IList<JSONTag>)new List<JSONTag>());
            }
            else
            {
                return SonarrClientV3.GetTags(httpClient, logger, settings);
            }
        }

        public Task<SearchedTvShow> SearchTvShowAsync(TvShowRequest request, int tvDbId)
        {
            return CreateInstance<ITvShowSearcher>(request, $"with tvdb id of {tvDbId}").SearchTvShowAsync(request, tvDbId);
        }

        public Task<IReadOnlyList<SearchedTvShow>> SearchTvShowAsync(TvShowRequest request, string tvShowName)
        {
            return CreateInstance<ITvShowSearcher>(request,tvShowName).SearchTvShowAsync(request, tvShowName);
        }

        public Task<TvShow> GetTvShowDetailsAsync(TvShowRequest request, int theTvDbId)
        {
            return CreateInstance<ITvShowSearcher>(request, $"with tvdb id of {theTvDbId}").GetTvShowDetailsAsync(request, theTvDbId);
        }

        public Task<IReadOnlyList<TvShow>> GetTvShowDetailsAsync(HashSet<int> theTvDbIds, CancellationToken token)
        {
            // return CreateInstance<ITvShowSearcher>(request, tvDbId).GetTvShowDetailsAsync(theTvDbIds, token);
            throw new NotImplementedException("Not yet bruh.");
        }

        public Task<TvShowRequestResult> RequestTvShowAsync(TvShowRequest request, TvShow tvShow, TvSeason season)
        {
            return CreateInstance<ITvShowRequester>(request, tvShow.Title).RequestTvShowAsync(request, tvShow, season);
        }

        private T CreateInstance<T>(TvShowRequest request, string tvShowTitle) where T : class
        {
            var category = GetSonarrCategory(request, tvShowTitle);
            var settings = GetSonarrClientSettings(category, tvShowTitle);

            if (settings.Version == "2")
            {
                return new SonarrClientV2(_httpClientFactory, _logger, settings, category) as T;
            }
            else
            {
                return new SonarrClientV3(_httpClientFactory, _logger, settings, category) as T;
            }
        }

        private SonarrDownloadClientSettings GetSonarrClientSettings(TvShowRequest request, string tvShowTitle)
        {
            var category = GetSonarrCategory(request, tvShowTitle);
            return GetSonarrClientSettings(category, tvShowTitle);
        }

        private SonarrDownloadClientSettings GetSonarrClientSettings(SonarrCategory category, string tvShowTitle)
        {
            try
            {
                return _settingsProvider.Provide()[category.DownloadClientId];
            }
            catch
            {
                _logger.LogError($"An error occurred while requesting tv show \"{tvShowTitle}\" from Sonarr, could not find radarr client with id {category.DownloadClientId}");
                throw new System.Exception($"An error occurred while requesting tv show \"{tvShowTitle}\" from Sonarr, could not find radarr client with id {category.DownloadClientId}");
            }
        }

        private SonarrCategory GetSonarrCategory(TvShowRequest request, string tvShowTitle)
        {
            try
            {
                return _tvShowsSettingsProvider.Provide().Categories.OfType<SonarrCategory>().Single(x => x.Id == request.CategoryId);
            }
            catch
            {
                _logger.LogError($"An error occurred while requesting tv show \"{tvShowTitle}\" from Sonarr, could not find category with id {request.CategoryId}");
                throw new System.Exception($"An error occurred while requesting tv show \"{tvShowTitle}\" from Sonarr, could not find category with id {request.CategoryId}");
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