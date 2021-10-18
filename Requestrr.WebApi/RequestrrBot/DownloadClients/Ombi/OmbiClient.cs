using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Requestrr.WebApi.RequestrrBot.Movies;
using Requestrr.WebApi.RequestrrBot.TvShows;

namespace Requestrr.WebApi.RequestrrBot.DownloadClients.Ombi
{
    public class OmbiClient : IMovieRequester, IMovieSearcher, ITvShowSearcher, ITvShowRequester
    {
        private IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OmbiClient> _logger;
        private OmbiSettingsProvider _ombiSettingsProvider;
        private readonly TvShowsSettingsProvider _tvShowsSettingsProvider;
        private readonly MoviesSettingsProvider _moviesSettingsProvider;

        public OmbiClient(IHttpClientFactory httpClientFactory, ILogger<OmbiClient> logger, OmbiSettingsProvider OmbiSettingsProvider, TvShowsSettingsProvider tvShowsSettingsProvider, MoviesSettingsProvider moviesSettingsProvider)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _ombiSettingsProvider = OmbiSettingsProvider;
            _tvShowsSettingsProvider = tvShowsSettingsProvider;
            _moviesSettingsProvider = moviesSettingsProvider;
        }

        public static Task TestConnectionAsync(HttpClient httpClient, ILogger<OmbiClient> logger, OmbiDownloadClientSettings settings)
        {
            return OmbiClientV4.TestConnectionAsync(httpClient, logger, settings);
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
            var category = GetOmbiCategory(request, tvShowTitle);
            var settings = GetOmbiClientSettings(category.DownloadClientId, tvShowTitle);

            return new OmbiClientV4(_httpClientFactory, _logger, settings, category, null) as T;
        }

        private T CreateMovieInstance<T>(MovieRequest request, string movieTitle) where T : class
        {
            var category = GetOmbiCategory(request, movieTitle);
            var settings = GetOmbiClientSettings(category.DownloadClientId, movieTitle);

            return new OmbiClientV4(_httpClientFactory, _logger, settings, null, category) as T;
        }

        private OmbiDownloadClientSettings GetOmbiClientSettings(int downloadClientId, string tvShowTitle)
        {
            try
            {
                return _ombiSettingsProvider.Provide()[downloadClientId];
            }
            catch
            {
                _logger.LogError($"An error occurred while requesting tv show \"{tvShowTitle}\" from Ombi, could not find radarr client with id {downloadClientId}");
                throw new System.Exception($"An error occurred while requesting tv show \"{tvShowTitle}\" from Ombi, could not find radarr client with id {downloadClientId}");
            }
        }

        private OmbiMovieCategory GetOmbiCategory(MovieRequest request, string movieTitle)
        {
            try
            {
                return _tvShowsSettingsProvider.Provide().Categories.OfType<OmbiMovieCategory>().Single(x => x.Id == request.CategoryId);
            }
            catch
            {
                _logger.LogError($"An error occurred while requesting movie \"{movieTitle}\" from Ombi, could not find category with id {request.CategoryId}");
                throw new Exception($"An error occurred while requesting movie \"{movieTitle}\" from Ombi, could not find category with id {request.CategoryId}");
            }
        }

        private OmbiTvShowCategory GetOmbiCategory(TvShowRequest request, string tvShowTitle)
        {
            try
            {
                return _tvShowsSettingsProvider.Provide().Categories.OfType<OmbiTvShowCategory>().Single(x => x.Id == request.CategoryId);
            }
            catch
            {
                _logger.LogError($"An error occurred while requesting a tv show \"{tvShowTitle}\" from Ombi, could not find category with id {request.CategoryId}");
                throw new Exception($"An error occurred while requesting a tv show \"{tvShowTitle}\" from Ombi, could not find category with id {request.CategoryId}");
            }
        }
    }
}