using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Requestrr.WebApi.RequestrrBot.Movies;

namespace Requestrr.WebApi.RequestrrBot.DownloadClients.Radarr
{
    public class RadarrClient : IMovieRequester, IMovieSearcher
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<RadarrClient> _logger;
        private RadarrSettingsProvider _RadarrSettingsProvider;
        private readonly MoviesSettingsProvider _moviesSettingsProvider;
        private MoviesSettings MoviesSettings => _moviesSettingsProvider.Provide();
        private IDictionary<int, RadarrDownloadClientSettings> RadarrSettings => _RadarrSettingsProvider.Provide();

        public RadarrClient(IHttpClientFactory httpClientFactory, ILogger<RadarrClient> logger, RadarrSettingsProvider RadarrSettingsProvider, MoviesSettingsProvider moviesSettingsProvider)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _RadarrSettingsProvider = RadarrSettingsProvider;
            _moviesSettingsProvider = moviesSettingsProvider;
        }

        public static Task TestConnectionAsync(HttpClient httpClient, ILogger<RadarrClient> logger, RadarrDownloadClientSettings settings)
        {
            if (settings.Version == "2")
            {
                return RadarrClientV2.TestConnectionAsync(httpClient, logger, settings);
            }
            else
            {
                return RadarrClientV3.TestConnectionAsync(httpClient, logger, settings);
            }
        }

        public static Task<IList<JSONRootPath>> GetRootPaths(HttpClient httpClient, ILogger<RadarrClient> logger, RadarrDownloadClientSettings settings)
        {
            if (settings.Version == "2")
            {
                return RadarrClientV2.GetRootPaths(httpClient, logger, settings);
            }
            else
            {
                return RadarrClientV3.GetRootPaths(httpClient, logger, settings);
            }
        }

        public static Task<IList<JSONProfile>> GetProfiles(HttpClient httpClient, ILogger<RadarrClient> logger, RadarrDownloadClientSettings settings)
        {
            if (settings.Version == "2")
            {
                return RadarrClientV2.GetProfiles(httpClient, logger, settings);
            }
            else
            {
                return RadarrClientV3.GetProfiles(httpClient, logger, settings);
            }
        }

        public static Task<IList<JSONTag>> GetTags(HttpClient httpClient, ILogger<RadarrClient> logger, RadarrDownloadClientSettings settings)
        {
            if (settings.Version == "2")
            {
                return Task.FromResult((IList<JSONTag>)new List<JSONTag>());
            }
            else
            {
                return RadarrClientV3.GetTags(httpClient, logger, settings);
            }
        }

        public Task<Movie> SearchMovieAsync(MovieRequest request, int theMovieDbId)
        {
            return CreateInstance<IMovieSearcher>(request, $"with tmdb id of {theMovieDbId}").SearchMovieAsync(request, theMovieDbId);
        }

        public Task<IReadOnlyList<Movie>> SearchMovieAsync(MovieRequest request, string movieName)
        {
            return CreateInstance<IMovieSearcher>(request, movieName).SearchMovieAsync(request, movieName);
        }

        public Task<MovieDetails> GetMovieDetails(MovieRequest request, string theMovieDbId)
        {
            return CreateInstance<IMovieSearcher>(request, $"with tmdb id of {theMovieDbId}").GetMovieDetails(request, theMovieDbId);
        }

        public Task<Dictionary<int, Movie>> SearchAvailableMoviesAsync(HashSet<int> theMovieDbIds, System.Threading.CancellationToken token)
        {
            // return CreateInstance<IMovieSearcher>(MovieRequest, theMovieDbId).SearchAvailableMoviesAsync(theMovieDbIds, token);
            throw new System.Exception("Not yet bruh");
        }

        public Task<MovieRequestResult> RequestMovieAsync(MovieRequest request, Movie movie)
        {
            return CreateInstance<IMovieRequester>(request, movie.Title).RequestMovieAsync(request, movie);
        }

        private T CreateInstance<T>(MovieRequest request, string movieTitle) where T : class
        {
            var category = GetRadarrCategory(request, movieTitle);
            var settings = GetRadarrClientSettings(category, movieTitle);

            if (settings.Version == "2")
            {
                return new RadarrClientV2(_httpClientFactory, _logger, settings, category) as T;
            }
            else
            {
                return new RadarrClientV3(_httpClientFactory, _logger, settings, category) as T;
            }
        }

        private RadarrDownloadClientSettings GetRadarrClientSettings(MovieRequest request, string movieTitle)
        {
            var category = GetRadarrCategory(request, movieTitle);
            return GetRadarrClientSettings(category, movieTitle);
        }

        private RadarrDownloadClientSettings GetRadarrClientSettings(RadarrCategory category, string movieTitle)
        {
            try
            {
                return RadarrSettings[category.DownloadClientId];
            }
            catch
            {
                _logger.LogError($"An error occurred while requesting movie \"{movieTitle}\" from Radarr, could not find radarr client with id {category.DownloadClientId}");
                throw new System.Exception($"An error occurred while requesting movie \"{movieTitle}\" from Radarr, could not find radarr client with id {category.DownloadClientId}");
            }
        }

        private RadarrCategory GetRadarrCategory(MovieRequest request, string movieTitle)
        {
            try
            {
                return MoviesSettings.Categories.OfType<RadarrCategory>().Single(x => x.Id == request.CategoryId);
            }
            catch
            {
                _logger.LogError($"An error occurred while requesting movie \"{movieTitle}\" from Radarr, could not find category with id {request.CategoryId}");
                throw new System.Exception($"An error occurred while requesting movie \"{movieTitle}\" from Radarr, could not find category with id {request.CategoryId}");
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

        public class JSONTag
        {
            public string label { get; set; }
            public int id { get; set; }
        }
    }
}