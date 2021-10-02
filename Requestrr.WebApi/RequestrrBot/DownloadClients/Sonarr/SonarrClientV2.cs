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
using Requestrr.WebApi.RequestrrBot.TvShows;
using static Requestrr.WebApi.RequestrrBot.DownloadClients.Sonarr.SonarrClient;

namespace Requestrr.WebApi.RequestrrBot.DownloadClients.Sonarr
{
    public class SonarrClientV2 : ITvShowSearcher, ITvShowRequester
    {
        private IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SonarrClient> _logger;
        private SonarrSettingsProvider _sonarrSettingsProvider;
        private SonarrSettings SonarrSettings => _sonarrSettingsProvider.Provide();
        private string BaseURL => GetBaseURL(SonarrSettings);
        private object _lock = new object();
        private bool _loadedCache = false;
        private string _cachedEndpoint = string.Empty;
        private Dictionary<int, int> _tvDbToSonarrId = new Dictionary<int, int>();

        public SonarrClientV2(IHttpClientFactory httpClientFactory, ILogger<SonarrClient> logger, SonarrSettingsProvider sonarrSettingsProvider)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _sonarrSettingsProvider = sonarrSettingsProvider;
        }

        public static async Task TestConnectionAsync(HttpClient httpClient, ILogger<SonarrClient> logger, SonarrSettings settings)
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
                        throw new Exception("Base url does not match what is set in Sonarr");
                    }
                }
                catch
                {
                    throw new Exception("Base url does not match what is set in Sonarr");
                }

                testSuccessful = true;
            }
            catch (HttpRequestException ex)
            {
                logger.LogWarning(ex, "Error while testing Sonarr connection: " + ex.Message);
                throw new Exception("Invalid host and/or port");
            }
            catch (System.Exception ex)
            {
                logger.LogWarning(ex, "Error while testing Sonarr connection: " + ex.Message);

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

        public static async Task<IList<JSONRootPath>> GetRootPaths(HttpClient httpClient, ILogger<SonarrClient> logger, SonarrSettings settings)
        {
            try
            {
                var response = await HttpGetAsync(httpClient, settings, $"{GetBaseURL(settings)}/rootfolder");
                var jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<IList<JSONRootPath>>(jsonResponse);
            }
            catch (System.Exception ex)
            {
                logger.LogWarning(ex, "An error while getting Sonarr root paths: " + ex.Message);
            }

            throw new System.Exception("An error occurred while getting Sonarr root paths");
        }

        public static async Task<IList<JSONProfile>> GetProfiles(HttpClient httpClient, ILogger<SonarrClient> logger, SonarrSettings settings)
        {
            try
            {
                var response = await HttpGetAsync(httpClient, settings, $"{GetBaseURL(settings)}/qualityprofile");
                var jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<IList<JSONProfile>>(jsonResponse);
            }
            catch (System.Exception ex)
            {
                logger.LogWarning(ex, "An error while getting Sonarr profiles: " + ex.Message);
            }

            throw new System.Exception("An error occurred while getting Sonarr profiles");
        }

        public async Task<SearchedTvShow> SearchTvShowAsync(int tvDbId)
        {
            RefreshSonarrCache();

            try
            {
                var jsonTvShow = await SearchSerieByTvDbIdAsync(tvDbId);

                var searchedTvShow = new SearchedTvShow
                {
                    TheTvDbId = jsonTvShow.tvdbId.Value,
                    Title = jsonTvShow.title,
                    FirstAired = jsonTvShow.year > 0 ? jsonTvShow.year.ToString() : string.Empty,
                    Banner = jsonTvShow.remotePoster
                };

                return searchedTvShow.TheTvDbId == tvDbId ? searchedTvShow : null;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while searching for tv show by tvDbId \"{tvDbId}\" from Sonarr: " + ex.Message);
            }

            throw new System.Exception("An error occurred while searching for tv show by tvDbId from Sonarr");
        }

        public async Task<IReadOnlyList<SearchedTvShow>> SearchTvShowAsync(string tvShowName)
        {
            RefreshSonarrCache();

            try
            {
                var searchTerm = Uri.EscapeDataString(tvShowName.ToLower().Trim().Replace(" ", "+"));
                var response = await HttpGetAsync($"{BaseURL}/series/lookup?term={searchTerm}");
                await response.ThrowIfNotSuccessfulAsync("SonarrSeriesLookup failed", x => x.message);
                var jsonResponse = await response.Content.ReadAsStringAsync();

                var jsonTvShows = JsonConvert.DeserializeObject<List<JSONTvShow>>(jsonResponse).Where(x => x.tvdbId.HasValue).ToArray();

                return jsonTvShows.Select(x => new SearchedTvShow
                {
                    TheTvDbId = x.tvdbId.Value,
                    Title = x.title,
                    FirstAired = x.year > 0 ? x.year.ToString() : string.Empty,
                    Banner = x.remotePoster
                }).ToArray();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while searching for tv show \"{tvShowName}\" from Sonarr: " + ex.Message);
            }

            throw new System.Exception("An error occurred while searching for tv show from Sonarr");
        }

        public async Task<TvShow> GetTvShowDetailsAsync(int theTvDbId)
        {
            RefreshSonarrCache();

            try
            {
                int? sonarrSeriesId = null;

                lock (_lock)
                {
                    sonarrSeriesId = _tvDbToSonarrId.ContainsKey(theTvDbId) ? _tvDbToSonarrId[theTvDbId] : (int?)null;
                }

                var jsonTvShow = await FindSeriesInSonarrAsync(theTvDbId, sonarrSeriesId?.ToString());

                var convertedTvShow = Convert(jsonTvShow, jsonTvShow.seasons, jsonTvShow.id.HasValue ? await GetSonarrEpisodesAsync(jsonTvShow.id.Value) : new Dictionary<int, JSONEpisode[]>());
                var searchedTvShow = (await SearchTvShowAsync(convertedTvShow.Title)).FirstOrDefault(x => x.TheTvDbId == theTvDbId);

                convertedTvShow.Banner = searchedTvShow?.Banner;

                return convertedTvShow;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting tv show details with Sonarr: " + ex.Message);
            }

            throw new System.Exception("An error occurred while searching for tv show details with Sonarr");
        }

        public async Task<IReadOnlyList<TvShow>> GetTvShowDetailsAsync(HashSet<int> theTvDbIds, CancellationToken token)
        {

            try
            {
                RefreshSonarrCache();

                var jsonTvShows = await GetSonarrSeriesAsync();
                var sonarrSeriesToSonarrId = new Dictionary<int, JSONTvShow>();

                foreach (var show in jsonTvShows)
                {
                    sonarrSeriesToSonarrId.Add(show.id.Value, show);
                }

                var convertedTvShows = new List<TvShow>();

                await Task.WhenAll(jsonTvShows.Where(x => theTvDbIds.Contains(x.tvdbId.Value)).Select(async x =>
                {
                    var convertedTvShow = Convert(x, sonarrSeriesToSonarrId[x.id.Value].seasons, await GetSonarrEpisodesAsync(x.id.Value));

                    try
                    {
                        convertedTvShow.Banner = (await SearchSerieByTvDbIdAsync(x.tvdbId.Value)).remotePoster;
                    }
                    catch
                    {
                        // Ignore
                    }

                    convertedTvShows.Add(convertedTvShow);
                }));

                return convertedTvShows;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occurred while searching available tv shows with Sonarr: " + ex.Message);
            }

            throw new System.Exception("An error occurred while searching available tv shows with Sonarr");
        }

        public async Task<TvShowRequestResult> RequestTvShowAsync(TvShowUserRequester requester, TvShow tvShow, TvSeason season)
        {
            RefreshSonarrCache();
            try
            {
                var requestedSeasons = season is AllTvSeasons
                                        ? new List<TvSeason>(tvShow.Seasons.OfType<NormalTvSeason>())
                                        : season is FutureTvSeasons
                                            ? new List<TvSeason>()
                                            : new List<TvSeason> { season };

                if (string.IsNullOrEmpty(tvShow.DownloadClientId))
                {
                    await CreateSonarrTvSeriesAsync(tvShow, requestedSeasons);
                }
                else
                {
                    await UpdateSonarrTvSeriesAsync(tvShow, requestedSeasons);
                }

                return new TvShowRequestResult();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"An error while requesting tv show \"{tvShow.Title}\" from Sonarr: " + ex.Message);
            }

            throw new System.Exception("An error occurred while requesting a tv show from Sonarr");
        }

        private async Task CreateSonarrTvSeriesAsync(TvShow tvShow, IReadOnlyList<TvSeason> seasons)
        {
            bool isAnime = false;

            try
            {
                var tzMazeResponse = await HttpGetAsync($"http://api.tvmaze.com/lookup/shows?thetvdb={tvShow.TheTvDbId}");
                await tzMazeResponse.ThrowIfNotSuccessfulAsync("TvMazeLookup failed", x => x.message);

                var tvMazeJsonResponse = await tzMazeResponse.Content.ReadAsStringAsync();
                var tvMazeShow = JsonConvert.DeserializeObject<TvMazeTvShow>(tvMazeJsonResponse);

                isAnime = tvMazeShow.isAnime;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            var response = await HttpGetAsync($"{BaseURL}/series/lookup?term=tvdb:{tvShow.TheTvDbId}");
            await response.ThrowIfNotSuccessfulAsync("SonarrSeriesLookup failed", x => x.error);

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var jsonTvShow = JsonConvert.DeserializeObject<IEnumerable<JSONTvShow>>(jsonResponse).First();

            response = await HttpPostAsync($"{BaseURL}/series", JsonConvert.SerializeObject(new
            {
                title = jsonTvShow.title,
                qualityProfileId = isAnime ? SonarrSettings.AnimeProfileId : SonarrSettings.TvProfileId,
                profileId = isAnime ? SonarrSettings.AnimeProfileId : SonarrSettings.TvProfileId,
                languageProfileId = isAnime ? SonarrSettings.AnimeLanguageId : SonarrSettings.TvLanguageId,
                titleSlug = jsonTvShow.titleSlug,
                monitored = SonarrSettings.MonitorNewRequests,
                images = new string[0],
                tvdbId = tvShow.TheTvDbId,
                seriesType = isAnime ? "anime" : "standard",
                year = jsonTvShow.year,
                seasonFolder = isAnime ? SonarrSettings.AnimeUseSeasonFolders : SonarrSettings.TvUseSeasonFolders,
                rootFolderPath = isAnime ? SonarrSettings.AnimeRootFolder : SonarrSettings.TvRootFolder,
                id = jsonTvShow.id,
                seasons = jsonTvShow.seasons.Select(s => new
                {
                    seasonNumber = s.seasonNumber,
                    monitored = seasons.Any(rs => rs.SeasonNumber == s.seasonNumber)
                }),
                addOptions = new
                {
                    searchForMissingEpisodes = SonarrSettings.SearchNewRequests
                }
            }));

            await response.ThrowIfNotSuccessfulAsync("SonarrSeriesCreation failed", x => x.error);

            jsonResponse = await response.Content.ReadAsStringAsync();
            dynamic sonarrTvShow = JObject.Parse(jsonResponse);

            var sonarrSeriesId = (int)sonarrTvShow.id;

            lock (_lock)
            {
                _tvDbToSonarrId[tvShow.TheTvDbId] = sonarrSeriesId;
            }
        }

        private async Task UpdateSonarrTvSeriesAsync(TvShow tvShow, IReadOnlyList<TvSeason> seasons)
        {
            var response = await HttpGetAsync($"{BaseURL}/series/{tvShow.DownloadClientId}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    var sonarrTvShow = await FindSeriesInSonarrAsync(tvShow.TheTvDbId, tvShow.DownloadClientId);

                    if (sonarrTvShow.id.HasValue)
                    {
                        await UpdateSonarrTvSeriesAsync(tvShow, seasons);
                        return;
                    }
                    else
                    {
                        await CreateSonarrTvSeriesAsync(tvShow, seasons);
                        return;
                    }
                }

                await response.ThrowIfNotSuccessfulAsync("SonarrSerieGet failed", x => x.error);
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();

            dynamic sonarrSeries = JObject.Parse(jsonResponse);
            var isAnime = sonarrSeries.seriesType.ToString().Equals("anime", StringComparison.InvariantCultureIgnoreCase);

            sonarrSeries.profileId = isAnime ? SonarrSettings.AnimeProfileId : SonarrSettings.TvProfileId;
            sonarrSeries.monitored = SonarrSettings.MonitorNewRequests;
            sonarrSeries.seasonFolder = isAnime ? SonarrSettings.AnimeUseSeasonFolders : SonarrSettings.TvUseSeasonFolders;

            if (seasons.Any())
            {
                IEnumerable<dynamic> jsonSeasons = sonarrSeries.seasons;

                foreach (var season in jsonSeasons)
                {
                    season.monitored = seasons.Any(s => s.SeasonNumber == (int)season.seasonNumber) ? false : (bool)season.monitored;
                }

                response = await HttpPutAsync($"{BaseURL}/series/{tvShow.DownloadClientId}", JsonConvert.SerializeObject(sonarrSeries));
                await response.ThrowIfNotSuccessfulAsync("SonarrSeriesUpdate failed", x => x.error);

                foreach (var season in jsonSeasons)
                {
                    season.monitored = seasons.Any(s => s.SeasonNumber == (int)season.seasonNumber) ? true : (bool)season.monitored;
                }
            }

            response = await HttpPutAsync($"{BaseURL}/series/{tvShow.DownloadClientId}", JsonConvert.SerializeObject(sonarrSeries));
            await response.ThrowIfNotSuccessfulAsync("SonarrSeriesUpdate failed", x => x.error);

            if (SonarrSettings.SearchNewRequests)
            {
                var episodes = await GetSonarrEpisodesAsync(int.Parse(tvShow.DownloadClientId));

                await Task.WhenAll(seasons.Select(async s =>
                {
                    try
                    {
                        if (episodes[s.SeasonNumber].Any())
                        {
                            var undownloadedEpisodes = episodes[s.SeasonNumber].Where(x => !x.hasFile).Select(x => x.id).ToArray();

                            if (undownloadedEpisodes.Length == episodes[s.SeasonNumber].Count())
                            {
                                response = await HttpPostAsync($"{BaseURL}/command", JsonConvert.SerializeObject(new
                                {
                                    name = "SeasonSearch",
                                    seasonNumber = s.SeasonNumber,
                                    seriesId = tvShow.DownloadClientId
                                }));

                                await response.ThrowIfNotSuccessfulAsync("SonarrSeasonSearchCommand failed", x => x.error);
                            }
                            else
                            {
                                await response.ThrowIfNotSuccessfulAsync("SonarrSearchEpisodeCommand failed", x => x.error);
                                response = await HttpPostAsync($"{BaseURL}/command", JsonConvert.SerializeObject(new
                                {
                                    name = "EpisodeSearch",
                                    episodeIds = undownloadedEpisodes,
                                }));

                                await response.ThrowIfNotSuccessfulAsync("SonarrSearchEpisodeCommand failed", x => x.error);
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        _logger.LogError(ex.Message);
                        throw;
                    }
                }));
            }
        }

        private async Task<JSONTvShow> FindSeriesInSonarrAsync(int tvDbId, string downloadClientId = null)
        {
            if (!string.IsNullOrEmpty(downloadClientId))
            {
                var response = await HttpGetAsync($"{BaseURL}/series/{downloadClientId}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<JSONTvShow>(jsonResponse);
                }
                else
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        var existingTvShowInSonarr = (await GetSonarrSeriesAsync()).FirstOrDefault(x => x.tvdbId.HasValue && x.tvdbId.Value == tvDbId);

                        return existingTvShowInSonarr != null
                        ? existingTvShowInSonarr
                        : await FindSeriesInSonarrAsync(tvDbId);
                    }

                    await response.ThrowIfNotSuccessfulAsync("SonarrSerieGet failed", x => x.error);
                }
            }

            var existingTvShow = (await GetSonarrSeriesAsync()).FirstOrDefault(x => x.tvdbId.HasValue && x.tvdbId.Value == tvDbId);

            if (existingTvShow != null)
            {
                return existingTvShow;
            }

            return await SearchSerieByTvDbIdAsync(tvDbId);
        }

        private async Task<JSONTvShow[]> GetSonarrSeriesAsync()
        {
            var response = await HttpGetAsync($"{BaseURL}/series");

            await response.ThrowIfNotSuccessfulAsync("SonarrSeriesGetAll failed", x => x.error);

            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<JSONTvShow>>(jsonResponse).Where(x => x.tvdbId.HasValue).ToArray();
        }

        private async Task<JSONTvShow> SearchSerieByTvDbIdAsync(int tvDbId)
        {
            var response = await HttpGetAsync($"{BaseURL}/series/lookup?term=tvdb:{tvDbId}");
            await response.ThrowIfNotSuccessfulAsync("SonarrSeriesLookupByTvDbId failed", x => x.error);

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var tvShow = JsonConvert.DeserializeObject<IEnumerable<JSONTvShow>>(jsonResponse).First();

            if (!tvShow.id.HasValue)
            {
                tvShow.monitored = false;
            }

            return tvShow;
        }

        private async Task<IDictionary<int, JSONEpisode[]>> GetSonarrEpisodesAsync(int sonarrId)
        {
            var response = await HttpGetAsync($"{BaseURL}/episode?seriesId={sonarrId}");

            await response.ThrowIfNotSuccessfulAsync("SonarrGetSeriesEpisode failed", x => x.error);

            var jsonResponse = await response.Content.ReadAsStringAsync();

            var episodes = JsonConvert.DeserializeObject<IReadOnlyList<JSONEpisode>>(jsonResponse);

            return episodes.GroupBy(x => x.seasonNumber).ToDictionary(x => x.Key, x => x.ToArray());
        }

        private async void RefreshSonarrCache()
        {
            try
            {
                if (_loadedCache && string.Equals(_cachedEndpoint, $"{SonarrSettings.Hostname}:{SonarrSettings.Port}"))
                    return;

                var jsonTvShows = await GetSonarrSeriesAsync();

                lock (_lock)
                {
                    _tvDbToSonarrId.Clear();

                    foreach (var show in jsonTvShows)
                    {
                        _tvDbToSonarrId.Add(show.tvdbId.Value, show.id.Value);
                    }

                    _loadedCache = true;

                    _cachedEndpoint = $"{SonarrSettings.Hostname}:{SonarrSettings.Port}";
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating sonarr cache: " + ex.Message);
            }
        }

        private TvShow Convert(JSONTvShow jsonTvShow, IReadOnlyList<JSONSeason> seasons, IDictionary<int, JSONEpisode[]> episodesToSeason)
        {
            var downloadClientId = jsonTvShow.id?.ToString();
            var isTvShowMonitored = (string.IsNullOrWhiteSpace(downloadClientId) || SonarrSettings.MonitorNewRequests) ? jsonTvShow.monitored : true;

            var tvSeasons = seasons.Where(x => x.seasonNumber > 0).Select(x =>
            {
                var episodes = episodesToSeason.ContainsKey(x.seasonNumber) ? episodesToSeason[x.seasonNumber] : Array.Empty<JSONEpisode>();
                var tvEpisodes = ConvertToTvEpisodes(x, episodes).OrderBy(x => x.EpisodeNumber).ToArray();

                if (tvEpisodes.Any())
                {
                    var allEpisodesDownloaded = isTvShowMonitored ? tvEpisodes.All(x => x.IsRequested || x.IsAvailable) : tvEpisodes.All(x => x.IsAvailable);
                    var partiallyDownloaded = isTvShowMonitored ? tvEpisodes.Any(x => x.IsRequested || x.IsAvailable) : tvEpisodes.Any(x => x.IsAvailable);

                    return new NormalTvSeason
                    {
                        SeasonNumber = x.seasonNumber,
                        IsAvailable = tvEpisodes.First().IsAvailable,
                        IsRequested = allEpisodesDownloaded ? RequestedState.Full : partiallyDownloaded ? RequestedState.Partial : RequestedState.None,
                    };
                }
                else if (jsonTvShow.ExistsInSonarr())
                {
                    return new NormalTvSeason
                    {
                        SeasonNumber = x.seasonNumber,
                        IsAvailable = false,
                        IsRequested = isTvShowMonitored ? RequestedState.Full : RequestedState.None,
                    };
                }
                else
                {
                    return new NormalTvSeason
                    {
                        SeasonNumber = x.seasonNumber,
                        IsAvailable = false,
                        IsRequested = RequestedState.None
                    };
                }

            }).ToArray();

            return new TvShow
            {
                TheTvDbId = jsonTvShow.tvdbId.Value,
                DownloadClientId = downloadClientId,
                Title = jsonTvShow.title,
                Overview = jsonTvShow.overview,
                Quality = "",
                IsRequested = isTvShowMonitored,
                PlexUrl = "",
                EmbyUrl = "",
                WebsiteUrl = jsonTvShow.tvdbId != null ? $"https://www.thetvdb.com/?id={jsonTvShow.tvdbId.Value}&tab=series" : null,
                Seasons = tvSeasons.OrderBy(x => x.SeasonNumber).ToArray(),
                FirstAired = ((int)jsonTvShow.year).ToString(),
                HasEnded = ((string)jsonTvShow.status).Equals("ended", StringComparison.InvariantCultureIgnoreCase)
            };
        }

        private TvEpisode[] ConvertToTvEpisodes(JSONSeason season, JSONEpisode[] episodes)
        {
            return episodes.Select(x => new TvEpisode
            {
                EpisodeNumber = x.episodeNumber,
                IsAvailable = x.hasFile,
                IsRequested = x.monitored
            }).ToArray();
        }

        private static string GetBaseURL(SonarrSettings settings)
        {
            var protocol = settings.UseSSL ? "https" : "http";

            return $"{protocol}://{settings.Hostname}:{settings.Port}{settings.BaseUrl}/api";
        }

        private Task<HttpResponseMessage> HttpGetAsync(string url)
        {
            return HttpGetAsync(_httpClientFactory.CreateClient(), SonarrSettings, url);
        }

        private static async Task<HttpResponseMessage> HttpGetAsync(HttpClient client, SonarrSettings settings, string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("X-Api-Key", settings.ApiKey);

            using (var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5)))
            {
                return await client.SendAsync(request, cts.Token);
            }
        }

        private async Task<HttpResponseMessage> HttpPostAsync(string url, string content)
        {
            var postRequest = new StringContent(content);
            postRequest.Headers.Clear();
            postRequest.Headers.Add("Content-Type", "application/json");
            postRequest.Headers.Add("X-Api-Key", SonarrSettings.ApiKey);

            var client = _httpClientFactory.CreateClient();
            return await client.PostAsync(url, postRequest);
        }

        private async Task<HttpResponseMessage> HttpPutAsync(string url, string content)
        {
            var putRequest = new StringContent(content);
            putRequest.Headers.Clear();
            putRequest.Headers.Add("Content-Type", "application/json");
            putRequest.Headers.Add("X-Api-Key", SonarrSettings.ApiKey);

            var client = _httpClientFactory.CreateClient();
            return await client.PutAsync(url, putRequest);
        }

        public class JSONEpisode
        {
            public int id { get; set; }
            public int seasonNumber { get; set; }
            public int episodeNumber { get; set; }
            public bool hasFile { get; set; }
            public bool monitored { get; set; }
        }

        private class JSONImage
        {
            public string coverType { get; set; }
            public string url { get; set; }
        }

        private class JSONSeason
        {
            public int seasonNumber { get; set; }
            public bool monitored { get; set; }
        }

        private class JSONTvShow
        {
            public int? id { get; set; }
            public string title { get; set; }
            public string status { get; set; }
            public string overview { get; set; }
            public string remotePoster { get; set; }
            public bool monitored { get; set; }
            public List<JSONSeason> seasons { get; set; }
            public int year { get; set; }
            public int? tvdbId { get; set; }
            public string titleSlug { get; set; }

            public bool ExistsInSonarr()
            {
                return id.HasValue;
            }
        }

        private class TvMazeSearchedTvShow
        {
            public TvMazeTvShow Show { get; set; }
        }

        private class TvMazeTvShow
        {
            public string name { get; set; }
            public string premiered { get; set; }
            public string type { get; set; }
            public string language { get; set; }
            public bool isAnime => type.Equals("animation", StringComparison.InvariantCultureIgnoreCase) && language.Equals("japanese", StringComparison.InvariantCultureIgnoreCase);
            public TvMazeShowExternals externals { get; set; }
            public TvMazeShowImages image { get; set; }
        }

        private class TvMazeShowImages
        {
            public string original { get; set; }
        }

        private class TvMazeShowExternals
        {
            public int? thetvdb { get; set; }
        }
    }
}