using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Requestrr.WebApi.Requestrr.Movies;
using Requestrr.WebApi.Requestrr.TvShows;

namespace Requestrr.WebApi.Requestrr.DownloadClients
{
    public class Ombi : IMovieRequester, IMovieSearcher, ITvShowSearcher, ITvShowRequester
    {
        private IHttpClientFactory _httpClientFactory;
        private readonly ILogger<Ombi> _logger;
        private OmbiSettingsProvider _ombiSettingsProvider;
        private OmbiSettings OmbiSettings => _ombiSettingsProvider.Provide();
        private string BaseURL => GetBaseURL(OmbiSettings);

        public Ombi(IHttpClientFactory httpClientFactory, ILogger<Ombi> logger, OmbiSettingsProvider OmbiSettingsProvider)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _ombiSettingsProvider = OmbiSettingsProvider;
        }

        public static async Task TestConnectionAsync(HttpClient httpClient, ILogger<Ombi> logger, OmbiSettings settings)
        {
            var testSuccessful = false;

            try
            {
                var response = await HttpGetAsync(httpClient, settings, $"{GetBaseURL(settings)}/api/v1/LandingPage");
                testSuccessful = response.IsSuccessStatusCode;
            }
            catch (System.Exception ex)
            {

                logger.LogWarning("Error while testing ombi connection: " + ex.Message);
                throw new Exception("Could not connect to Radarr");
            }

            if (!testSuccessful)
            {
                throw new Exception("Could not connect to Radarr");
            }
        }

        public async Task RequestMovieAsync(string username, Movie movie)
        {
            var retryCount = 0;

            while (retryCount <= 5)
            {
                try
                {
                    var response = await HttpPostAsync(username, $"{BaseURL}/api/v1/Request/Movie", JsonConvert.SerializeObject(new { theMovieDbId = movie.TheMovieDbId }));
                    await response.ThrowIfNotSuccessfulAsync("OmbiCreateMovieRequest failed", x => x.error);

                    return;
                }
                catch (System.Exception ex)
                {
                    _logger.LogWarning($"An error while requesting movie \"{movie.Title}\" from Ombi: " + ex.Message);
                    retryCount++;
                    await Task.Delay(1000);
                }
            }

            throw new System.Exception("An error occurred while requesting a movie from oOmbimbi");
        }

        public async Task<IReadOnlyList<Movie>> SearchMovieAsync(string movieName)
        {
            var retryCount = 0;

            while (retryCount <= 5)
            {
                try
                {
                    var response = await HttpGetAsync($"{BaseURL}/api/v1/search/movie/{movieName}");
                    await response.ThrowIfNotSuccessfulAsync("OmbiMovieSearch failed", x => x.error);

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var movies = JsonConvert.DeserializeObject<List<JSONMovie>>(jsonResponse).ToArray();

                    return movies.Select(Convert).ToArray();
                }
                catch (System.Exception ex)
                {
                    _logger.LogWarning("An error occurred while searching for movies from Ombi: " + ex.Message);
                    retryCount++;
                    await Task.Delay(1000);
                }
            }

            throw new System.Exception("An error occurred while searching for movies from Ombi");
        }

        public Task<MovieDetails> GetMovieDetails(string theMovieDbId)
        {
            return TheMovieDb.GetMovieDetailsAsync(_httpClientFactory.CreateClient(), theMovieDbId, _logger);
        }

        public async Task<Movie> GetMovieAsync(int movieId)
        {
            var retryCount = 0;

            while (retryCount <= 5)
            {
                try
                {
                    var response = await HttpGetAsync($"{BaseURL}/api/v1/search/movie/info/{movieId.ToString()}");
                    await response.ThrowIfNotSuccessfulAsync("OmbiGetMovieInfo failed", x => x.error);

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var jsonMovie = JsonConvert.DeserializeObject<JSONMovie>(jsonResponse);

                    return Convert(jsonMovie);
                }
                catch (System.Exception ex)
                {
                    _logger.LogWarning("An error occurred while searching for a movie from Ombi: " + ex.Message);
                    retryCount++;
                    await Task.Delay(1000);
                }
            }

            throw new System.Exception("An error occurred while searching for a movie from Ombi");
        }

        public async Task<Dictionary<int, Movie>> SearchAvailableMoviesAsync(HashSet<int> movieIds, System.Threading.CancellationToken token)
        {
            var retryCount = 0;

            while (retryCount <= 5 && !token.IsCancellationRequested)
            {
                try
                {
                    var movies = new HashSet<Movie>();

                    foreach (var movieId in movieIds)
                    {
                        await Task.Delay(100);
                        var movie = await GetMovieAsync(movieId);

                        if (movie.Available)
                        {
                            movies.Add(movie);
                        }
                    }

                    return movies.ToDictionary(x => int.Parse(x.TheMovieDbId), x => x);
                }
                catch (System.Exception ex)
                {
                    _logger.LogWarning("An error occurred while searching for availables movies from Ombi: " + ex.Message);
                    retryCount++;
                    await Task.Delay(1000, token);
                }
            }

            throw new System.Exception("An error occurred while searching for availables movies from Ombi");
        }

