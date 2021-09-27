using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Requestrr.WebApi.Extensions;
using Requestrr.WebApi.RequestrrBot.Movies;
using Requestrr.WebApi.RequestrrBot.TvShows;

namespace Requestrr.WebApi.RequestrrBot.DownloadClients.Overseerr
{
    public class OverseerrClient : IMovieRequester, IMovieSearcher, ITvShowSearcher, ITvShowRequester
    {
        private IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OverseerrClient> _logger;
        private OverseerrSettingsProvider _overseerrSettingsProvider;
        private OverseerrSettings OverseerrSettings => _overseerrSettingsProvider.Provide();
        private string BaseURL => GetBaseURL(OverseerrSettings);
        private ConcurrentDictionary<string, int> _requesterIdToOverseerUserID = new ConcurrentDictionary<string, int>();

        public OverseerrClient(IHttpClientFactory httpClientFactory, ILogger<OverseerrClient> logger, OverseerrSettingsProvider overseerrSettingsProvider)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _overseerrSettingsProvider = overseerrSettingsProvider;
        }

        public static async Task TestConnectionAsync(HttpClient httpClient, ILogger<OverseerrClient> logger, OverseerrSettings settings)
        {
            var testSuccessful = false;

            try
            {
                var response = await HttpGetAsync(httpClient, settings, $"{GetBaseURL(settings)}settings/main");

                if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                {
                    throw new Exception("Invalid api key");
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new Exception("Invalid host and/or port");
                }

                try
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    dynamic jsonResponse = JObject.Parse(responseString);

                    if (!jsonResponse.apiKey.ToString().Equals(settings.ApiKey, StringComparison.InvariantCultureIgnoreCase))
                    {
                        throw new Exception("Invalid host and/or port");
                    }
                }
                catch
                {
                    throw new Exception("Invalid host and/or port");
                }

                if (!string.IsNullOrWhiteSpace(settings.DefaultApiUserID))
                {
                    if (!int.TryParse(settings.DefaultApiUserID, out var userId))
                    {
                        throw new Exception("Overseerr default user ID must be a number.");
                    }

                    response = await HttpGetAsync(httpClient, settings, $"{GetBaseURL(settings)}user/{userId}");

                    try
                    {
                        await response.ThrowIfNotSuccessfulAsync("OverseerrFindSpecificUser failed", x => x.error);
                    }
                    catch (System.Exception ex)
                    {
                        logger.LogWarning(ex, $"Default overseerr user with user id  \"{userId}\" could not found: " + ex.Message);
                        throw new Exception($"Default overseerr user with user id \"{userId}\" could not found.");
                    }
                }

                testSuccessful = true;
            }
            catch (HttpRequestException ex)
            {
                logger.LogWarning(ex, "Error while testing Overseeerr connection: " + ex.Message);
                throw new Exception("Invalid host and/or port");
            }
            catch (System.Exception ex)
            {
                logger.LogWarning(ex, "Error while testing Overseeerr connection: " + ex.Message);

                if (ex.GetType() == typeof(System.Exception))
                {
                    throw;
                }
                else
                {
                    throw new Exception("Invalid host and/or port");
                }
            }

