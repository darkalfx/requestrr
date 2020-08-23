using System.Collections.Generic;
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
        private readonly RadarrSettingsProvider _settingsProvider;

        public RadarrClient(IHttpClientFactory httpClientFactory, ILogger<RadarrClient> logger, RadarrSettingsProvider RadarrSettingsProvider)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _settingsProvider = RadarrSettingsProvider;
        }
        
        public static Task TestConnectionAsync(HttpClient httpClient, ILogger<RadarrClient> logger, RadarrSettings settings)
        {
            if(settings.Version == "2")
            {
                return RadarrClientV2.TestConnectionAsync(httpClient, logger, settings);
            }
            else 
            {
                return RadarrClientV3.TestConnectionAsync(httpClient, logger, settings);
            }
        }

        public static Task<IList<JSONRootPath>> GetRootPaths(HttpClient httpClient, ILogger<RadarrClient> logger, RadarrSettings settings)
        {
            if(settings.Version == "2")
            {
                return RadarrClientV2.GetRootPaths(httpClient, logger, settings);
            }
            else 
            {
                return RadarrClientV3.GetRootPaths(httpClient, logger, settings);
            }
        }

        public static Task<IList<JSONProfile>> GetProfiles(HttpClient httpClient, ILogger<RadarrClient> logger, RadarrSettings settings)
        {
            if(settings.Version == "2")
            {
                return RadarrClientV2.GetProfiles(httpClient, logger, settings);
            }
            else 
            {
                return RadarrClientV3.GetProfiles(httpClient, logger, settings);
            }
        }

        public static Task<IList<JSONTag>> GetTags(HttpClient httpClient, ILogger<RadarrClient> logger, RadarrSettings settings)
        {
            if(settings.Version == "2")
            {
                return Task.FromResult((IList<JSONTag>)new List<JSONTag>());
            }
            else 
            {
                return RadarrClientV3.GetTags(httpClient, logger, settings);
            }
        }

        public Task<Movie> SearchMovieAsync(int theMovieDbId)
        {
            return CreateInstance<IMovieSearcher>().SearchMovieAsync(theMovieDbId);
        }

        public Task<IReadOnlyList<Movie>> SearchMovieAsync(string movieName)
        {
            return CreateInstance<IMovieSearcher>().SearchMovieAsync(movieName);
        }

        public Task<MovieDetails> GetMovieDetails(string theMovieDbId)
        {
            return CreateInstance<IMovieSearcher>().GetMovieDetails(theMovieDbId);
        }

        public Task<Dictionary<int, Movie>> SearchAvailableMoviesAsync(HashSet<int> theMovieDbIds, System.Threading.CancellationToken token)
        {
            return CreateInstance<IMovieSearcher>().SearchAvailableMoviesAsync(theMovieDbIds, token);
        }

        public Task<MovieRequestResult> RequestMovieAsync(MovieUserRequester requester, Movie movie)
        {
            return CreateInstance<IMovieRequester>().RequestMovieAsync(requester, movie);
        }

        private T CreateInstance<T>() where T : class
        {
            if(_settingsProvider.Provide().Version == "2")
            {
                return new RadarrClientV2(_httpClientFactory, _logger, _settingsProvider) as T;
            }
            else 
            {
                return new RadarrClientV3(_httpClientFactory, _logger, _settingsProvider) as T;
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