        public async Task RequestTvShowAsync(string username, TvShow tvShow, TvSeason season)
        {
            var retryCount = 0;

            while (retryCount <= 5)
            {
                try
                {
                    var jsonTvShow = await FindTvShowByTheTvDbIdAsync(tvShow.TheTvDbId.ToString());

                    var wantedSeasonIds = season is AllTvSeasons
                        ? new HashSet<int>(tvShow.Seasons.Select(x => x.SeasonNumber))
                        : season is FutureTvSeasons
                            ? new HashSet<int>()
                            : new HashSet<int> { season.SeasonNumber };

                    var response = await HttpPostAsync(username, $"{BaseURL}/api/v1/Request/Tv", JsonConvert.SerializeObject(new
                    {
                        tvDbId = tvShow.TheTvDbId,
                        requestAll = false,
                        latestSeason = false,
                        firstSeason = false,
                        seasons = jsonTvShow.seasonRequests.Select(s => new
                        {
                            seasonNumber = s.seasonNumber,
                            episodes = wantedSeasonIds.Contains(s.seasonNumber) && s.CanBeRequested() ? s.episodes.Select(e => new JSONTvEpisode { episodeNumber = e.episodeNumber }) : Array.Empty<JSONTvEpisode>()
                        }),
                    }));

                    await response.ThrowIfNotSuccessfulAsync("OmbiCreateTvShowRequest failed", x => x.error);

                    return;
                }
                catch (System.Exception ex)
                {
                    _logger.LogWarning($"An error while requesting tv show \"{tvShow.Title}\" from Ombi: " + ex.Message);
                    retryCount++;
                    await Task.Delay(1000);
                }
            }

            throw new System.Exception("Something went wrong while requesting a tv show from Ombi");
        }

        public async Task<TvShow> GetTvShowDetailsAsync(SearchedTvShow searchedTvShow)
        {
            var retryCount = 0;

            while (retryCount <= 5)
            {
                try
                {
                    var jsonTvShow = await FindTvShowByTheTvDbIdAsync(searchedTvShow.TheTvDbId.ToString());
                    return Convert(jsonTvShow);
                }
                catch (System.Exception ex)
                {
                    _logger.LogWarning("An error occurred while getting details for a tv show from Ombi: " + ex.Message);
                    retryCount++;
                    await Task.Delay(1000);
                }
            }

            throw new System.Exception("An error occurred while getting details for a tv show from Ombi");
        }

        public async Task<IReadOnlyList<TvShow>> GetTvShowDetailsAsync(HashSet<int> tvShowIds, System.Threading.CancellationToken token)
        {
            var retryCount = 0;

            while (retryCount <= 5 && !token.IsCancellationRequested)
            {
                try
                {
                    var tvShows = new List<TvShow>();

                    foreach (var showId in tvShowIds)
                    {
                        await Task.Delay(100);
                        var show = await GetTvShowDetailsAsync(new SearchedTvShow { TheTvDbId = showId });
                        tvShows.Add(show);
                    }

                    return tvShows;
                }
                catch (System.Exception ex)
                {
                    _logger.LogWarning("An error occurred while getting tv show details from Ombi: " + ex.Message);
                    retryCount++;
                    await Task.Delay(1000, token);
                }
            }

            throw new System.Exception("An error occurred while getting tv show details from Ombi");
        }

        public async Task<IReadOnlyList<SearchedTvShow>> SearchTvShowAsync(string tvShowName)
        {
            var retryCount = 0;

            while (retryCount <= 5)
            {
                try
                {
                    var response = await HttpGetAsync($"{BaseURL}/api/v1/Search/Tv/{tvShowName}");

                    await response.ThrowIfNotSuccessfulAsync("OmbiSearchTvShow failed", x => x.error);

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var jsonTvShows = JsonConvert.DeserializeObject<List<JSONTvShow>>(jsonResponse).ToArray();

                    return jsonTvShows.Select(x => new SearchedTvShow
                    {
                        Title = x.title,
                        Banner = x.banner,
                        TheTvDbId = x.id,
                        FirstAired = x.firstAired,
                    }).ToArray();
                }
                catch (System.Exception ex)
                {
                    _logger.LogWarning($"An error occurred while searching for tv show \"{tvShowName}\" from Ombi: " + ex.Message);
                    retryCount++;
                    await Task.Delay(1000);
                }
            }

            throw new System.Exception("An error occurred while searching for tv show from Ombi");
        }

