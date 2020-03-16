using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Requestrr.WebApi.Extensions;
using Requestrr.WebApi.RequestrrBot.Movies;

namespace Requestrr.WebApi.RequestrrBot.DownloadClients.Radarr
{
    public class Radarr : IMovieRequester, IMovieSearcher
    {
        private IHttpClientFactory _httpClientFactory;
        private readonly ILogger<Radarr> _logger;
        private RadarrSettingsProvider _RadarrSettingsProvider;
        private RadarrSettings RadarrSettings => _RadarrSettingsProvider.Provide();
        private string BaseURL => GetBaseURL(RadarrSettings);

        public Radarr(IHttpClientFactory httpClientFactory, ILogger<Radarr> logger, RadarrSettingsProvider RadarrSettingsProvider)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _RadarrSettingsProvider = RadarrSettingsProvider;
        }

        public static async Task TestConnectionAsync(HttpClient httpClient, ILogger<Radarr> logger, RadarrSettings settings)
        {
            var testSuccessful = false;

            try
            {
                var response = await HttpGetAsync(httpClient, settings, $"{GetBaseURL(settings)}/config/host");
                dynamic jsonResponse = JObject.Parse(await response.Content.ReadAsStringAsync());
                testSuccessful = jsonResponse.urlBase.ToString().Equals(settings.BaseUrl, StringComparison.InvariantCultureIgnoreCase);
            }
            catch (System.Exception ex)
            {
                logger.LogWarning("Error while testing Radarr connection: " + ex.Message);
                throw new Exception("Could not connect to Radarr");
            }

            if (!testSuccessful)
            {
                throw new Exception("Could not connect to Radarr");
            }
        }

        public static async Task<IList<JSONRootPath>> GetRootPaths(HttpClient httpClient, ILogger<Radarr> logger, RadarrSettings settings)
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
                    logger.LogWarning("An error while getting Radarr root paths: " + ex.Message);
                    retryCount++;
                    await Task.Delay(1000);
                }
            }

            throw new System.Exception("An error occurred while getting Radarr root paths");
        }

        public static async Task<IList<JSONProfile>> GetProfiles(HttpClient httpClient, ILogger<Radarr> logger, RadarrSettings settings)
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
                    logger.LogWarning("An error while getting Radarr profiles: " + ex.Message);
                    retryCount++;
                    await Task.Delay(1000);
                }
            }

            throw new System.Exception("An error occurred while getting Radarr profiles");
        }

        public static async Task<IList<JSONTag>> GetTags(HttpClient httpClient, ILogger<Radarr> logger, RadarrSettings settings)
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
                    logger.LogWarning("An error while getting Radarr tags: " + ex.Message);
                    retryCount++;
                    await Task.Delay(1000);
                }
            }

            throw new System.Exception("An error occurred while getting Radarr tags");
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
                    _logger.LogWarning("An error occurred while searching for movies with Radarr: " + ex.Message);
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
                        movie.images = await GetImagesAsync(movie.tmdbId);
                        convertedMovies.Add(Convert(movie));
                    }

                    return convertedMovies.Where(x => x.Available).ToDictionary(x => int.Parse(x.TheMovieDbId), x => x);
                }
                catch (System.Exception ex)
                {
                    _logger.LogWarning("An error occurred while searching available movies with Radarr: " + ex.Message);
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
                    _logger.LogWarning($"An error while requesting movie \"{movie.Title}\" from Radarr: " + ex.Message);
                    retryCount++;
                    await Task.Delay(1000);
                }
            }

            throw new System.Exception("An error occurred while requesting a movie from Radarr");
        }

        private async Task CreateMovieInRadarr(Movie movie)
        {
            var response = await TheMovieDb.GetMovieFromTheMovieDbAsync(_httpClientFactory.CreateClient(), movie.TheMovieDbId);
            await response.ThrowIfNotSuccessfulAsync("TheMovieDbFindMovie failed", x => x.status_message);

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var theMovieDbMovie = JsonConvert.DeserializeObject<TheMovieDbMovie>(jsonResponse);

            var jsonMovie = await SearchMovieByMovieDbId(int.Parse(movie.TheMovieDbId));

            int[] tags = Array.Empty<int>();

            if (RadarrSettings.Version != "2")
            {
                tags = theMovieDbMovie.IsAnime ? RadarrSettings.AnimeTags : RadarrSettings.MovieTags;
            }

            response = await HttpPostAsync($"{BaseURL}/movie", JsonConvert.SerializeObject(new
            {
                title = jsonMovie.title,
                qualityProfileId = theMovieDbMovie.IsAnime ? RadarrSettings.AnimeProfileId : RadarrSettings.MovieProfileId,
                titleSlug = jsonMovie.titleSlug,
                monitored = RadarrSettings.MonitorNewRequests,
                tags = tags,
                images = new string[0],
                tmdbId = int.Parse(movie.TheMovieDbId),
                year = jsonMovie.year,
                rootFolderPath = theMovieDbMovie.IsAnime ? RadarrSettings.AnimeRootFolder : RadarrSettings.MovieRootFolder,
                minimumAvailability = theMovieDbMovie.IsAnime ? RadarrSettings.AnimeMinimumAvailability : RadarrSettings.MovieMinimumAvailability,
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

            response = await TheMovieDb.GetMovieFromTheMovieDbAsync(_httpClientFactory.CreateClient(), movie.TheMovieDbId);
            await response.ThrowIfNotSuccessfulAsync("TheMovieDbFindMovie failed", x => x.status_message);

            jsonResponse = await response.Content.ReadAsStringAsync();
            var theMovieDbMovie = JsonConvert.DeserializeObject<TheMovieDbMovie>(jsonResponse);

            if (RadarrSettings.Version == "2")
            {
                radarrMovie.profileId = theMovieDbMovie.IsAnime ? RadarrSettings.AnimeProfileId : RadarrSettings.MovieProfileId;
            }
            else
            {
                radarrMovie.tags = JToken.FromObject(theMovieDbMovie.IsAnime ? RadarrSettings.AnimeTags : RadarrSettings.MovieTags);
                radarrMovie.qualityProfileId = theMovieDbMovie.IsAnime ? RadarrSettings.AnimeProfileId : RadarrSettings.MovieProfileId;
            }

            radarrMovie.minimumAvailability = theMovieDbMovie.IsAnime ? RadarrSettings.AnimeMinimumAvailability : RadarrSettings.MovieMinimumAvailability;
            radarrMovie.monitored = true;

            response = await HttpPutAsync($"{BaseURL}/movie/{radarrMovieId}", JsonConvert.SerializeObject(radarrMovie));

            await response.ThrowIfNotSuccessfulAsync("RadarrUpdateMovie failed", x => x.error);

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
                _logger.LogWarning($"An error while sending search command for movie \"{movie.Title}\" to Radarr: " + ex.Message);
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
                Approved = false,
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

            return await client.SendAsync(request);
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