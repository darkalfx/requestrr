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
    public class SonarrClientV3 : ITvShowSearcher, ITvShowRequester
    {
        private IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SonarrClient> _logger;
        private SonarrSettingsProvider _sonarrSettingsProvider;
        private SonarrSettings SonarrSettings => _sonarrSettingsProvider.Provide();
        private string BaseURL => GetBaseURL(SonarrSettings);

        public SonarrClientV3(IHttpClientFactory httpClientFactory, ILogger<SonarrClient> logger, SonarrSettingsProvider sonarrSettingsProvider)
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

        public static async Task<IList<JSONLanguageProfile>> GetLanguages(HttpClient httpClient, ILogger<SonarrClient> logger, SonarrSettings settings)
        {
            try
            {
                var response = await HttpGetAsync(httpClient, settings, $"{GetBaseURL(settings)}/languageprofile");
                var jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<IList<JSONLanguageProfile>>(jsonResponse);
            }
            catch (System.Exception ex)
            {
                logger.LogWarning(ex, "An error while getting Sonarr languages: " + ex.Message);
            }

            throw new System.Exception("An error occurred while getting Sonarr languages");
        }

        public static async Task<IList<JSONTag>> GetTags(HttpClient httpClient, ILogger<SonarrClient> logger, SonarrSettings settings)
        {
            try
            {
                var response = await HttpGetAsync(httpClient, settings, $"{GetBaseURL(settings)}/tag");
                var jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<IList<JSONTag>>(jsonResponse);
            }
            catch (System.Exception ex)
            {
                logger.LogWarning(ex, "An error while getting Sonarr tags: " + ex.Message);
            }

            throw new System.Exception("An error occurred while getting Sonarr tags");
        }

        public async Task<SearchedTvShow> SearchTvShowAsync(TvShowRequest request, int tvDbId)
        {
            try
            {
                var jsonTvShow = await SearchSerieByTvDbIdAsync(tvDbId);

                if (jsonTvShow != null && jsonTvShow.tvdbId == tvDbId)
                {
                    return new SearchedTvShow
                    {
                        TheTvDbId = jsonTvShow.tvdbId.Value,
                        Title = jsonTvShow.title,
                        FirstAired = jsonTvShow.year > 0 ? jsonTvShow.year.ToString() : string.Empty,
                        Banner = jsonTvShow.remotePoster
                    };
                }

                return null;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while searching for tv show by tvDbId \"{tvDbId}\" from Sonarr: " + ex.Message);
            }

            throw new System.Exception("An error occurred while searching for tv show by tvDbId from Sonarr");
        }

        public async Task<IReadOnlyList<SearchedTvShow>> SearchTvShowAsync(TvShowRequest request, string tvShowName)
        {
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

        public async Task<TvShow> GetTvShowDetailsAsync(TvShowRequest request, int tvDbId)
        {
            try
            {
                var jsonTvShow = await FindSeriesInSonarrAsync(tvDbId);

                var convertedTvShow = Convert(jsonTvShow, jsonTvShow.seasons, jsonTvShow.id.HasValue ? await GetSonarrEpisodesAsync(jsonTvShow.id.Value) : new Dictionary<int, JSONEpisode[]>());

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
                var convertedTvShows = new List<TvShow>();

                foreach (var tvDbId in theTvDbIds)
                {
                    try
                    {
                        var series = await FindSeriesInSonarrAsync(tvDbId);

                        if (series != null && series.id != null && series.id > 0)
                        {
                            convertedTvShows.Add(Convert(series, series.seasons, await GetSonarrEpisodesAsync(series.id.Value)));
                        }
                    }
                    catch (System.Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred while searching available tv shows with Sonarr: " + ex.Message);
                    }
                }

                return convertedTvShows;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occurred while searching available tv shows with Sonarr: " + ex.Message);
            }

            throw new System.Exception("An error occurred while searching available tv shows with Sonarr");
        }

        public async Task<TvShowRequestResult> RequestTvShowAsync(TvShowRequest request, TvShow tvShow, TvSeason season)
        {
            try
            {
                var requestedSeasons = season is AllTvSeasons
                                        ? new List<TvSeason>(tvShow.Seasons.OfType<NormalTvSeason>())
                                        : season is FutureTvSeasons
                                            ? new List<TvSeason>()
                                            : new List<TvSeason> { season };

                if (string.IsNullOrEmpty(tvShow.DownloadClientId))
                {
                    await CreateSonarrTvSeriesAsync(request, tvShow, requestedSeasons);
                }
                else
                {
                    await UpdateSonarrTvSeriesAsync(request, tvShow, requestedSeasons);
                }

                return new TvShowRequestResult();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"An error while requesting tv show \"{tvShow.Title}\" from Sonarr: " + ex.Message);
            }

            throw new System.Exception("An error occurred while requesting a tv show from Sonarr");
        }

        private async Task CreateSonarrTvSeriesAsync(TvShowRequest request, TvShow tvShow, IReadOnlyList<TvSeason> seasons)
        {
            SonarrCategory category = null;

            try
            {
                category = SonarrSettings.Categories.Single(x => x.Id == request.CategoryId);
            }
            catch
            {
                _logger.LogError($"An error occurred while requesting a tv show \"{tvShow.Title}\" from Sonarr, could not find category with id {request.CategoryId}");
                throw new System.Exception($"An error occurred while requesting tv show \"{tvShow.Title}\" from Sonarr, could not find category with id {request.CategoryId}");
            }

            var response = await HttpGetAsync($"{BaseURL}/series/lookup?term=tvdb:{tvShow.TheTvDbId}");
            await response.ThrowIfNotSuccessfulAsync("SonarrSeriesLookup failed", x => x.error);

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var jsonTvShow = JsonConvert.DeserializeObject<IEnumerable<JSONTvShow>>(jsonResponse).First();

            int[] tags = category.Tags;

            string seriesType = category.SeriesType;

            if (seriesType == "automatic")
            {
                seriesType = await IsAnimeAsync(tvShow.TheTvDbId) ? "anime" : "standard";
            }

            response = await HttpPostAsync($"{BaseURL}/series", JsonConvert.SerializeObject(new
            {
                title = jsonTvShow.title,
                qualityProfileId = category.ProfileId,
                profileId = category.ProfileId,
                languageProfileId = category.LanguageId,
                titleSlug = jsonTvShow.titleSlug,
                monitored = SonarrSettings.MonitorNewRequests,
                images = new string[0],
                tvdbId = tvShow.TheTvDbId,
                tags = JToken.FromObject(tags),
                seriesType = seriesType,
                year = jsonTvShow.year,
                seasonFolder = category.UseSeasonFolders,
                rootFolderPath = category.RootFolder,
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
        }

        private async Task UpdateSonarrTvSeriesAsync(TvShowRequest request, TvShow tvShow, IReadOnlyList<TvSeason> seasons)
        {
            var response = await HttpGetAsync($"{BaseURL}/series/{tvShow.DownloadClientId}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    var sonarrTvShow = await FindSeriesInSonarrAsync(tvShow.TheTvDbId);

                    if (sonarrTvShow != null && sonarrTvShow.id.HasValue)
                    {
                        await UpdateSonarrTvSeriesAsync(request, tvShow, seasons);
                        return;
                    }
                    else
                    {
                        await CreateSonarrTvSeriesAsync(request, tvShow, seasons);
                        return;
                    }
                }

                await response.ThrowIfNotSuccessfulAsync("SonarrSerieGet failed", x => x.error);
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            dynamic sonarrSeries = JObject.Parse(jsonResponse);

            SonarrCategory category = null;

            try
            {
                category = SonarrSettings.Categories.Single(x => x.Id == request.CategoryId);
            }
            catch
            {
                _logger.LogError($"An error occurred while requesting a tv show \"{tvShow.Title}\" from Sonarr, could not find category with id {request.CategoryId}");
                throw new System.Exception($"An error occurred while requesting tv show \"{tvShow.Title}\" from Sonarr, could not find category with id {request.CategoryId}");
            }

            sonarrSeries.tags = JToken.FromObject(category.Tags);
            sonarrSeries.qualityProfileId = category.ProfileId;
            sonarrSeries.languageProfileId = category.LanguageId;
            sonarrSeries.monitored = SonarrSettings.MonitorNewRequests;
            sonarrSeries.seasonFolder = category.UseSeasonFolders;

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
                                    seriesId = int.Parse(tvShow.DownloadClientId)
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

        private async Task<JSONTvShow> FindSeriesInSonarrAsync(int tvDbId)
        {
            var series = await SearchSerieByTvDbIdAsync(tvDbId);

            if (series != null && series.id != null && series.id > 0)
            {
                var response = await HttpGetAsync($"{BaseURL}/series/{series.id}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<JSONTvShow>(jsonResponse);
                }
            }

            return series;
        }

        private async Task<JSONTvShow> SearchSerieByTvDbIdAsync(int tvDbId)
        {
            var response = await HttpGetAsync($"{BaseURL}/series/lookup?term=tvdb:{tvDbId}");
            await response.ThrowIfNotSuccessfulAsync("SonarrSeriesLookupByTvDbId failed", x => x.error);

            var jsonResponse = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<IEnumerable<JSONTvShow>>(jsonResponse).FirstOrDefault();
        }

        private async Task<IDictionary<int, JSONEpisode[]>> GetSonarrEpisodesAsync(int sonarrId)
        {
            var response = await HttpGetAsync($"{BaseURL}/episode?seriesId={sonarrId}");

            await response.ThrowIfNotSuccessfulAsync("SonarrGetSeriesEpisode failed", x => x.error);

            var jsonResponse = await response.Content.ReadAsStringAsync();

            var episodes = JsonConvert.DeserializeObject<IReadOnlyList<JSONEpisode>>(jsonResponse);

            return episodes.GroupBy(x => x.seasonNumber).ToDictionary(x => x.Key, x => x.ToArray());
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

            var tvShow = new TvShow
            {
                TheTvDbId = jsonTvShow.tvdbId.Value,
                DownloadClientId = downloadClientId,
                Title = jsonTvShow.title,
                Overview = jsonTvShow.overview,
                Quality = "",
                IsRequested = jsonTvShow.ExistsInSonarr() && jsonTvShow.monitored,
                PlexUrl = "",
                EmbyUrl = "",
                Banner = GetPosterImageUrl(jsonTvShow.images),
                WebsiteUrl = jsonTvShow.tvdbId != null ? $"https://www.thetvdb.com/?id={jsonTvShow.tvdbId.Value}&tab=series" : null,
                Seasons = tvSeasons.OrderBy(x => x.SeasonNumber).ToArray(),
                FirstAired = ((int)jsonTvShow.year).ToString(),
                HasEnded = ((string)jsonTvShow.status).Equals("ended", StringComparison.InvariantCultureIgnoreCase)
            };

            if (SonarrSettings.MonitorNewRequests && !tvShow.HasEnded)
            {
                tvShow.Seasons = tvShow.Seasons.Append(new FutureTvSeasons
                {
                    SeasonNumber = tvShow.Seasons?.Any() == true ? tvShow.Seasons.Max(x => x.SeasonNumber) + 1 : 1,
                    IsAvailable = false,
                    IsRequested = tvShow.IsRequested ? RequestedState.Full : RequestedState.None,
                }).ToArray();
            }

            return tvShow;
        }

        private string GetPosterImageUrl(List<JSONImage> jsonImages)
        {
            var posterImage = jsonImages.Where(x => x.coverType.Equals("poster", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

            if (posterImage != null)
            {
                if (!string.IsNullOrWhiteSpace(posterImage.remoteUrl))
                {
                    return posterImage.remoteUrl;
                }

                return posterImage.url;
            }

            return string.Empty;
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

        public async Task<bool> IsAnimeAsync(int theTvDbId)
        {
            try
            {
                var tzMazeResponse = await HttpGetAsync($"http://api.tvmaze.com/lookup/shows?thetvdb={theTvDbId}");
                await tzMazeResponse.ThrowIfNotSuccessfulAsync("TvMazeLookup failed", x => x.message);

                var tvMazeJsonResponse = await tzMazeResponse.Content.ReadAsStringAsync();
                var tvMazeShow = JsonConvert.DeserializeObject<TvMazeTvShow>(tvMazeJsonResponse);

                return tvMazeShow.isAnime;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return false;
        }

        private static string GetBaseURL(SonarrSettings settings)
        {
            var protocol = settings.UseSSL ? "https" : "http";
            return $"{protocol}://{settings.Hostname}:{settings.Port}{settings.BaseUrl}/api/v{settings.Version}";
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

        private class JSONEpisode
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
            public string remoteUrl { get; set; }
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
            public List<JSONImage> images { get; set; }
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
            public string[] genres { get; set; }
            public bool isAnime => (genres != null && genres.Any(x => string.Equals(x, "anime", StringComparison.InvariantCultureIgnoreCase))) || (type.Equals("animation", StringComparison.InvariantCultureIgnoreCase) && language.Equals("japanese", StringComparison.InvariantCultureIgnoreCase));
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