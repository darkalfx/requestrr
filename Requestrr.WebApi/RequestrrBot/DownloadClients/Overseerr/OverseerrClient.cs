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
    public class OverseerrClient : IMovieRequester, IMovieSearcher, ITvShowSearcher, ITvShowRequester, IMovieIssueSearcher, IMovieIssueRequester
    {
        private IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OverseerrClient> _logger;
        private OverseerrSettingsProvider _overseerrSettingsProvider;
        private OverseerrSettings OverseerrSettings => _overseerrSettingsProvider.Provide();
        private string BaseURL => GetBaseURL(OverseerrSettings);
        private ConcurrentDictionary<string, int> _requesterIdToOverseerUserID = new ConcurrentDictionary<string, int>();
        private static OverseerrTvShowCategory DefaultTvShowCategory = new OverseerrTvShowCategory { Is4K = false };
        private static OverseerrMovieCategory DefaultMovieCategory = new OverseerrMovieCategory { Is4K = false };

        public List<string> IssueTypes { get => new List<string> { "Video", "Audio", "Subtitle", "Other" }; }

        public OverseerrClient(IHttpClientFactory httpClientFactory, ILogger<OverseerrClient> logger, OverseerrSettingsProvider overseerrSettingsProvider)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _overseerrSettingsProvider = overseerrSettingsProvider;
        }

        public static async Task TestConnectionAsync(HttpClient httpClient, ILogger<OverseerrClient> logger, OverseerrTestSettings settings)
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

                if (!string.IsNullOrWhiteSpace(settings.DefaultApiUserId))
                {
                    if (!int.TryParse(settings.DefaultApiUserId, out var userId))
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

        public static async Task<RadarrServiceSettings> GetRadarrServiceSettingsAsync(HttpClient httpClient, ILogger<OverseerrClient> logger, OverseerrTestSettings settings)
        {
            var radarrServiceSettings = new RadarrServiceSettings();

            try
            {
                var response = await HttpGetAsync(httpClient, settings, $"{GetBaseURL(settings)}service/radarr");
                await response.ThrowIfNotSuccessfulAsync("Overseerr GetRadarrServiceSettings Failed", x => x.error);

                var responseString = await response.Content.ReadAsStringAsync();
                var downloadClient = JsonConvert.DeserializeObject<JSONDownloadClient[]>(responseString);

                foreach (var radarrClient in downloadClient)
                {
                    var radarrService = new RadarrService
                    {
                        Id = radarrClient.ID,
                        Name = radarrClient.Name
                    };

                    response = await HttpGetAsync(httpClient, settings, $"{GetBaseURL(settings)}service/radarr/{radarrClient.ID}");
                    await response.ThrowIfNotSuccessfulAsync("Overseerr GetRadarrDetails Failed", x => x.error);

                    responseString = await response.Content.ReadAsStringAsync();
                    var details = JsonConvert.DeserializeObject<JSONRadarrClientDetails>(responseString);

                    radarrService.Profiles = details.Profiles?.Select(x => new ServiceOption { Id = x.ID, Name = x.Name }).ToArray() ?? Array.Empty<ServiceOption>();
                    radarrService.Tags = details.Tags?.Select(x => new ServiceOption { Id = x.ID, Name = x.Label }).ToArray() ?? Array.Empty<ServiceOption>();
                    radarrService.RootPaths = details.RootFolders?.Select(x => new ServiceOption { Id = x.ID, Name = x.Path }).ToArray() ?? Array.Empty<ServiceOption>();

                    radarrServiceSettings.RadarrServices = radarrServiceSettings.RadarrServices.Concat(new[] { radarrService }).ToArray();
                }
            }
            catch (System.Exception ex)
            {
                logger.LogWarning(ex, "Error getting Overseerr radarr service settings" + ex.Message);
                throw;
            }

            return radarrServiceSettings;
        }

        public static async Task<SonarrServiceSettings> GetSonarrServiceSettingsAsync(HttpClient httpClient, ILogger<OverseerrClient> logger, OverseerrTestSettings settings)
        {
            var sonarrServiceSettings = new SonarrServiceSettings();

            try
            {
                var response = await HttpGetAsync(httpClient, settings, $"{GetBaseURL(settings)}service/sonarr");
                await response.ThrowIfNotSuccessfulAsync("Overseerr GetSonarrServiceSettings Failed", x => x.error);

                var responseString = await response.Content.ReadAsStringAsync();
                var downloadClient = JsonConvert.DeserializeObject<JSONDownloadClient[]>(responseString);

                foreach (var sonarrClient in downloadClient)
                {
                    var sonarrService = new SonarrService
                    {
                        Id = sonarrClient.ID,
                        Name = sonarrClient.Name
                    };

                    response = await HttpGetAsync(httpClient, settings, $"{GetBaseURL(settings)}service/sonarr/{sonarrClient.ID}");
                    await response.ThrowIfNotSuccessfulAsync("Overseerr GetSonarrDetails Failed", x => x.error);

                    responseString = await response.Content.ReadAsStringAsync();
                    var details = JsonConvert.DeserializeObject<JSONSonarrClientDetails>(responseString);

                    sonarrService.Profiles = details.Profiles?.Select(x => new ServiceOption { Id = x.ID, Name = x.Name }).ToArray() ?? Array.Empty<ServiceOption>();
                    sonarrService.Tags = details.Tags?.Select(x => new ServiceOption { Id = x.ID, Name = x.Label }).ToArray() ?? Array.Empty<ServiceOption>();
                    sonarrService.RootPaths = details.RootFolders?.Select(x => new ServiceOption { Id = x.ID, Name = x.Path }).ToArray() ?? Array.Empty<ServiceOption>();
                    sonarrService.LanguageProfiles = details.LanguageProfiles?.Select(x => new ServiceOption { Id = x.ID, Name = x.Name }).ToArray() ?? Array.Empty<ServiceOption>();

                    sonarrServiceSettings.SonarrServices = sonarrServiceSettings.SonarrServices.Concat(new[] { sonarrService }).ToArray();
                }
            }
            catch (System.Exception ex)
            {
                logger.LogWarning(ex, "Error getting Overseerr sonarr service settings" + ex.Message);
                throw;
            }

            return sonarrServiceSettings;
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
                        var movie = await SearchMovieAsync(new MovieRequest(null, int.MinValue), movieId);

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
                        var show = await GetTvShowDetailsAsync(new TvShowRequest(null, int.MinValue), showId);
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

        public async Task<IReadOnlyList<Movie>> SearchMovieAsync(MovieRequest request, string movieName)
        {
            try
            {
                var category = GetCurrentCategory(request, movieName);
                var response = await HttpGetAsync($"{BaseURL}search/?query={Uri.EscapeDataString(movieName)}&page=1&language=en");
                await response.ThrowIfNotSuccessfulAsync("OverseerrMovieSearch failed", x => x.error);

                var jsonResponse = await response.Content.ReadAsStringAsync();

                var movies = JsonConvert.DeserializeObject<JSONSearchResult>(jsonResponse).Results
                    .Where(x => x.MediaType == MediaTypes.MOVIE)
                    .ToArray();

                return movies.Select(x => ConvertMovie(x, category.Is4K ? x.MediaInfo?.Status4k : x.MediaInfo?.Status)).ToArray();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occurred while searching for movies from Overseerr: " + ex.Message);
                throw new System.Exception("An error occurred while searching for movies from Overseerr: " + ex.Message);
            }
        }


        /// <summary>
        /// This gets all the movies that match the name and are found in the internal library that are avalible
        /// </summary>
        /// <param name="request">Movie request</param>
        /// <param name="movieName">Name of the movie to look for</param>
        /// <returns>Returns the list of movies matching the name in library</returns>
        /// <exception cref="Exception">Returns error if Overseerr returns unexpected or no response</exception>
        public async Task<IReadOnlyList<Movie>> SearchMovieLibraryAsync(MovieRequest request, string movieName)
        {
            try
            {
                var category = GetCurrentCategory(request, movieName);
                var response = await HttpGetAsync($"{BaseURL}search/?query={Uri.EscapeDataString(movieName)}&page=1&language=en");
                await response.ThrowIfNotSuccessfulAsync("OverseerrMovieSearch failed", x => x.error);

                var jsonResponse = await response.Content.ReadAsStringAsync();

                var movies = JsonConvert.DeserializeObject<JSONSearchResult>(jsonResponse).Results
                    .Where(x => x.MediaType == MediaTypes.MOVIE)
                    .Where(x => x.MediaInfo != null)
                    .Where(x => x.MediaInfo.Status == MediaStatus.AVAILABLE || x.MediaInfo.Status4k == MediaStatus.AVAILABLE)
                    .ToArray();

                return movies.Select(x => ConvertMovie(x, category.Is4K ? x.MediaInfo?.Status4k : x.MediaInfo?.Status)).ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while searching for movies from Overseerr: " + ex.Message);
                throw new Exception("An error occurred while searching for movies from Overseerr: " + ex.Message);
            }
        }


        public async Task<Movie> SearchMovieLibraryAsync(MovieRequest request, int theMovieDbId)
        {
            try
            {
                var category = GetCurrentCategory(request, $"with TMDB Id {theMovieDbId}");
                var response = await HttpGetAsync($"{BaseURL}movie/{theMovieDbId}");
                await response.ThrowIfNotSuccessfulAsync("OverseerrMovieSearchByMovieDbId failed", x => x.error);

                var jsonResponse = await response.Content.ReadAsStringAsync();

                var movie = JsonConvert.DeserializeObject<JSONMedia>(jsonResponse);
                if(movie.MediaInfo == null)
                {
                    return null;
                }

                return ConvertMovie(movie, category.Is4K ? movie.MediaInfo?.Status4k : movie.MediaInfo?.Status);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while searching for a movie by tmdbId \"{theMovieDbId}\" from Overseerr: " + ex.Message);
                throw new System.Exception($"An error occurred while searching for a movie by tmdbId \"{theMovieDbId}\" from Overseerr: " + ex.Message);
            }
        }


        /// <summary>
        /// This handles the submitting of a movie with its issue and description to Overseerr
        /// </summary>
        /// <param name="theMovieDbId"></param>
        /// <param name="issueName"></param>
        /// <param name="issueDescription"></param>
        /// <returns></returns>
        public async Task<bool> SubmitMovieIssueAsync(int theMovieDbId, string issueName, string issueDescription)
        {
            try
            {
                HttpResponseMessage response = await HttpGetAsync($"{BaseURL}movie/{theMovieDbId}");
                await response.ThrowIfNotSuccessfulAsync("OverseerrMovieSearch failed", x => x.error);

                string jsonResponse = await response.Content.ReadAsStringAsync();
                JSONMedia movies = JsonConvert.DeserializeObject<JSONMedia>(jsonResponse);

                int interalMediaId = movies.MediaInfo.Id;
                int issueId = IssueTypes.IndexOf(issueName) + 1;

                response = await HttpPostAsync(null, $"{BaseURL}issue", JsonConvert.SerializeObject(new
                {
                    issueType = issueId,
                    message = issueDescription,
                    mediaId = interalMediaId
                }));

                await response.ThrowIfNotSuccessfulAsync("OverseerrRequestMovieRequest failed", x => x.error);
                return true;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred while searching for movies from Overseerr: " + ex.Message);
                return false;
            }
        }


        public async Task<Movie> SearchMovieAsync(MovieRequest request, int theMovieDbId)
        {
            try
            {
                var category = GetCurrentCategory(request, $"with TMDB Id {theMovieDbId}");
                var response = await HttpGetAsync($"{BaseURL}movie/{theMovieDbId}");
                await response.ThrowIfNotSuccessfulAsync("OverseerrMovieSearchByMovieDbId failed", x => x.error);

                var jsonResponse = await response.Content.ReadAsStringAsync();

                var movie = JsonConvert.DeserializeObject<JSONMedia>(jsonResponse);

                return ConvertMovie(movie, category.Is4K ? movie.MediaInfo?.Status4k : movie.MediaInfo?.Status);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while searching for a movie by tmdbId \"{theMovieDbId}\" from Overseerr: " + ex.Message);
                throw new System.Exception($"An error occurred while searching for a movie by tmdbId \"{theMovieDbId}\" from Overseerr: " + ex.Message);
            }
        }

        async Task<MovieRequestResult> IMovieRequester.RequestMovieAsync(MovieRequest request, Movie movie)
        {
            try
            {
                HttpResponseMessage response = null;
                var overseerrUser = await FindLinkedOverseerUserAsync(request.User.UserId, request.User.Username, OverseerrSettings.Movies.DefaultApiUserId);

                if (OverseerrSettings.Movies.Categories.Any())
                {
                    Permission[] permissions = new[] { Permission.AUTO_APPROVE, Permission.AUTO_APPROVE_MOVIE };

                    var category = GetCurrentCategory(request, movie.Title);

                    if (category.Is4K)
                    {
                        permissions = new[] { Permission.AUTO_APPROVE_4K, Permission.AUTO_APPROVE_4K_MOVIE };
                    }

                    if (overseerrUser != null)
                    {
                        response = await HttpGetAsync($"{BaseURL}user/{overseerrUser}/settings/permissions");
                        await response.ThrowIfNotSuccessfulAsync("OverseerrGetUserPermissions failed", x => x.error);
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        var userPermissions = JsonConvert.DeserializeObject<JSONUserPermissions>(jsonResponse);

                        if (HasPermission(permissions, userPermissions.Permissions, PermissionCheckOptions.OR))
                        {
                            response = await HttpPostAsync(null, $"{BaseURL}request", JsonConvert.SerializeObject(new
                            {
                                mediaId = int.Parse(movie.TheMovieDbId),
                                mediaType = "movie",
                                is4k = category.Is4K,
                                serverId = category.ServiceId,
                                profileId = category.ProfileId,
                                rootFolder = category.RootFolder,
                                tags = JToken.FromObject(category.Tags),
                                userId = int.Parse(overseerrUser),
                            }));
                        }
                        else
                        {
                            response = await HttpPostAsync(overseerrUser, $"{BaseURL}request", JsonConvert.SerializeObject(new
                            {
                                mediaId = int.Parse(movie.TheMovieDbId),
                                mediaType = "movie",
                                is4k = category.Is4K
                            }));

                            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                            {
                                return new MovieRequestResult
                                {
                                    WasDenied = true
                                };
                            }

                            jsonResponse = await response.Content.ReadAsStringAsync();
                            var overseerrRequest = JsonConvert.DeserializeObject<JSONRequest>(jsonResponse);

                            response = await HttpPutAsync(null, $"{BaseURL}request/{overseerrRequest.ID}", JsonConvert.SerializeObject(new
                            {
                                mediaType = "movie",
                                profileId = category.ProfileId,
                                rootFolder = category.RootFolder,
                                serverId = category.ServiceId,
                                tags = JToken.FromObject(category.Tags),
                                userId = int.Parse(overseerrUser),
                            }));
                        }
                    }
                    else
                    {
                        response = await HttpPostAsync(null, $"{BaseURL}request", JsonConvert.SerializeObject(new
                        {
                            mediaId = int.Parse(movie.TheMovieDbId),
                            mediaType = "movie",
                            is4k = category.Is4K,
                            serverId = category.ServiceId,
                            profileId = category.ProfileId,
                            rootFolder = category.RootFolder,
                            tags = JToken.FromObject(category.Tags)
                        }));
                    }
                }
                else
                {
                    response = await HttpPostAsync(overseerrUser, $"{BaseURL}request", JsonConvert.SerializeObject(new
                    {
                        mediaId = int.Parse(movie.TheMovieDbId),
                        mediaType = "movie",
                    }));
                }

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

        public Task<MovieDetails> GetMovieDetails(MovieRequest request, string theMovieDbId)
        {
            return TheMovieDb.GetMovieDetailsAsync(_httpClientFactory.CreateClient(), theMovieDbId, _logger);
        }

        async Task<IReadOnlyList<SearchedTvShow>> ITvShowSearcher.SearchTvShowAsync(TvShowRequest request, string tvShowName)
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

        public async Task<TvShow> GetTvShowDetailsAsync(TvShowRequest request, int theTvDbId)
        {
            try
            {
                var category = GetCurrentCategory(request, $"with TVDB id {theTvDbId}");
                var response = await HttpGetAsync($"{BaseURL}tv/{theTvDbId}");
                await response.ThrowIfNotSuccessfulAsync("OverseerrGetTvShowDetail failed", x => x.error);

                var jsonResponse = await response.Content.ReadAsStringAsync();

                var tvShow = JsonConvert.DeserializeObject<JSONMedia>(jsonResponse);

                return ConvertTvShow(tvShow, category.Is4K ? tvShow.MediaInfo?.Status4k : tvShow.MediaInfo?.Status);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while searching for a tv show by tmdbId \"{theTvDbId}\" from Overseerr: " + ex.Message);
                throw new System.Exception($"An error occurred while searching for a tv show by tmdbId \"{theTvDbId}\" from Overseerr: " + ex.Message);
            }
        }

        async Task<TvShowRequestResult> ITvShowRequester.RequestTvShowAsync(TvShowRequest request, TvShow tvShow, TvSeason season)
        {
            try
            {
                var wantedSeasonIds = season is AllTvSeasons
                    ? new HashSet<int>(tvShow.Seasons.OfType<NormalTvSeason>().Select(x => x.SeasonNumber))
                    : season is FutureTvSeasons
                        ? new HashSet<int>()
                        : new HashSet<int> { season.SeasonNumber };

                HttpResponseMessage response = null;
                var overseerrUser = await FindLinkedOverseerUserAsync(request.User.UserId, request.User.Username, OverseerrSettings.TvShows.DefaultApiUserId);

                if (OverseerrSettings.TvShows.Categories.Any())
                {
                    Permission[] permissions = new[] { Permission.AUTO_APPROVE, Permission.AUTO_APPROVE_TV };

                    var category = GetCurrentCategory(request, tvShow.Title);

                    if (category.Is4K)
                    {
                        permissions = new[] { Permission.AUTO_APPROVE_4K, Permission.AUTO_APPROVE_4K_TV };
                    }

                    if (overseerrUser != null)
                    {
                        response = await HttpGetAsync($"{BaseURL}user/{overseerrUser}/settings/permissions");
                        await response.ThrowIfNotSuccessfulAsync("OverseerrGetUserPermissions failed", x => x.error);
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        var userPermissions = JsonConvert.DeserializeObject<JSONUserPermissions>(jsonResponse);

                        if (HasPermission(permissions, userPermissions.Permissions, PermissionCheckOptions.OR))
                        {
                            response = await HttpPostAsync(null, $"{BaseURL}request", JsonConvert.SerializeObject(new
                            {
                                mediaId = tvShow.TheTvDbId,
                                mediaType = "tv",
                                seasons = wantedSeasonIds.ToArray(),
                                is4k = category.Is4K,
                                serverId = category.ServiceId,
                                profileId = category.ProfileId,
                                languageProfileId = category.LanguageProfileId,
                                rootFolder = category.RootFolder,
                                tags = JToken.FromObject(category.Tags),
                                userId = int.Parse(overseerrUser),
                            }));
                        }
                        else
                        {
                            response = await HttpPostAsync(overseerrUser, $"{BaseURL}request", JsonConvert.SerializeObject(new
                            {
                                mediaId = tvShow.TheTvDbId,
                                mediaType = "tv",
                                seasons = wantedSeasonIds.ToArray(),
                                is4k = category.Is4K
                            }));

                            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                            {
                                return new TvShowRequestResult
                                {
                                    WasDenied = true
                                };
                            }

                            jsonResponse = await response.Content.ReadAsStringAsync();
                            var overseerrRequest = JsonConvert.DeserializeObject<JSONRequest>(jsonResponse);

                            response = await HttpPutAsync(null, $"{BaseURL}request/{overseerrRequest.ID}", JsonConvert.SerializeObject(new
                            {
                                mediaType = "tv",
                                seasons = wantedSeasonIds.ToArray(),
                                profileId = category.ProfileId,
                                languageProfileId = category.LanguageProfileId,
                                rootFolder = category.RootFolder,
                                serverId = category.ServiceId,
                                tags = JToken.FromObject(category.Tags),
                                userId = int.Parse(overseerrUser),
                            }));
                        }
                    }
                    else
                    {
                        response = await HttpPostAsync(null, $"{BaseURL}request", JsonConvert.SerializeObject(new
                        {
                            mediaId = tvShow.TheTvDbId,
                            mediaType = "tv",
                            seasons = wantedSeasonIds.ToArray(),
                            is4k = category.Is4K,
                            serverId = category.ServiceId,
                            profileId = category.ProfileId,
                            languageProfileId = category.LanguageProfileId,
                            rootFolder = category.RootFolder,
                            tags = JToken.FromObject(category.Tags)
                        }));
                    }
                }
                else
                {
                    response = await HttpPostAsync(overseerrUser, $"{BaseURL}request", JsonConvert.SerializeObject(new
                    {
                        mediaId = tvShow.TheTvDbId,
                        mediaType = "tv",
                        seasons = wantedSeasonIds.ToArray(),
                    }));
                }

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

        Task<SearchedTvShow> ITvShowSearcher.SearchTvShowAsync(TvShowRequest request, int tvDbId)
        {
            throw new NotSupportedException("Overseerr does not support searching via TheTvDatabase.");
        }

        private async Task<string> FindLinkedOverseerUserAsync(string userId, string username, string defaultApiUserID)
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

            return !string.IsNullOrWhiteSpace(defaultApiUserID) && int.TryParse(defaultApiUserID, out var defaultUserId)
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

        private Movie ConvertMovie(JSONMedia jsonMedia, MediaStatus? mediaStatus)
        {
            return new Movie
            {
                TheMovieDbId = jsonMedia.Id.ToString(),
                Title = jsonMedia.Title,
                Available = mediaStatus == MediaStatus.AVAILABLE,
                Quality = string.Empty,
                Requested = mediaStatus == MediaStatus.PENDING
                    || mediaStatus == MediaStatus.PROCESSING
                    || mediaStatus == MediaStatus.PARTIALLY_AVAILABLE
                    || mediaStatus == MediaStatus.AVAILABLE,
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

        private TvShow ConvertTvShow(JSONMedia jsonMedia, MediaStatus? mediaInfo)
        {
            return new TvShow
            {
                TheTvDbId = jsonMedia.Id,
                Title = jsonMedia.Name,
                Banner = !string.IsNullOrWhiteSpace(jsonMedia.PosterPath) ? $"https://image.tmdb.org/t/p/w500{jsonMedia.PosterPath}" : null,
                FirstAired = jsonMedia.FirstAirDate,
                IsRequested = mediaInfo == MediaStatus.PENDING
                    || mediaInfo == MediaStatus.PROCESSING
                    || mediaInfo == MediaStatus.PARTIALLY_AVAILABLE
                    || mediaInfo == MediaStatus.AVAILABLE,
                Quality = string.Empty,
                WebsiteUrl = jsonMedia.TvdbId != null && jsonMedia.TvdbId.ToString() != "0" ? $"https://www.thetvdb.com/?id={jsonMedia.TvdbId}&tab=series" : null,
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
            return await HttpGetAsync(client, settings.ApiKey, url);
        }

        private static async Task<HttpResponseMessage> HttpGetAsync(HttpClient client, OverseerrTestSettings settings, string url)
        {
            return await HttpGetAsync(client, settings.ApiKey, url);
        }

        private static async Task<HttpResponseMessage> HttpGetAsync(HttpClient client, string apiKey, string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("X-Api-Key", apiKey);

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

        private async Task<HttpResponseMessage> HttpPutAsync(string overseerrUserId, string url, string content)
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
            return await client.PutAsync(url, postRequest);
        }

        private static string GetBaseURL(OverseerrTestSettings settings)
        {
            return GetBaseURL(settings.UseSSL, settings.Hostname, settings.Port, settings.Version);
        }

        private static string GetBaseURL(OverseerrSettings settings)
        {
            return GetBaseURL(settings.UseSSL, settings.Hostname, settings.Port, settings.Version);
        }

        private static string GetBaseURL(bool useSSl, string hostname, int port, string version)
        {
            var protocol = useSSl ? "https" : "http";
            return $"{protocol}://{hostname}:{port}/api/v{version}/";
        }

        private OverseerrMovieCategory GetCurrentCategory(MovieRequest request, string movieTitle)
        {
            try
            {
                if (OverseerrSettings.Movies.Categories.Any() && request.CategoryId != int.MinValue)
                    return OverseerrSettings.Movies.Categories.Single(x => x.Id == request.CategoryId);
            }
            catch
            {
                _logger.LogError($"An error occurred while requesting movie \"{movieTitle}\" from Overseerr, could not find category with id {request.CategoryId}");
                throw new Exception($"An error occurred while requesting movie \"{movieTitle}\" from Overseerr, could not find category with id {request.CategoryId}");
            }

            return DefaultMovieCategory;
        }

        private OverseerrTvShowCategory GetCurrentCategory(TvShowRequest request, string tvShowTitle)
        {
            try
            {
                if (OverseerrSettings.TvShows.Categories.Any() && request.CategoryId != int.MinValue)
                    return OverseerrSettings.TvShows.Categories.Single(x => x.Id == request.CategoryId);
            }
            catch
            {
                _logger.LogError($"An error occurred while requesting a tv show \"{tvShowTitle}\" from Overseerr, could not find category with id {request.CategoryId}");
                throw new Exception($"An error occurred while requesting a tv show \"{tvShowTitle}\" from Overseerr, could not find category with id {request.CategoryId}");
            }

            return DefaultTvShowCategory;
        }

        public class JSONDownloadClient
        {
            [JsonProperty("id")]
            public int ID { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
        }

        public class JSONSonarrClientDetails
        {
            [JsonProperty("profiles")]
            public JSONProfile[]? Profiles { get; set; }

            [JsonProperty("rootFolders")]
            public JSONRootFolder[]? RootFolders { get; set; }

            [JsonProperty("tags")]
            public JSONTags[]? Tags { get; set; }

            [JsonProperty("languageProfiles")]
            public JSONLanguageProfile[]? LanguageProfiles { get; set; }
        }

        public class JSONRadarrClientDetails
        {
            [JsonProperty("profiles")]
            public JSONProfile[]? Profiles { get; set; }

            [JsonProperty("rootFolders")]
            public JSONRootFolder[]? RootFolders { get; set; }

            [JsonProperty("tags")]
            public JSONTags[]? Tags { get; set; }
        }

        public class JSONProfile
        {
            [JsonProperty("id")]
            public int ID { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
        }

        public class JSONTags
        {
            [JsonProperty("id")]
            public int ID { get; set; }

            [JsonProperty("label")]
            public string Label { get; set; }
        }

        public class JSONRootFolder
        {
            [JsonProperty("id")]
            public int ID { get; set; }

            [JsonProperty("path")]
            public string Path { get; set; }
        }

        public class JSONLanguageProfile
        {
            [JsonProperty("id")]
            public int ID { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
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

        public class JSONExternalIds
        {
            [JsonProperty("tvdbId")]
            public object TvdbId { get; set; }
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
            [JsonProperty("id")]
            public int ID { get; set; }

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

            public MediaStatus Status4k
            {
                get
                {
                    return (MediaStatus)Status4kValue;
                }
            }

            [JsonProperty("status4k")]
            public int Status4kValue { get; set; }

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

            public object TvdbId
            {
                get
                {
                    return MediaInfo?.TvdbId != null ? MediaInfo.TvdbId : ExternalIds?.TvdbId != null ? ExternalIds.TvdbId : 0;
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

            [JsonProperty("externalIds")]
            public JSONExternalIds ExternalIds { get; set; }

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

        public class JSONUserPermissions
        {
            [JsonProperty("permissions")]
            public int Permissions { get; set; }
        }

        private enum Permission
        {
            NONE = 0,
            ADMIN = 2,
            MANAGE_SETTINGS = 4,
            MANAGE_USERS = 8,
            MANAGE_REQUESTS = 16,
            REQUEST = 32,
            VOTE = 64,
            AUTO_APPROVE = 128,
            AUTO_APPROVE_MOVIE = 256,
            AUTO_APPROVE_TV = 512,
            REQUEST_4K = 1024,
            REQUEST_4K_MOVIE = 2048,
            REQUEST_4K_TV = 4096,
            REQUEST_ADVANCED = 8192,
            REQUEST_VIEW = 16384,
            AUTO_APPROVE_4K = 32768,
            AUTO_APPROVE_4K_MOVIE = 65536,
            AUTO_APPROVE_4K_TV = 131072,
            REQUEST_MOVIE = 262144,
            REQUEST_TV = 524288,
        }

        private enum PermissionCheckOptions
        {
            AND,
            OR
        }

        private static bool HasPermission(Permission[] permissions, int value, PermissionCheckOptions options = PermissionCheckOptions.AND)
        {
            int total = 0;

            if (!permissions.Any())
            {
                return true;
            }

            if ((value & (int)Permission.ADMIN) != 0)
            {
                return true;
            }

            switch (options)
            {
                case PermissionCheckOptions.AND:
                    return permissions.All((permission) => !!((value & (int)permission) != 0));
                case PermissionCheckOptions.OR:
                    return permissions.Any((permission) => !!((value & (int)permission) != 0));
            }

            return !!((value & (int)Permission.ADMIN) != 0) || !!((value & total) != 0);
        }
    }
}