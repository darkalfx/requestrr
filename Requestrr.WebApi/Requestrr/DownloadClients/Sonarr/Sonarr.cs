using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Requestrr.WebApi.Requestrr.TvShows;

namespace Requestrr.WebApi.Requestrr.DownloadClients
{
    public class Sonarr : ITvShowSearcher, ITvShowRequester
    {
        private IHttpClientFactory _httpClientFactory;
        private readonly ILogger<Sonarr> _logger;
        private SonarrSettingsProvider _sonarrSettingsProvider;
        private SonarrSettings SonarrSettings => _sonarrSettingsProvider.Provide();
        private string BaseURL => GetBaseURL(SonarrSettings);
        private Dictionary<int, int> _tvDbToSonarrId = new Dictionary<int, int>();
        private object _lock = new object();
        private bool _loadedCache = false;

        public Sonarr(IHttpClientFactory httpClientFactory, ILogger<Sonarr> logger, SonarrSettingsProvider sonarrSettingsProvider)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _sonarrSettingsProvider = sonarrSettingsProvider;
        }

        public static async Task TestConnectionAsync(HttpClient httpClient, ILogger<Sonarr> logger, SonarrSettings settings)
        {
            var testSuccessful = false;

            try
            {
                var response = await HttpGetAsync(httpClient, settings, $"{GetBaseURL(settings)}/system/status");
                testSuccessful = response.IsSuccessStatusCode;
            }
            catch (System.Exception ex)
            {
                logger.LogWarning("Error while testing Sonarr connection: " + ex.Message);
                throw new Exception("Could not connect to Sonarr");
            }

            if (!testSuccessful)
            {
                throw new Exception("Could not connect to Sonarr");
            }
        }

