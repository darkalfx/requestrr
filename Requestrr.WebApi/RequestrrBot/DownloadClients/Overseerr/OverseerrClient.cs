using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Requestrr.WebApi.RequestrrBot.Movies;
using Requestrr.WebApi.RequestrrBot.TvShows;

namespace Requestrr.WebApi.RequestrrBot.DownloadClients.Overseerr
{
    public class OverseerrClient : IMovieRequester, IMovieSearcher, ITvShowSearcher, ITvShowRequester
    {
        private IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OverseerrClientV1> _logger;
        private readonly OverseerrSettingsProvider _overseerrSettingsProvider;
        private readonly TvShowsSettingsProvider _tvShowsSettingsProvider;
        private readonly MoviesSettingsProvider _moviesSettingsProvider;
        private readonly OverseerrTvShowCategory _tvShowCategory;
        private readonly OverseerrMovieCategory _movieCategory;
        private ConcurrentDictionary<string, int> _requesterIdToOverseerUserID = new ConcurrentDictionary<string, int>();

        public OverseerrClient(IHttpClientFactory httpClientFactory, ILogger<OverseerrClientV1> logger, OverseerrSettingsProvider overseerrSettingsProvider, TvShowsSettingsProvider tvShowsSettingsProvider, MoviesSettingsProvider moviesSettingsProvider)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _overseerrSettingsProvider = overseerrSettingsProvider;
            _tvShowsSettingsProvider = tvShowsSettingsProvider;
            _moviesSettingsProvider = moviesSettingsProvider;
        }

        public static Task TestConnectionAsync(HttpClient httpClient, ILogger<OverseerrClientV1> logger, OverseerrTestSettings settings)
        {
            return OverseerrClientV1.TestConnectionAsync(httpClient, logger, settings);
        }

        public static Task<RadarrServiceSettings> GetRadarrServiceSettingsAsync(HttpClient httpClient, ILogger<OverseerrClientV1> logger, OverseerrTestSettings settings)
        {
            return OverseerrClientV1.GetRadarrServiceSettingsAsync(httpClient, logger, settings);
        }

        public static Task<SonarrServiceSettings> GetSonarrServiceSettingsAsync(HttpClient httpClient, ILogger<OverseerrClientV1> logger, OverseerrTestSettings settings)
        {
            return OverseerrClientV1.GetSonarrServiceSettingsAsync(httpClient, logger, settings);
        }

        async Task<Dictionary<int, Movie>> IMovieSearcher.SearchAvailableMoviesAsync(HashSet<int> movieIds, CancellationToken token)
        {
            throw new NotImplementedException("Not yet bruh");
        }

        async Task<IReadOnlyList<TvShow>> ITvShowSearcher.GetTvShowDetailsAsync(HashSet<int> tvShowIds, CancellationToken token)
        {
            throw new NotImplementedException("Not yet bruh");
        }

        public Task<IReadOnlyList<Movie>> SearchMovieAsync(MovieRequest request, string movieName)
        {
            return CreateMovieInstance<IMovieSearcher>(request, movieName).SearchMovieAsync(request, movieName);
        }

        public Task<Movie> SearchMovieAsync(MovieRequest request, int theMovieDbId)
        {
            return CreateMovieInstance<IMovieSearcher>(request, $"with tmdb id of {theMovieDbId}").SearchMovieAsync(request, theMovieDbId);
        }

        Task<MovieRequestResult> IMovieRequester.RequestMovieAsync(MovieRequest request, Movie movie)
        {
            return CreateMovieInstance<IMovieRequester>(request, movie.Title).RequestMovieAsync(request, movie);
        }

        public Task<MovieDetails> GetMovieDetails(MovieRequest request, string theMovieDbId)
        {
            return CreateMovieInstance<IMovieSearcher>(request, $"with tmdb id of {theMovieDbId}").GetMovieDetails(request, theMovieDbId);
        }

        Task<IReadOnlyList<SearchedTvShow>> ITvShowSearcher.SearchTvShowAsync(TvShowRequest request, string tvShowName)
        {
            return CreateTvShowInstance<ITvShowSearcher>(request, tvShowName).SearchTvShowAsync(request, tvShowName);
        }

        public Task<TvShow> GetTvShowDetailsAsync(TvShowRequest request, int theTvDbId)
        {
            return CreateTvShowInstance<ITvShowSearcher>(request, $"with tvdb id of {theTvDbId}").GetTvShowDetailsAsync(request, theTvDbId);
        }

        Task<TvShowRequestResult> ITvShowRequester.RequestTvShowAsync(TvShowRequest request, TvShow tvShow, TvSeason season)
        {
            return CreateTvShowInstance<ITvShowRequester>(request, tvShow.Title).RequestTvShowAsync(request, tvShow, season);
        }

        Task<SearchedTvShow> ITvShowSearcher.SearchTvShowAsync(TvShowRequest request, int tvDbId)
        {
            return CreateTvShowInstance<ITvShowSearcher>(request, $"with tvdb id of {tvDbId}").SearchTvShowAsync(request, tvDbId);
        }

        private T CreateTvShowInstance<T>(TvShowRequest request, string tvShowTitle) where T : class
        {
            var category = GetOverseerrCategory(request, tvShowTitle);
            var settings = GetOverseerrClientSettings(category.DownloadClientId, tvShowTitle);

            return new OverseerrClientV1(_httpClientFactory, _logger, settings, category, null) as T;
        }

        private T CreateMovieInstance<T>(MovieRequest request, string movieTitle) where T : class
        {
            var category = GetOverseerrCategory(request, movieTitle);
            var settings = GetOverseerrClientSettings(category.DownloadClientId, movieTitle);

            return new OverseerrClientV1(_httpClientFactory, _logger, settings, null, category) as T;
        }

        private OverseerrDownloadClientSettings GetOverseerrClientSettings(int downloadClientId, string tvShowTitle)
        {
            try
            {
                return _overseerrSettingsProvider.Provide()[downloadClientId];
            }
            catch
            {
                _logger.LogError($"An error occurred while requesting tv show \"{tvShowTitle}\" from Overseerr, could not find radarr client with id {downloadClientId}");
                throw new System.Exception($"An error occurred while requesting tv show \"{tvShowTitle}\" from Overseerr, could not find radarr client with id {downloadClientId}");
            }
        }

        private OverseerrMovieCategory GetOverseerrCategory(MovieRequest request, string movieTitle)
        {
            try
            {
                return _tvShowsSettingsProvider.Provide().Categories.OfType<OverseerrMovieCategory>().Single(x => x.Id == request.CategoryId);
            }
            catch
            {
                _logger.LogError($"An error occurred while requesting movie \"{movieTitle}\" from Overseerr, could not find category with id {request.CategoryId}");
                throw new Exception($"An error occurred while requesting movie \"{movieTitle}\" from Overseerr, could not find category with id {request.CategoryId}");
            }
        }

        private OverseerrTvShowCategory GetOverseerrCategory(TvShowRequest request, string tvShowTitle)
        {
            try
            {
                return _tvShowsSettingsProvider.Provide().Categories.OfType<OverseerrTvShowCategory>().Single(x => x.Id == request.CategoryId);
            }
            catch
            {
                _logger.LogError($"An error occurred while requesting a tv show \"{tvShowTitle}\" from Overseerr, could not find category with id {request.CategoryId}");
                throw new Exception($"An error occurred while requesting a tv show \"{tvShowTitle}\" from Overseerr, could not find category with id {request.CategoryId}");
            }
        }
    }
}