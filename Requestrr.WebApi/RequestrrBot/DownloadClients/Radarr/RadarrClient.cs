using System;
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

namespace Requestrr.WebApi.RequestrrBot.DownloadClients.Radarr
{
    public class RadarrClient : IMovieRequester, IMovieSearcher
    {
        private IHttpClientFactory _httpClientFactory;
        private readonly ILogger<RadarrClient> _logger;
        private RadarrSettingsProvider _RadarrSettingsProvider;
        private RadarrSettings RadarrSettings => _RadarrSettingsProvider.Provide();
        private string BaseURL => GetBaseURL(RadarrSettings);

        public RadarrClient(IHttpClientFactory httpClientFactory, ILogger<RadarrClient> logger, RadarrSettingsProvider RadarrSettingsProvider)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _RadarrSettingsProvider = RadarrSettingsProvider;
        }

        public static async Task TestConnectionAsync(HttpClient httpClient, ILogger<RadarrClient> logger, RadarrSettings settings)
        {
            if (!string.IsNullOrWhiteSpace(settings.BaseUrl) && !settings.BaseUrl.StartsWith("/"))
            {
                throw new Exception("Invalid base URL, must start with /");
            }

            var testSuccessful = false;

            try
            {
                var response = await HttpGetAsync(httpClient, settings, $"{GetBaseURL(settings)}/config/host");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new Exception("Invalid api key");
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new Exception("Incorrect api version");
                }

                try
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    dynamic jsonResponse = JObject.Parse(responseString);

                    if (!jsonResponse.urlBase.ToString().Equals(settings.BaseUrl, StringComparison.InvariantCultureIgnoreCase))
                    {
                        throw new Exception("Base url does not match what is set in Radarr");
                    }
                }
                catch
                {
                    throw new Exception("Base url does not match what is set in Radarr");
                }