        public static async Task<IList<JSONRootPath>> GetRootPaths(HttpClient httpClient, ILogger<Sonarr> logger, SonarrSettings settings)
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
                    logger.LogWarning("An error while getting Sonarr root paths: " + ex.Message);
                    retryCount++;
                    await Task.Delay(1000);
                }
            }

            throw new System.Exception("An error occurred while getting Sonarr root paths");
        }

        public static async Task<IList<JSONProfile>> GetProfiles(HttpClient httpClient, ILogger<Sonarr> logger, SonarrSettings settings)
        {
            var retryCount = 0;

            while (retryCount <= 5)
            {
                try
                {
                    var response = await HttpGetAsync(httpClient, settings, $"{GetBaseURL(settings)}/qualityprofile");
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<IList<JSONProfile>>(jsonResponse);
                }
                catch (System.Exception ex)
                {
                    logger.LogWarning("An error while getting Sonarr profiles: " + ex.Message);
                    retryCount++;
                    await Task.Delay(1000);
                }
            }

            throw new System.Exception("An error occurred while getting Sonarr profiles");
        }

        public static async Task<IList<JSONLanguageProfile>> GetLanguages(HttpClient httpClient, ILogger<Sonarr> logger, SonarrSettings settings)
        {
            var retryCount = 0;

            while (retryCount <= 5)
            {
                try
                {
                    var response = await HttpGetAsync(httpClient, settings, $"{GetBaseURL(settings)}/languageprofile");
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<IList<JSONLanguageProfile>>(jsonResponse);
                }
                catch (System.Exception ex)
                {
                    logger.LogWarning("An error while getting Sonarr languages: " + ex.Message);
                    retryCount++;
                    await Task.Delay(1000);
                }
            }

            throw new System.Exception("An error occurred while getting Sonarr languages");
        }

        public static async Task<IList<JSONTag>> GetTags(HttpClient httpClient, ILogger<Sonarr> logger, SonarrSettings settings)
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
                    logger.LogWarning("An error while getting Sonarr tags: " + ex.Message);
                    retryCount++;
                    await Task.Delay(1000);
                }
            }

            throw new System.Exception("An error occurred while getting Sonarr tags");
        }

        public async Task<IReadOnlyList<SearchedTvShow>> SearchTvShowAsync(string tvShowName)
        {
            var retryCount = 0;

            while (retryCount <= 5 && _loadedCache)
            {
                try
                {
                    var response = await HttpGetAsync($"http://api.tvmaze.com/search/shows?q={tvShowName}");

                    if (!response.IsSuccessStatusCode)
                        throw new Exception(response.ReasonPhrase);

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var tvMazeShows = JsonConvert.DeserializeObject<List<TvMazeSearchedTvShow>>(jsonResponse).Where(x => x.Show.externals.thetvdb.HasValue).ToArray();

                    return tvMazeShows.Select(x => new SearchedTvShow
                    {
                        TheTvDbId = x.Show.externals.thetvdb.Value,
                        Title = x.Show.name,
                        FirstAired = x.Show.premiered,
                        Banner = (x.Show.image != null && !string.IsNullOrEmpty(x.Show.image.original)) ? x.Show.image.original : string.Empty
                    }).ToArray();
                }
                catch (System.Exception ex)
                {
                    _logger.LogWarning($"An error occurred while searching for tv show \"{tvShowName}\" from Sonarr: " + ex.Message);
                    retryCount++;
                    await Task.Delay(1000);
                }
            }

            throw new System.Exception("An error occurred while searching for tv show from Sonarr");
        }

        public async Task<TvShow> GetTvShowDetailsAsync(SearchedTvShow searchedTvShow)
        {
            var retryCount = 0;

            while (retryCount <= 5 && _loadedCache)
            {
                try
                {
                    var tvDbId = searchedTvShow.TheTvDbId;
                    int? sonarrSeriesId = null;

                    lock (_lock)
                    {
                        sonarrSeriesId = _tvDbToSonarrId.ContainsKey(tvDbId) ? _tvDbToSonarrId[tvDbId] : (int?)null;
                    }

                    var jsonTvShow = await FindSeriesInSonarrAsync(tvDbId, sonarrSeriesId?.ToString());

                    var convertedTvShow = Convert(jsonTvShow, jsonTvShow.seasons, jsonTvShow.id.HasValue ? await GetSonarrEpisodesAsync(jsonTvShow.id.Value) : new Dictionary<int, JSONEpisode[]>());
                    convertedTvShow.Banner = searchedTvShow.Banner;

                    return convertedTvShow;
                }
                catch (System.Exception ex)
                {
                    _logger.LogWarning("An error occurred while getting tv show details with Sonarr: " + ex.Message);
                    retryCount++;
                    await Task.Delay(1000);
                }
            }

            throw new System.Exception("An error occurred while searching for tv show details with Sonarr");
        }

        public async Task<IReadOnlyList<TvShow>> GetTvShowDetailsAsync(HashSet<int> theTvDbIds, CancellationToken token)
        {
            var retryCount = 0;

            while (retryCount <= 5 && !token.IsCancellationRequested)
            {
                try
                {
                    var jsonTvShows = await GetSonarrSeriesAsync();
                    var sonarrSeriesToSonarrId = new Dictionary<int, JSONTvShow>();

                    lock (_lock)
                    {
                        _tvDbToSonarrId.Clear();

                        foreach (var show in jsonTvShows)
                        {
                            _tvDbToSonarrId.Add(show.tvdbId.Value, show.id.Value);
                            sonarrSeriesToSonarrId.Add(show.id.Value, show);
                        }
                    }

                    _loadedCache = true;

                    var convertedTvShows = new List<TvShow>();

                    await Task.WhenAll(jsonTvShows.Where(x => theTvDbIds.Contains(x.tvdbId.Value)).Select(async x =>
                    {
                        x.images = await GetImagesFromTvMazeAsync(x.tvdbId.Value);

                        var convertedTvShow = Convert(x, sonarrSeriesToSonarrId[x.id.Value].seasons, await GetSonarrEpisodesAsync(x.id.Value));
                        convertedTvShows.Add(convertedTvShow);
                    }));

                    return convertedTvShows;
                }
                catch (System.Exception ex)
                {
                    _logger.LogWarning("An error occurred while searching available tv shows with Sonarr: " + ex.Message);
                    retryCount++;
                    await Task.Delay(1000, token);
                }
            }

            throw new System.Exception("An error occurred while searching available tv shows with Sonarr");
        }

        public async Task RequestTvShowAsync(string userName, TvShow tvShow, TvSeason season)
        {
            var retryCount = 0;

            while (retryCount <= 5 && _loadedCache)
            {
                try
                {
                    var requestedSeasons = season is AllTvSeasons
                                            ? tvShow.Seasons.ToList()
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

                    return;
                }
                catch (System.Exception ex)
                {
                    _logger.LogWarning($"An error while requesting tv show \"{tvShow.Title}\" from Sonarr: " + ex.Message);
                    retryCount++;
                    await Task.Delay(1000);
                }
            }

            throw new System.Exception("An error occurred while requesting a tv show from Sonarr");
        }

        private async Task<List<JSONImage>> GetImagesFromTvMazeAsync(int tvdbId)
        {
            try
            {
                await Task.Delay(100);
                var response = await HttpGetAsync($"http://api.tvmaze.com/lookup/shows?thetvdb={tvdbId}");

                if (!response.IsSuccessStatusCode)
                    return new List<JSONImage>();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var tvMazeShow = JsonConvert.DeserializeObject<TvMazeTvShow>(jsonResponse);

                if (tvMazeShow.image != null && !string.IsNullOrEmpty(tvMazeShow.image.original))
                {
                    return new List<JSONImage>
                    {
                        new JSONImage
                        {
                            coverType = "poster",
                            url = tvMazeShow.image.original,
                        }
                    };
                }

                return new List<JSONImage>();
            }
            catch
            {
                return new List<JSONImage>();
            }
        }

        private async Task CreateSonarrTvSeriesAsync(TvShow tvShow, IReadOnlyList<TvSeason> seasons)
        {
            var response = await HttpGetAsync($"http://api.tvmaze.com/lookup/shows?thetvdb={tvShow.TheTvDbId}");

            if (!response.IsSuccessStatusCode)
                throw new Exception(response.ReasonPhrase);

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var tvMazeShow = JsonConvert.DeserializeObject<TvMazeTvShow>(jsonResponse);

            response = await HttpGetAsync($"{BaseURL}/series/lookup?term=tvdb:{tvShow.TheTvDbId}");

            if (!response.IsSuccessStatusCode)
                throw new Exception(response.ReasonPhrase);

            jsonResponse = await response.Content.ReadAsStringAsync();
            var jsonTvShow = JsonConvert.DeserializeObject<IEnumerable<JSONTvShow>>(jsonResponse).First();

            int[] tags = Array.Empty<int>();

            if (SonarrSettings.Version != "2")
            {
                tags = tvMazeShow.isAnime ? SonarrSettings.AnimeTags : SonarrSettings.TvTags;
            }

            response = await HttpPostAsync($"{BaseURL}/series", JsonConvert.SerializeObject(new
            {
                title = jsonTvShow.title,
                qualityProfileId = tvMazeShow.isAnime ? SonarrSettings.AnimeProfileId : SonarrSettings.TvProfileId,
                profileId = tvMazeShow.isAnime ? SonarrSettings.AnimeProfileId : SonarrSettings.TvProfileId,
                languageProfileId = tvMazeShow.isAnime ? SonarrSettings.AnimeLanguageId : SonarrSettings.TvLanguageId,
                titleSlug = jsonTvShow.titleSlug,
                monitored = true,
                images = new string[0],
                tvdbId = tvShow.TheTvDbId,
                tags = tags,
                seriesType = tvMazeShow.isAnime ? "anime" : "standard",
                year = jsonTvShow.year,
                seasonFolder = tvMazeShow.isAnime ? SonarrSettings.AnimeUseSeasonFolders : SonarrSettings.TvUseSeasonFolders,
                rootFolderPath = tvMazeShow.isAnime ? SonarrSettings.AnimeRootFolder : SonarrSettings.TvRootFolder,
                id = jsonTvShow.id,
                seasons = jsonTvShow.seasons.Select(s => new
                {
                    seasonNumber = s.seasonNumber,
                    monitored = seasons.Any(rs => rs.SeasonNumber == s.seasonNumber)
                }),
                addOptions = new
                {
                    searchForMissingEpisodes = true
                }
            }));

            if (!response.IsSuccessStatusCode)
                throw new Exception(response.ReasonPhrase);

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

                throw new Exception(response.ReasonPhrase);
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();

            dynamic sonarrSeries = JObject.Parse(jsonResponse);
            var isAnime = sonarrSeries.seriesType.ToString().Equals("anime", StringComparison.InvariantCultureIgnoreCase);

            if (SonarrSettings.Version == "2")
            {
                sonarrSeries.profileId = isAnime ? SonarrSettings.AnimeProfileId : SonarrSettings.TvProfileId;
            }
            else
            {
                sonarrSeries.tags = JToken.FromObject(isAnime ? SonarrSettings.AnimeTags : SonarrSettings.TvTags);
                sonarrSeries.qualityProfileId = isAnime ? SonarrSettings.AnimeProfileId : SonarrSettings.TvProfileId;
                sonarrSeries.languageProfileId = isAnime ? SonarrSettings.AnimeLanguageId : SonarrSettings.TvLanguageId;
            }

            sonarrSeries.monitored = true;
            sonarrSeries.seasonFolder = isAnime ? SonarrSettings.AnimeUseSeasonFolders : SonarrSettings.TvUseSeasonFolders;

            if (seasons.Any())
            {
                IEnumerable<dynamic> jsonSeasons = sonarrSeries.seasons;

                foreach (var season in jsonSeasons)
                {
                    season.monitored = seasons.Any(s => s.SeasonNumber == (int)season.seasonNumber) ? false : (bool)season.monitored;
                }

                response = await HttpPutAsync($"{BaseURL}/series/{tvShow.DownloadClientId}", JsonConvert.SerializeObject(sonarrSeries));

                if (!response.IsSuccessStatusCode)
                    throw new Exception(response.ReasonPhrase);

                foreach (var season in jsonSeasons)
                {
                    season.monitored = seasons.Any(s => s.SeasonNumber == (int)season.seasonNumber) ? true : (bool)season.monitored;
                }
            }

            response = await HttpPutAsync($"{BaseURL}/series/{tvShow.DownloadClientId}", JsonConvert.SerializeObject(sonarrSeries));

            if (!response.IsSuccessStatusCode)
                throw new Exception(response.ReasonPhrase);

            var episodes = await GetSonarrEpisodesAsync(int.Parse(tvShow.DownloadClientId));

            await Task.WhenAll(seasons.Select(async s =>
            {
                {
                    try
                    {
                        if (episodes[s.SeasonNumber].Any())
                        {
                            var undownloadedEpisodes = episodes[s.SeasonNumber].Where(x => !x.hasFile).Select(x => x.id).ToArray();

                            response = await HttpPostAsync($"{BaseURL}/command", JsonConvert.SerializeObject(new
                            {
                                name = "EpisodeSearch",
                                episodeIds = undownloadedEpisodes,
                            }));

                            if (!response.IsSuccessStatusCode)
                                throw new Exception(response.ReasonPhrase);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        _logger.LogWarning(ex.Message);
                    }
                }
            }));
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
                        var existingTvShow = (await GetSonarrSeriesAsync()).FirstOrDefault(x => x.tvdbId.HasValue && x.tvdbId.Value == tvDbId);

                        return existingTvShow != null
                        ? existingTvShow
                        : await FindSeriesInSonarrAsync(tvDbId);
                    }

                    throw new Exception("Failed to get a tv show detail with Sonarr");
                }
            }

            if (SonarrSettings.Version == "2")
            {
                var existingTvShow = (await GetSonarrSeriesAsync()).FirstOrDefault(x => x.tvdbId.HasValue && x.tvdbId.Value == tvDbId);

                if (existingTvShow != null)
                {
                    return existingTvShow;
                }
            }

            return await SearchSerieByTvDbIdAsync(tvDbId);
        }

        private async Task<JSONTvShow[]> GetSonarrSeriesAsync()
        {
            var response = await HttpGetAsync($"{BaseURL}/series");

            if (!response.IsSuccessStatusCode)
                throw new Exception(response.ReasonPhrase);

            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<JSONTvShow>>(jsonResponse).Where(x => x.tvdbId.HasValue).ToArray();
        }

        private async Task<JSONTvShow> SearchSerieByTvDbIdAsync(int tvDbId)
        {
            var response = await HttpGetAsync($"{BaseURL}/series/lookup?term=tvdb:{tvDbId}");

            if (!response.IsSuccessStatusCode)
                throw new Exception(response.ReasonPhrase);

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

            if (!response.IsSuccessStatusCode)
                throw new Exception(response.ReasonPhrase);

            var jsonResponse = await response.Content.ReadAsStringAsync();

            var episodes = JsonConvert.DeserializeObject<IReadOnlyList<JSONEpisode>>(jsonResponse);

            return episodes.GroupBy(x => x.seasonNumber).ToDictionary(x => x.Key, x => x.ToArray());
        }

        private TvShow Convert(JSONTvShow jsonTvShow, IReadOnlyList<JSONSeason> seasons, IDictionary<int, JSONEpisode[]> episodesToSeason)
        {
            var tvSeasons = seasons.Where(x => x.seasonNumber > 0).Select(x =>
            {
                var episodes = episodesToSeason.ContainsKey(x.seasonNumber) ? episodesToSeason[x.seasonNumber] : Array.Empty<JSONEpisode>();
                var tvEpisodes = ConvertToTvEpisodes(x, episodes).OrderBy(x => x.EpisodeNumber).ToArray();

                if (tvEpisodes.Any())
                {
                    return new NormalTvSeason
                    {
                        SeasonNumber = x.seasonNumber,
                        IsAvailable = tvEpisodes.First().IsAvailable,
                        IsRequested = jsonTvShow.monitored ? tvEpisodes.All(x => x.IsRequested || x.IsAvailable) : tvEpisodes.All(x => x.IsAvailable),
                    };
                }
                else if (jsonTvShow.ExistsInSonarr())
                {
                    return new NormalTvSeason
                    {
                        SeasonNumber = x.seasonNumber,
                        IsAvailable = false,
                        IsRequested = jsonTvShow.monitored ? x.monitored : false,
                    };
                }
                else
                {
                    return new NormalTvSeason
                    {
                        SeasonNumber = x.seasonNumber,
                        IsAvailable = false,
                        IsRequested = false
                    };
                }

            }).ToArray();

            return new TvShow
            {
                TheTvDbId = jsonTvShow.tvdbId.Value,
                DownloadClientId = jsonTvShow.id?.ToString(),
                Title = jsonTvShow.title,
                Overview = jsonTvShow.overview,
                Quality = "",
                IsRequested = jsonTvShow.monitored,
                PlexUrl = "",
                EmbyUrl = "",
                Seasons = tvSeasons.OrderBy(x => x.SeasonNumber).ToArray(),
                Banner = jsonTvShow.images.Where(x => ((string)x.coverType).Equals("poster", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault()?.url ?? string.Empty,
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
            var version = settings.Version == "2" ? string.Empty : $"/v{settings.Version}";
            var protocol = settings.UseSSL ? "https" : "http";

            return $"{protocol}://{settings.Hostname}:{settings.Port}/api{version}";
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

            return await client.SendAsync(request);
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

        public class JSONLanguageProfile
        {
            public string name { get; set; }
            public int id { get; set; }
        }

        public class JSONTag
        {
            public string label { get; set; }
            public int id { get; set; }
        }

        public class JSONImage
        {
            public string coverType { get; set; }
            public string url { get; set; }
        }

        public class JSONSeason
        {
            public int seasonNumber { get; set; }
            public bool monitored { get; set; }
        }

        public class JSONTvShow
        {
            public int? id { get; set; }
            public string title { get; set; }
            public string status { get; set; }
            public string overview { get; set; }
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

        public class TvMazeSearchedTvShow
        {
            public TvMazeTvShow Show { get; set; }
        }

        public class TvMazeTvShow
        {
            public string name { get; set; }
            public string premiered { get; set; }
            public string type { get; set; }
            public string language { get; set; }
            public bool isAnime => type.Equals("animation", StringComparison.InvariantCultureIgnoreCase) && language.Equals("japanese", StringComparison.InvariantCultureIgnoreCase);
            public TvMazeShowExternals externals { get; set; }
            public TvMazeShowImages image { get; set; }
        }

        public class TvMazeShowImages
        {
            public string original { get; set; }
        }

        public class TvMazeShowExternals
        {
            public int? thetvdb { get; set; }
        }
    }
}