            if (!testSuccessful)
            {
                throw new Exception("Invalid host and/or port");
            }
        }

        async Task<Dictionary<int, Movie>> IMovieSearcher.SearchAvailableMoviesAsync(HashSet<int> movieIds, CancellationToken token)
        {
            try
            {
                var movies = new HashSet<Movie>();

                foreach (var movieId in movieIds)
                {
                    try
                    {
                        var movie = await SearchMovieAsync(movieId);

                        if (movie.Available)
                        {
                            movies.Add(movie);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        _logger.LogError(ex, $"An error occurred while getting availability for a specific movie with TMDB ID {movieId} from Overseer: " + ex.Message);
                    }
                }

                return movies.Where(x => x != null).ToDictionary(x => int.Parse(x.TheMovieDbId), x => x);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occurred while searching for availables movies from Overseerr: " + ex.Message);
                throw new System.Exception("An error occurred while searching for availables movies from Overseerr: " + ex.Message);
            }
        }

        async Task<IReadOnlyList<TvShow>> ITvShowSearcher.GetTvShowDetailsAsync(HashSet<int> tvShowIds, CancellationToken token)
        {
            try
            {
                var tvShows = new List<TvShow>();

                foreach (var showId in tvShowIds)
                {
                    try
                    {
                        var show = await GetTvShowDetailsAsync(showId);
                        tvShows.Add(show);
                    }
                    catch (System.Exception ex)
                    {
                        _logger.LogError(ex, $"An error occurred while getting availability for a specific tv show with TMDB ID {showId} from Overseer: " + ex.Message);
                    }
                }

                return tvShows.Where(x => x != null).ToArray();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting tv show details from Overseer: " + ex.Message);
                throw new System.Exception("An error occurred while getting tv show details from Overseer: " + ex.Message);
            }
        }

        public async Task<IReadOnlyList<Movie>> SearchMovieAsync(string movieName)
        {
            try
            {
                var response = await HttpGetAsync($"{BaseURL}search/?query={Uri.EscapeDataString(movieName)}&page=1&language=en");
                await response.ThrowIfNotSuccessfulAsync("OverseerrMovieSearch failed", x => x.error);

                var jsonResponse = await response.Content.ReadAsStringAsync();

                var movies = JsonConvert.DeserializeObject<JSONSearchResult>(jsonResponse).Results
                    .Where(x => x.MediaType == MediaTypes.MOVIE)
                    .ToArray();

                return movies.Select(ConvertMovie).ToArray();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occurred while searching for movies from Overseerr: " + ex.Message);
                throw new System.Exception("An error occurred while searching for movies from Overseerr: " + ex.Message);
            }
        }
        
        public async Task<Movie> SearchMovieAsync(int theMovieDbId)
        {
            try
            {
                var response = await HttpGetAsync($"{BaseURL}movie/{theMovieDbId}");
                await response.ThrowIfNotSuccessfulAsync("OverseerrMovieSearchByMovieDbId failed", x => x.error);

                var jsonResponse = await response.Content.ReadAsStringAsync();

                var movie = JsonConvert.DeserializeObject<JSONMedia>(jsonResponse);

                return ConvertMovie(movie);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while searching for a movie by tmdbId \"{theMovieDbId}\" from Overseerr: " + ex.Message);
                throw new System.Exception($"An error occurred while searching for a movie by tmdbId \"{theMovieDbId}\" from Overseerr: " + ex.Message);
            }
        }

        async Task<MovieRequestResult> IMovieRequester.RequestMovieAsync(MovieUserRequester requester, Movie movie)
        {
            try
            {
                var overseerrUser = await FindLinkedOverseerUserAsync(requester.UserId, requester.Username);

                var response = await HttpPostAsync(overseerrUser, $"{BaseURL}request", JsonConvert.SerializeObject(new
                {
                    mediaId = int.Parse(movie.TheMovieDbId),
                    mediaType = "movie",
                }));

                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return new MovieRequestResult
                    {
                        WasDenied = true
                    };
                }

                await response.ThrowIfNotSuccessfulAsync("OverseerrRequestMovieRequest failed", x => x.error);

                return new MovieRequestResult();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"An error while requesting movie \"{movie.Title}\" from Overseerr: " + ex.Message);
                throw new System.Exception($"An error while requesting movie \"{movie.Title}\" from Overseerr: " + ex.Message);
            }
        }

        public Task<MovieDetails> GetMovieDetails(string theMovieDbId)
        {
            return TheMovieDb.GetMovieDetailsAsync(_httpClientFactory.CreateClient(), theMovieDbId, _logger);
        }

        async Task<IReadOnlyList<SearchedTvShow>> ITvShowSearcher.SearchTvShowAsync(string tvShowName)
        {
            try
            {
                var response = await HttpGetAsync($"{BaseURL}search/?query={Uri.EscapeDataString(tvShowName)}&page=1&language=en");
                await response.ThrowIfNotSuccessfulAsync("OverseerrTvShowSearch failed", x => x.error);

                var jsonResponse = await response.Content.ReadAsStringAsync();

                var tvShows = JsonConvert.DeserializeObject<JSONSearchResult>(jsonResponse).Results
                    .Where(x => x.MediaType == MediaTypes.TV)
                    .ToArray();

                return tvShows.Select(ConvertSearchedTvShow).ToArray();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occurred while searching for tv shows from Overseerr: " + ex.Message);
                throw new System.Exception("An error occurred while searching for tv shows from Overseerr: " + ex.Message);
            }
        }

        public async Task<TvShow> GetTvShowDetailsAsync(int theTvDbId)
        {
            try
            {
                var response = await HttpGetAsync($"{BaseURL}tv/{theTvDbId}");
                await response.ThrowIfNotSuccessfulAsync("OverseerrGetTvShowDetail failed", x => x.error);

                var jsonResponse = await response.Content.ReadAsStringAsync();

                var tvShow = JsonConvert.DeserializeObject<JSONMedia>(jsonResponse);

                return ConvertTvShow(tvShow);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while searching for a tv show by tmdbId \"{theTvDbId}\" from Overseerr: " + ex.Message);
                throw new System.Exception($"An error occurred while searching for a tv show by tmdbId \"{theTvDbId}\" from Overseerr: " + ex.Message);
            }
        }

        async Task<TvShowRequestResult> ITvShowRequester.RequestTvShowAsync(TvShowUserRequester requester, TvShow tvShow, TvSeason season)
        {
            try
            {
                var wantedSeasonIds = season is AllTvSeasons
                    ? new HashSet<int>(tvShow.Seasons.OfType<NormalTvSeason>().Select(x => x.SeasonNumber))
                    : season is FutureTvSeasons
                        ? new HashSet<int>()
                        : new HashSet<int> { season.SeasonNumber };

                var overseerrUser = await FindLinkedOverseerUserAsync(requester.UserId, requester.Username);

                var response = await HttpPostAsync(overseerrUser, $"{BaseURL}request", JsonConvert.SerializeObject(new
                {
                    mediaId = tvShow.TheTvDbId,
                    mediaType = "tv",
                    seasons = wantedSeasonIds.ToArray(),
                }));

                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return new TvShowRequestResult
                    {
                        WasDenied = true
                    };
                }

                await response.ThrowIfNotSuccessfulAsync("OverseerrRequestTvShowRequest failed", x => x.error);

                return new TvShowRequestResult();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"An error while requesting tv show \"{tvShow.Title}\" from Overseerr: " + ex.Message);
                throw new System.Exception($"An error while requesting tv show \"{tvShow.Title}\" from Overseerr: " + ex.Message);
            }
        }

        Task<SearchedTvShow> ITvShowSearcher.SearchTvShowAsync(int tvDbId)
        {
            throw new NotSupportedException("Overseerr does not support searching via TheTvDatabase.");
        }

        private async Task<string> FindLinkedOverseerUserAsync(string userId, string username)
        {
            if (_requesterIdToOverseerUserID.TryGetValue(userId, out var overseerrUserID))
            {
                var notificationSettings = await GetUserNotificationSettings(overseerrUserID);

                if (notificationSettings.DiscordID != null && notificationSettings.DiscordID.Equals(userId, StringComparison.InvariantCultureIgnoreCase))
                {
                    return overseerrUserID.ToString();
                }
                else
                {
                    _requesterIdToOverseerUserID.Remove(userId, out _);
                }
            }

            var response = await HttpGetAsync($"{BaseURL}user?take={int.MaxValue}&skip=0&sort=created");
            var jsonResponse = await response.Content.ReadAsStringAsync();
            await response.ThrowIfNotSuccessfulAsync("OverseerrFindAllUsers failed", x => x.error);

            var users = JsonConvert.DeserializeObject<JSONAllUserQuery>(jsonResponse).Results;

            foreach (var user in users)
            {
                try
                {
                    var notificationSettings = await GetUserNotificationSettings(user.ID);

                    if (notificationSettings.DiscordID != null && notificationSettings.DiscordID.Equals(userId, StringComparison.InvariantCultureIgnoreCase))
                    {
                        _requesterIdToOverseerUserID[userId] = user.ID;
                        return user.ID.ToString();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Could not get notification settings from Overseerr for user {user.ID}: " + ex.Message);
                }
            }

            return !string.IsNullOrWhiteSpace(OverseerrSettings.DefaultApiUserID) && int.TryParse(OverseerrSettings.DefaultApiUserID, out var defaultUserId)
                ? defaultUserId.ToString()
                : null;
        }

        private async Task<JSONUserNotificationSettings> GetUserNotificationSettings(int userId)
        {
            var response = await HttpGetAsync($"{BaseURL}user/{userId}/settings/notifications");
            var jsonResponse = await response.Content.ReadAsStringAsync();
            await response.ThrowIfNotSuccessfulAsync("OverseerrFindSpecificUserNotifications failed", x => x.error);

            return JsonConvert.DeserializeObject<JSONUserNotificationSettings>(jsonResponse);
        }

        private Movie ConvertMovie(JSONMedia jsonMedia)
        {
            return new Movie
            {
                TheMovieDbId = jsonMedia.Id.ToString(),
                Title = jsonMedia.Title,
                Available = jsonMedia.MediaInfo?.Status == MediaStatus.AVAILABLE,
                Quality = string.Empty,
                Requested = jsonMedia.MediaInfo?.Status == MediaStatus.PENDING
                    || jsonMedia.MediaInfo?.Status == MediaStatus.PROCESSING
                    || jsonMedia.MediaInfo?.Status == MediaStatus.PARTIALLY_AVAILABLE
                    || jsonMedia.MediaInfo?.Status == MediaStatus.AVAILABLE,
                PlexUrl = jsonMedia.MediaInfo?.PlexUrl,
                Overview = jsonMedia.Overview,
                PosterPath = !string.IsNullOrWhiteSpace(jsonMedia.PosterPath) ? $"https://image.tmdb.org/t/p/w500{jsonMedia.PosterPath}" : null,
                ReleaseDate = jsonMedia.ReleaseDate,
            };
        }

        private SearchedTvShow ConvertSearchedTvShow(JSONMedia jsonMedia)
        {
            return new SearchedTvShow
            {
                TheTvDbId = jsonMedia.Id,
                Title = jsonMedia.Name,
                Banner = !string.IsNullOrWhiteSpace(jsonMedia.PosterPath) ? $"https://image.tmdb.org/t/p/w500{jsonMedia.PosterPath}" : null,
                FirstAired = jsonMedia.FirstAirDate,
            };
        }

        private TvShow ConvertTvShow(JSONMedia jsonMedia)
        {
            return new TvShow
            {
                TheTvDbId = jsonMedia.Id,
                Title = jsonMedia.Name,
                Banner = !string.IsNullOrWhiteSpace(jsonMedia.PosterPath) ? $"https://image.tmdb.org/t/p/w500{jsonMedia.PosterPath}" : null,
                FirstAired = jsonMedia.FirstAirDate,
                IsRequested = jsonMedia.MediaInfo?.Status == MediaStatus.PENDING
                    || jsonMedia.MediaInfo?.Status == MediaStatus.PROCESSING
                    || jsonMedia.MediaInfo?.Status == MediaStatus.PARTIALLY_AVAILABLE
                    || jsonMedia.MediaInfo?.Status == MediaStatus.AVAILABLE,
                Quality = string.Empty,
                WebsiteUrl = jsonMedia.MediaInfo?.TvdbId != null ? $"https://www.thetvdb.com/?id={jsonMedia.MediaInfo?.TvdbId}&tab=series" : null,
                PlexUrl = jsonMedia.MediaInfo?.PlexUrl,
                Overview = jsonMedia.Overview,
                HasEnded = !jsonMedia.InProduction,
                Network = jsonMedia.Networks.FirstOrDefault()?.Name,
                Status = jsonMedia.Status,
                Seasons = ConvertSeasons(jsonMedia)
            };
        }

        private TvSeason[] ConvertSeasons(JSONMedia jsonMedia)
        {
            var seasons = jsonMedia.Seasons.Select(x =>
                new NormalTvSeason
                {
                    SeasonNumber = x.SeasonNumber,
                    IsAvailable = x.Status == MediaStatus.AVAILABLE,
                    IsRequested = ConvertRequestedState(x.Status)
                }).ToArray();

            if (jsonMedia.MediaInfo != null)
            {
                if (jsonMedia.MediaInfo.Requests != null && jsonMedia.MediaInfo.Requests.Any())
                {
                    foreach (var season in seasons)
                    {
                        var request = jsonMedia.MediaInfo.Requests
                            .Where(x => x.Seasons.Any(s => s.SeasonNumber == season.SeasonNumber))
                            .Where(x => x.Status == MediaRequestStatus.PENDING || x.Status == MediaRequestStatus.APPROVED)
                            .FirstOrDefault();

                        if (request != null)
                        {
                            season.IsAvailable = false;
                            season.IsRequested = RequestedState.Full;
                        }
                    }
                }

                if (jsonMedia.MediaInfo != null && jsonMedia.MediaInfo.Seasons.Any())
                {
                    foreach (var season in seasons)
                    {
                        var mediaSeason = jsonMedia.MediaInfo.Seasons.FirstOrDefault(x => x.SeasonNumber == season.SeasonNumber);

                        if (mediaSeason != null && (mediaSeason.Status == MediaStatus.PROCESSING || mediaSeason.Status == MediaStatus.PARTIALLY_AVAILABLE || mediaSeason.Status == MediaStatus.AVAILABLE))
                        {
                            season.IsAvailable = mediaSeason.Status == MediaStatus.AVAILABLE;
                            season.IsRequested = ConvertRequestedState(mediaSeason.Status);
                        }
                    }
                }
            }

            return seasons.Where(x => x.SeasonNumber > 0).ToArray();
        }

        private RequestedState ConvertRequestedState(MediaStatus status)
        {
            if (status == MediaStatus.UNKNOWN || status == MediaStatus.PENDING)
            {
                return RequestedState.None;
            }

            if (status == MediaStatus.AVAILABLE || status == MediaStatus.PROCESSING || status == MediaStatus.PARTIALLY_AVAILABLE)
            {
                return RequestedState.Full;
            }

            return RequestedState.None;
        }

        private Task<HttpResponseMessage> HttpGetAsync(string url)
        {
            return HttpGetAsync(_httpClientFactory.CreateClient(), OverseerrSettings, url);
        }

        private static async Task<HttpResponseMessage> HttpGetAsync(HttpClient client, OverseerrSettings settings, string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("X-Api-Key", settings.ApiKey);

            return await client.SendAsync(request);
        }

        private async Task<HttpResponseMessage> HttpPostAsync(string overseerrUserId, string url, string content)
        {
            var postRequest = new StringContent(content);
            postRequest.Headers.Clear();
            postRequest.Headers.Add("Content-Type", "application/json;charset=utf-8");
            postRequest.Headers.Add("X-Api-Key", OverseerrSettings.ApiKey);

            if (!string.IsNullOrWhiteSpace(overseerrUserId))
            {
                postRequest.Headers.Add("X-API-User", overseerrUserId);
            }

            var client = _httpClientFactory.CreateClient();
            return await client.PostAsync(url, postRequest);
        }

        private static string GetBaseURL(OverseerrSettings settings)
        {
            var protocol = settings.UseSSL ? "https" : "http";
            return $"{protocol}://{settings.Hostname}:{settings.Port}/api/v{settings.Version}/";
        }

        public class JSONAllUserQuery
        {
            [JsonProperty("results")]
            public List<JSONUserResult> Results { get; set; }
        }

        public class JSONUserNotificationSettings
        {
            [JsonProperty("discordId")]
            public string DiscordID { get; set; }
        }

        public class JSONUserResult
        {
            [JsonProperty("id")]
            public int ID { get; set; }
        }

        public class JSONNetwork
        {
            [JsonProperty("name")]
            public string Name { get; set; }
        }


        public class JSONTvSeason
        {
            [JsonProperty("id")]
            public int ID { get; set; }

            [JsonProperty("seasonNumber")]
            public int SeasonNumber { get; set; }

            [JsonProperty("status")]
            public int StatusValue { get; set; }

            public MediaStatus Status
            {
                get
                {
                    return (MediaStatus)StatusValue;
                }
            }

            [JsonProperty("status4k")]
            public int Status4kValue { get; set; }

            public MediaStatus Status4k
            {
                get
                {
                    return (MediaStatus)Status4kValue;
                }
            }
        }

        public class JSONRequest
        {
            [JsonProperty("status")]
            public int StatusValue { get; set; }

            public MediaRequestStatus Status => (MediaRequestStatus)StatusValue;

            [JsonProperty("seasons")]
            public List<JSONTvSeason> Seasons { get; set; }
        }

        public class MediaInfo
        {
            [JsonProperty("downloadStatus")]
            public List<object> DownloadStatus { get; set; }

            [JsonProperty("downloadStatus4k")]
            public List<object> DownloadStatus4k { get; set; }

            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("mediaType")]
            public string MediaTypeValue { get; set; }

            public MediaTypes MediaType
            {
                get
                {
                    return MediaTypesConverter.FromString(MediaTypeValue);
                }
            }

            [JsonProperty("tmdbId")]
            public int? TmdbId { get; set; }

            [JsonProperty("tvdbId")]
            public object TvdbId { get; set; }

            [JsonProperty("imdbId")]
            public string ImdbId { get; set; }

            [JsonProperty("status")]
            public int StatusValue { get; set; }

            public MediaStatus Status
            {
                get
                {
                    return (MediaStatus)StatusValue;
                }
            }

            [JsonProperty("status4k")]
            public int Status4k { get; set; }

            [JsonProperty("createdAt")]
            public DateTime? CreatedAt { get; set; }

            [JsonProperty("mediaAddedAt")]
            public DateTime? MediaAddedAt { get; set; }

            [JsonProperty("ratingKey")]
            public string RatingKey { get; set; }

            [JsonProperty("ratingKey4k")]
            public object RatingKey4k { get; set; }

            [JsonProperty("plexUrl")]
            public string PlexUrl { get; set; }

            [JsonProperty("seasons")]
            public List<JSONTvSeason> Seasons { get; set; }

            [JsonProperty("requests")]
            public List<JSONRequest> Requests { get; set; }
        }

        public enum MediaTypes
        {
            NOT_SUPPORTED,
            MOVIE,
            TV
        }

        public static class MediaTypesConverter
        {
            public static MediaTypes FromString(string value)
            {
                if (value.Equals("movie", StringComparison.InvariantCultureIgnoreCase))
                    return MediaTypes.MOVIE;

                if (value.Equals("tv", StringComparison.InvariantCultureIgnoreCase))
                    return MediaTypes.TV;

                return MediaTypes.NOT_SUPPORTED;
            }
        }

        public enum MediaStatus
        {
            UNKNOWN = 1,
            PENDING = 2,
            PROCESSING = 3,
            PARTIALLY_AVAILABLE = 4,
            AVAILABLE = 5,
        }

        public enum MediaRequestStatus
        {
            PENDING = 1,
            APPROVED = 2,
            DECLINED = 3,
        }

        public class JSONMedia
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("mediaType")]
            public string MediaTypeValue { get; set; }

            public MediaTypes MediaType
            {
                get
                {
                    return MediaTypesConverter.FromString(MediaTypeValue);
                }
            }

            [JsonProperty("overview")]
            public string Overview { get; set; }

            [JsonProperty("releaseDate")]
            public string ReleaseDate { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("inProduction")]
            public bool InProduction { get; set; }

            [JsonProperty("posterPath")]
            public string PosterPath { get; set; }

            [JsonProperty("firstAirDate")]
            public string FirstAirDate { get; set; }

            [JsonProperty("mediaInfo")]
            public MediaInfo MediaInfo { get; set; }

            [JsonProperty("networks")]
            public List<JSONNetwork> Networks { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("seasons")]
            public List<JSONTvSeason> Seasons { get; set; }
        }

        public class JSONSearchResult
        {
            [JsonProperty("results")]
            public List<JSONMedia> Results { get; set; }
        }
    }
}