        private async Task<JSONTvShow> FindTvShowByTheTvDbIdAsync(string theTvDbId)
        {
            var response = await HttpGetAsync($"{BaseURL}/api/v1/Search/tv/info/{theTvDbId}");

            var jsonResponse = await response.Content.ReadAsStringAsync();
            await response.ThrowIfNotSuccessfulAsync("OmbiFindTvShowTvDbId failed", x => x.error);

            return JsonConvert.DeserializeObject<JSONTvShow>(jsonResponse);
        }

        private Movie Convert(JSONMovie jsonMovie)
        {
            return new Movie
            {
                TheMovieDbId = jsonMovie.theMovieDbId,
                Title = jsonMovie.title,
                Available = jsonMovie.available,
                Quality = jsonMovie.quality,
                Requested = jsonMovie.requested,
                Approved = jsonMovie.approved,
                PlexUrl = jsonMovie.plexUrl,
                EmbyUrl = jsonMovie.embyUrl,
                Overview = jsonMovie.overview,
                PosterPath = !string.IsNullOrWhiteSpace(jsonMovie.posterPath) ? $"https://image.tmdb.org/t/p/w500{jsonMovie.posterPath}" : null,
                ReleaseDate = jsonMovie.releaseDate,
            };
        }

        private TvShow Convert(JSONTvShow jsonTvShow)
        {
            return new TvShow
            {
                TheTvDbId = jsonTvShow.id,
                Title = jsonTvShow.title,
                Quality = jsonTvShow.quality,
                PlexUrl = jsonTvShow.plexUrl,
                EmbyUrl = jsonTvShow.embyUrl,
                Overview = jsonTvShow.overview,
                Banner = jsonTvShow.banner,
                FirstAired = jsonTvShow.firstAired,
                Network = jsonTvShow.network,
                Status = jsonTvShow.status,
                HasEnded = !string.IsNullOrWhiteSpace(jsonTvShow.status) && jsonTvShow.status.Equals("ended", StringComparison.InvariantCultureIgnoreCase),
                Seasons = jsonTvShow.seasonRequests.Select(x => new NormalTvSeason
                {
                    SeasonNumber = x.seasonNumber,
                    IsAvailable = x.episodes.FirstOrDefault()?.available == true,
                    IsRequested = !x.CanBeRequested(),
                }).ToArray(),
                IsRequested = jsonTvShow.requested || jsonTvShow.available,
            };
        }

        private static string GetBaseURL(OmbiSettings settings)
        {
            var protocol = settings.UseSSL ? "https" : "http";
            return $"{protocol}://{settings.Hostname}:{settings.Port}";
        }

        private Task<HttpResponseMessage> HttpGetAsync(string url)
        {
            return HttpGetAsync(_httpClientFactory.CreateClient(), OmbiSettings, url);
        }

        private static async Task<HttpResponseMessage> HttpGetAsync(HttpClient client, OmbiSettings settings, string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("ApiKey", settings.ApiKey);

            return await client.SendAsync(request);
        }

        private async Task<HttpResponseMessage> HttpPostAsync(string username, string url, string content)
        {
            var postRequest = new StringContent(content);
            postRequest.Headers.Clear();
            postRequest.Headers.Add("Content-Type", "application/json");
            postRequest.Headers.Add("ApiKey", OmbiSettings.ApiKey);
            postRequest.Headers.Add("ApiAlias", username);
            postRequest.Headers.Add("UserName", string.IsNullOrWhiteSpace(OmbiSettings.ApiUsername) ? "api" : OmbiSettings.ApiUsername);

            var client = _httpClientFactory.CreateClient();
            return await client.PostAsync(url, postRequest);
        }

        public class JSONMovie
        {
            public string title { get; set; }
            public bool available { get; set; }
            public string quality { get; set; }
            public bool requested { get; set; }
            public bool approved { get; set; }
            public string plexUrl { get; set; }
            public string embyUrl { get; set; }
            public string overview { get; set; }
            public string posterPath { get; set; }
            public string releaseDate { get; set; }
            public string theMovieDbId { get; set; }
        }

        public class JSONTvShow
        {
            public int id { get; set; }
            public string title { get; set; }
            public string quality { get; set; }
            public string plexUrl { get; set; }
            public string embyUrl { get; set; }
            public string overview { get; set; }
            public bool requested { get; set; }
            public bool available { get; set; }
            public string banner { get; set; }
            public string firstAired { get; set; }
            public string network { get; set; }
            public string status { get; set; }
            public JSONTvSeason[] seasonRequests { get; set; }
        }

        public class JSONTvSeason
        {
            public int seasonNumber { get; set; }
            public JSONTvEpisode[] episodes { get; set; }

            public bool CanBeRequested()
            {
                return !episodes.Any(e => !e.CanBeRequested());
            }
        }

        public class JSONTvEpisode
        {
            public int episodeNumber { get; set; }
            public bool available { get; set; }
            public bool approved { get; set; }
            public bool requested { get; set; }

            public bool CanBeRequested()
            {
                return !available && !requested && !approved;
            }
        }
    }
}