                testSuccessful = true;
            }
            catch (HttpRequestException ex)
            {
                logger.LogWarning(ex, "Error while testing Radarr connection: " + ex.Message);
                throw new Exception("Invalid host and/or port");
            }
            catch (System.Exception ex)
            {
                logger.LogWarning(ex, "Error while testing Radarr connection: " + ex.Message);

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

        public static async Task<IList<JSONRootPath>> GetRootPaths(HttpClient httpClient, ILogger<RadarrClient> logger, RadarrSettings settings)
        {
            var retryCount = 0;

            while (retryCount <= 5)
            {
                try
                {
                    var response = await HttpGetAsync(httpClient, settings, $"{GetBaseURL(settings)}/rootfolder");

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<IList<JSONRootPath>>(jsonResponse);
                }
                catch (System.Exception ex)
                {
                    logger.LogWarning(ex, "An error while getting Radarr root paths: " + ex.Message);
                    retryCount++;
                    await Task.Delay(1000);
                }
            }

            throw new System.Exception("An error occurred while getting Radarr root paths");
        }

        public static async Task<IList<JSONProfile>> GetProfiles(HttpClient httpClient, ILogger<RadarrClient> logger, RadarrSettings settings)
        {
            var retryCount = 0;

            while (retryCount <= 5)
            {
                try
                {
                    var profileUri = settings.Version == "2" ? "/profile" : $"/qualityprofile";
                    var response = await HttpGetAsync(httpClient, settings, $"{GetBaseURL(settings)}{profileUri}");

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<IList<JSONProfile>>(jsonResponse);
                }
                catch (System.Exception ex)
                {
                    logger.LogWarning(ex, "An error while getting Radarr profiles: " + ex.Message);
                    retryCount++;
                    await Task.Delay(1000);
                }
            }

            throw new System.Exception("An error occurred while getting Radarr profiles");
        }

        public static async Task<IList<JSONTag>> GetTags(HttpClient httpClient, ILogger<RadarrClient> logger, RadarrSettings settings)
        {
            var retryCount = 0;

            while (retryCount <= 5)
            {
                try
                {
                    var response = await HttpGetAsync(httpClient, settings, $"{GetBaseURL(settings)}/tag");
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<IList<JSONTag>>(jsonResponse);
                }
                catch (System.Exception ex)
                {
                    logger.LogWarning(ex, "An error while getting Radarr tags: " + ex.Message);
                    retryCount++;
                    await Task.Delay(1000);
                }
            }

            throw new System.Exception("An error occurred while getting Radarr tags");
        }

        public async Task<Movie> SearchMovieAsync(int theMovieDbId)
        {
            var retryCount = 0;

            while (retryCount <= 5)
            {
                try
                {
                    var response = await HttpGetAsync($"{BaseURL}/movie");
                    await response.ThrowIfNotSuccessfulAsync("RadarrGetAllMovies failed", x => x.error);

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var allRadarrMoviesJson = JsonConvert.DeserializeObject<List<JSONMovie>>(jsonResponse).ToArray();

                    var foundMovieJson = allRadarrMoviesJson.FirstOrDefault(x => x.tmdbId == theMovieDbId);

                    response = await HttpGetAsync($"{BaseURL}/movie/lookup/tmdb?tmdbId={theMovieDbId}");
                    await response.ThrowIfNotSuccessfulAsync("RadarrMovieLookupByTmdbId failed", x => x.error);

                    jsonResponse = await response.Content.ReadAsStringAsync();
                    var movieFoundByIdJson = JsonConvert.DeserializeObject<JSONMovie>(jsonResponse);

                    if (foundMovieJson != null)
                    {
                        if (movieFoundByIdJson.tmdbId == theMovieDbId)
                        {
                            foundMovieJson.images = movieFoundByIdJson.images;
                        }
                    }
                    else
                    {
                        foundMovieJson = movieFoundByIdJson;
                    }

                    return foundMovieJson != null ? Convert(foundMovieJson) : null;
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, $"An error occurred while searching for a movie by tmdbId \"{theMovieDbId}\" with Radarr: " + ex.Message);
                    retryCount++;
                    await Task.Delay(1000);
                }
            }

            throw new System.Exception("An error occurred while searching for a movie by tmdbId with Radarr");
        }

        public async Task<IReadOnlyList<Movie>> SearchMovieAsync(string movieName)
        {
            var retryCount = 0;

            while (retryCount <= 5)
            {
                try
                {
                    var searchTerm = movieName.ToLower().Trim().Replace(" ", "+");
                    var response = await HttpGetAsync($"{BaseURL}/movie/lookup?term={searchTerm}");
                    await response.ThrowIfNotSuccessfulAsync("RadarrMovieLookup failed", x => x.error);

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var jsonMovies = JsonConvert.DeserializeObject<List<JSONMovie>>(jsonResponse).ToArray();

                    return jsonMovies.Select(x => Convert(x)).ToArray();
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while searching for movies with Radarr: " + ex.Message);
                    retryCount++;
                    await Task.Delay(1000);
                }
            }

            throw new System.Exception("An error occurred while searching for movies with Radarr");
        }

        public Task<MovieDetails> GetMovieDetails(string theMovieDbId)
        {
            return TheMovieDb.GetMovieDetailsAsync(_httpClientFactory.CreateClient(), theMovieDbId, _logger);
        }

        public async Task<Dictionary<int, Movie>> SearchAvailableMoviesAsync(HashSet<int> theMovieDbIds, System.Threading.CancellationToken token)
        {
            var retryCount = 0;

            while (retryCount <= 5 && !token.IsCancellationRequested)
            {
                try
                {
                    var response = await HttpGetAsync($"{BaseURL}/movie");
                    await response.ThrowIfNotSuccessfulAsync("RadarrGetAllMovies failed", x => x.error);

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var jsonMovies = JsonConvert.DeserializeObject<List<JSONMovie>>(jsonResponse).ToArray();

                    var convertedMovies = new List<Movie>();

                    foreach (var movie in jsonMovies.Where(x => theMovieDbIds.Contains(x.tmdbId)))
                    {
                        try
                        {
                            movie.images = await GetImagesAsync(movie.tmdbId);
                        }
                        catch
                        {
                            // Ignore
                        }

                        convertedMovies.Add(Convert(movie));
                    }

                    return convertedMovies.Where(x => x.Available).ToDictionary(x => int.Parse(x.TheMovieDbId), x => x);
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while searching available movies with Radarr: " + ex.Message);
                    retryCount++;
                    await Task.Delay(1000, token);
                }
            }

            throw new System.Exception("An error occurred while searching available movies with Radarr");
        }

        public async Task<MovieRequestResult> RequestMovieAsync(MovieUserRequester requester, Movie movie)
        {
            var retryCount = 0;

            while (retryCount <= 5)
            {
                try
                {
                    if (string.IsNullOrEmpty(movie.DownloadClientId))
                    {
                        await CreateMovieInRadarr(movie);
                    }
                    else
                    {
                        await UpdateExistingMovie(movie);
                    }

                    return new MovieRequestResult();
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, $"An error while requesting movie \"{movie.Title}\" from Radarr: " + ex.Message);
                    retryCount++;
                    await Task.Delay(1000);
                }
            }

            throw new System.Exception("An error occurred while requesting a movie from Radarr");
        }

        private async Task CreateMovieInRadarr(Movie movie)
        {
            var isAnime = false;

            try
            {
                var movieDbResponse = await TheMovieDb.GetMovieFromTheMovieDbAsync(_httpClientFactory.CreateClient(), movie.TheMovieDbId);
                await movieDbResponse.ThrowIfNotSuccessfulAsync("TheMovieDbFindMovie failed", x => x.status_message);
                var jsonResponse = await movieDbResponse.Content.ReadAsStringAsync();
                var theMovieDbMovie = JsonConvert.DeserializeObject<TheMovieDbMovie>(jsonResponse);

                isAnime = theMovieDbMovie.IsAnime;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while requesting movie: " + ex.Message);
            }

            var jsonMovie = await SearchMovieByMovieDbId(int.Parse(movie.TheMovieDbId));

            int[] tags = Array.Empty<int>();

            if (RadarrSettings.Version != "2")
            {
                tags = isAnime ? RadarrSettings.AnimeTags : RadarrSettings.MovieTags;
            }

            var response = await HttpPostAsync($"{BaseURL}/movie", JsonConvert.SerializeObject(new
            {
                title = jsonMovie.title,
                qualityProfileId = isAnime ? RadarrSettings.AnimeProfileId : RadarrSettings.MovieProfileId,
                titleSlug = jsonMovie.titleSlug,
                monitored = RadarrSettings.MonitorNewRequests,
                tags = tags,
                images = new string[0],
                tmdbId = int.Parse(movie.TheMovieDbId),
                year = jsonMovie.year,
                rootFolderPath = isAnime ? RadarrSettings.AnimeRootFolder : RadarrSettings.MovieRootFolder,
                minimumAvailability = isAnime ? RadarrSettings.AnimeMinimumAvailability : RadarrSettings.MovieMinimumAvailability,
                addOptions = new
                {
                    ignoreEpisodesWithFiles = false,
                    ignoreEpisodesWithoutFiles = false,
                    searchForMovie = RadarrSettings.SearchNewRequests
                }
            }));

            await response.ThrowIfNotSuccessfulAsync("RadarrMovieCreation failed", x => x.error);
        }

        private async Task UpdateExistingMovie(Movie movie)
        {
            var isAnime = false;
            var radarrMovieId = int.Parse(movie.DownloadClientId);
            var response = await HttpGetAsync($"{BaseURL}/movie/{radarrMovieId}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await CreateMovieInRadarr(movie);
                    return;
                }

                await response.ThrowIfNotSuccessfulAsync("RadarrGetMovie failed", x => x.error);
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            dynamic radarrMovie = JObject.Parse(jsonResponse);

            try
            {
                var movieDbResponse = await TheMovieDb.GetMovieFromTheMovieDbAsync(_httpClientFactory.CreateClient(), movie.TheMovieDbId);
                await movieDbResponse.ThrowIfNotSuccessfulAsync("TheMovieDbFindMovie failed", x => x.status_message);
                var movieDbJsonResponse = await movieDbResponse.Content.ReadAsStringAsync();
                var theMovieDbMovie = JsonConvert.DeserializeObject<TheMovieDbMovie>(movieDbJsonResponse);

                isAnime = theMovieDbMovie.IsAnime;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while requesting movie: " + ex.Message);
            }

            if (RadarrSettings.Version == "2")
            {
                radarrMovie.profileId = isAnime ? RadarrSettings.AnimeProfileId : RadarrSettings.MovieProfileId;
            }
            else
            {
                radarrMovie.tags = JToken.FromObject(isAnime ? RadarrSettings.AnimeTags : RadarrSettings.MovieTags);
                radarrMovie.qualityProfileId = isAnime ? RadarrSettings.AnimeProfileId : RadarrSettings.MovieProfileId;
            }

            radarrMovie.minimumAvailability = isAnime ? RadarrSettings.AnimeMinimumAvailability : RadarrSettings.MovieMinimumAvailability;
            radarrMovie.monitored = RadarrSettings.MonitorNewRequests;

            response = await HttpPutAsync($"{BaseURL}/movie/{radarrMovieId}", JsonConvert.SerializeObject(radarrMovie));

            await response.ThrowIfNotSuccessfulAsync("RadarrUpdateMovie failed", x => x.error);

            if (RadarrSettings.SearchNewRequests)
            {
                try
                {
                    response = await HttpPostAsync($"{BaseURL}/command", JsonConvert.SerializeObject(new
                    {
                        name = "moviesSearch",
                        movieIds = new[] { radarrMovieId },
                    }));

                    await response.ThrowIfNotSuccessfulAsync("RadarrMovieSearchCommand failed", x => x.error);
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, $"An error while sending search command for movie \"{movie.Title}\" to Radarr: " + ex.Message);
                }
            }
        }

        private Movie Convert(JSONMovie jsonMovie)
        {
            var isDownloaded = jsonMovie.downloaded.HasValue ? jsonMovie.downloaded.Value : jsonMovie.movieFile != null;
            var isMonitored = jsonMovie.monitored;

            var downloadClientId = jsonMovie.id?.ToString();

            return new Movie
            {
                DownloadClientId = downloadClientId,
                Title = jsonMovie.title,
                Available = isDownloaded,
                Overview = jsonMovie.overview,
                TheMovieDbId = jsonMovie.tmdbId.ToString(),
                Quality = "",
                Requested = !isDownloaded && (string.IsNullOrWhiteSpace(downloadClientId) || RadarrSettings.MonitorNewRequests) ? isMonitored : true,
                PlexUrl = "",
                EmbyUrl = "",
                PosterPath = jsonMovie.images.Where(x => x.coverType.Equals("poster", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault()?.url ?? string.Empty,
                ReleaseDate = jsonMovie.inCinemas,
            };
        }

        private async Task<List<JSONImage>> GetImagesAsync(int movieDbId)
        {
            try
            {
                await Task.Delay(100);
                var jsonMovie = await SearchMovieByMovieDbId(movieDbId);

                return jsonMovie.images.ToList();
            }
            catch
            {
                return new List<JSONImage>();
            }
        }

        private async Task<JSONMovie> SearchMovieByMovieDbId(int movieId)
        {
            var response = await HttpGetAsync($"{BaseURL}/movie/lookup/tmdb?tmdbId={movieId}");
            await response.ThrowIfNotSuccessfulAsync("RadarrMovieLookupTmDb failed", x => x.error);

            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<JSONMovie>(jsonResponse);
        }

        private Task<HttpResponseMessage> HttpGetAsync(string url)
        {
            return HttpGetAsync(_httpClientFactory.CreateClient(), RadarrSettings, url);
        }

        private static async Task<HttpResponseMessage> HttpGetAsync(HttpClient client, RadarrSettings settings, string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("X-Api-Key", settings.ApiKey);

            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                return await client.SendAsync(request, cts.Token);
            }
        }

        private async Task<HttpResponseMessage> HttpPostAsync(string url, string content)
        {
            var postRequest = new StringContent(content);
            postRequest.Headers.Clear();
            postRequest.Headers.Add("Content-Type", "application/json");
            postRequest.Headers.Add("X-Api-Key", RadarrSettings.ApiKey);

            var client = _httpClientFactory.CreateClient();
            return await client.PostAsync(url, postRequest);
        }

        private async Task<HttpResponseMessage> HttpPutAsync(string url, string content)
        {
            var putRequest = new StringContent(content);
            putRequest.Headers.Clear();
            putRequest.Headers.Add("Content-Type", "application/json");
            putRequest.Headers.Add("X-Api-Key", RadarrSettings.ApiKey);

            var client = _httpClientFactory.CreateClient();
            return await client.PutAsync(url, putRequest);
        }

        private static string GetBaseURL(RadarrSettings settings)
        {
            var version = settings.Version == "2" ? string.Empty : $"/v{settings.Version}";
            var protocol = settings.UseSSL ? "https" : "http";

            return $"{protocol}://{settings.Hostname}:{settings.Port}{settings.BaseUrl}/api{version}";
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

        public class JSONMovieFile
        {
            public int id { get; set; }
        }

        public class JSONImage
        {
            public string coverType { get; set; }
            public string url { get; set; }
        }

        public class JSONMovie
        {
            public int? id { get; set; }
            public string title { get; set; }
            public string overview { get; set; }
            public string inCinemas { get; set; }
            public List<JSONImage> images { get; set; }
            public bool? downloaded { get; set; }
            public int year { get; set; }
            public bool monitored { get; set; }
            public int tmdbId { get; set; }
            public string titleSlug { get; set; }
            public JSONMovieFile movieFile { get; set; }
        }

        private class TheMovieDbMovieGenre
        {
            public string name { get; set; }
        }

        private class TheMovieDbMovie
        {
            public string original_language { get; set; }
            public TheMovieDbMovieGenre[] genres { get; set; }
            public bool IsAnime => original_language.Equals("ja", StringComparison.InvariantCultureIgnoreCase) && (genres?.Any(x => x.name.Equals("animation", StringComparison.InvariantCultureIgnoreCase)) ?? false);
        }
    }
}