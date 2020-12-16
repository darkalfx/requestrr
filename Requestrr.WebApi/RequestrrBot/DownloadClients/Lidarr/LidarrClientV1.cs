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
using Requestrr.WebApi.RequestrrBot.Music;
using static Requestrr.WebApi.RequestrrBot.DownloadClients.Lidarr.LidarrClient;

namespace Requestrr.WebApi.RequestrrBot.DownloadClients.Lidarr
{
    public class LidarrClientV1 : IArtistRequester, IArtistSearcher
    {
        private IHttpClientFactory _httpClientFactory;
        private readonly ILogger<LidarrClient> _logger;
        private LidarrSettingsProvider _lidarrSettingsProvider;
        private LidarrSettings LidarrSettings => _lidarrSettingsProvider.Provide();
        private string BaseURL => GetBaseURL(LidarrSettings);

        public LidarrClientV1(IHttpClientFactory httpClientFactory, ILogger<LidarrClient> logger, LidarrSettingsProvider RadarrSettingsProvider)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _lidarrSettingsProvider = RadarrSettingsProvider;
        }

        public static async Task TestConnectionAsync(HttpClient httpClient, ILogger<LidarrClient> logger, LidarrSettings settings)
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
                        throw new Exception("Base url does not match what is set in Lidarr");
                    }
                }
                catch
                {
                    throw new Exception("Base url does not match what is set in Lidarr");
                }

                testSuccessful = true;
            }
            catch (HttpRequestException ex)
            {
                logger.LogWarning(ex, "Error while testing Lidarr connection: " + ex.Message);
                throw new Exception("Invalid host and/or port");
            }
            catch (System.Exception ex)
            {
                logger.LogWarning(ex, "Error while testing Lidarr connection: " + ex.Message);

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

        public static async Task<IList<JSONRootPath>> GetRootPaths(HttpClient httpClient, ILogger<LidarrClient> logger, LidarrSettings settings)
        {
            try
            {
                var response = await HttpGetAsync(httpClient, settings, $"{GetBaseURL(settings)}/rootfolder");

                var jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<IList<JSONRootPath>>(jsonResponse);
            }
            catch (System.Exception ex)
            {
                logger.LogWarning(ex, "An error while getting Lidarr root paths: " + ex.Message);
            }

            throw new System.Exception("An error occurred while getting Lidarr root paths");
        }

        public static async Task<IList<JSONProfile>> GetProfiles(HttpClient httpClient, ILogger<LidarrClient> logger, LidarrSettings settings)
        {
            try
            {
                var response = await HttpGetAsync(httpClient, settings, $"{GetBaseURL(settings)}/qualityprofile");

                var jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<IList<JSONProfile>>(jsonResponse);
            }
            catch (System.Exception ex)
            {
                logger.LogWarning(ex, "An error while getting Lidarr profiles: " + ex.Message);
            }

            throw new System.Exception("An error occurred while getting Lidarr profiles");
        }

        public static async Task<IList<JSONMetadataProfile>> GetMetadataProfiles(HttpClient httpClient, ILogger<LidarrClient> logger, LidarrSettings settings)
        {
            try
            {
                var response = await HttpGetAsync(httpClient, settings, $"{GetBaseURL(settings)}/metadataprofile");

                var jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<IList<JSONMetadataProfile>>(jsonResponse);
            }
            catch (System.Exception ex)
            {
                logger.LogWarning(ex, "An error while getting Lidarr profiles: " + ex.Message);
            }

            throw new System.Exception("An error occurred while getting Lidarr profiles");
        }

        public static async Task<IList<JSONTag>> GetTags(HttpClient httpClient, ILogger<LidarrClient> logger, LidarrSettings settings)
        {
            try
            {
                var response = await HttpGetAsync(httpClient, settings, $"{GetBaseURL(settings)}/tag");
                var jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<IList<JSONTag>>(jsonResponse);
            }
            catch (System.Exception ex)
            {
                logger.LogWarning(ex, "An error while getting Lidarr tags: " + ex.Message);
            }

            throw new System.Exception("An error occurred while getting Lidarr tags");
        }

        public async Task<Artist> SearchArtistByIdAsync(string mbId)
        {
            try
            {
                var foundArtistJson = await FindExistingArtistByMBIdAsync(mbId);

                if (foundArtistJson == null)
                {
                    var response = await HttpGetAsync($"{BaseURL}/artist/lookup?term=mbid:{mbId}");
                    await response.ThrowIfNotSuccessfulAsync("LidarrArtistLookupByMBId failed", x => x.error);

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    foundArtistJson = JsonConvert.DeserializeObject<JSONArtist>(jsonResponse);
                }

                return foundArtistJson != null ? Convert(foundArtistJson) : null;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while searching for a artist by MusicBrainz Id \"{mbId}\" with Lidarr: " + ex.Message);
            }

            throw new System.Exception("An error occurred while searching for a artist by MusicBrainz Id with Lidarr");
        }

        public async Task<IReadOnlyList<Artist>> SearchArtistAsync(string artistName)
        {
            try
            {
                var searchTerm = artistName.ToLower().Trim().Replace(" ", "+");
                var response = await HttpGetAsync($"{BaseURL}/artist/lookup?term={searchTerm}");
                await response.ThrowIfNotSuccessfulAsync("LidarrArtistLookup failed", x => x.error);

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var jsonArtists = JsonConvert.DeserializeObject<List<JSONArtist>>(jsonResponse).ToArray();

                return jsonArtists.Select(x => Convert(x)).ToArray();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occurred while searching for artists with Lidarr: " + ex.Message);
            }

            throw new System.Exception("An error occurred while searching for artists with Lidarr");
        }

        public async Task<Dictionary<string, Artist>> SearchAvailableArtistsAsync(HashSet<string> mbIds, System.Threading.CancellationToken token)
        {
            try
            {
                var convertedArtists = new List<Artist>();

                foreach (var mbId in mbIds)
                {
                    var existingArtist = await FindExistingArtistByMBIdAsync(mbId);

                    if (existingArtist != null)
                    {
                        convertedArtists.Add(Convert(existingArtist));
                    }
                }

                //TODO Fix for Available Only
                return convertedArtists.ToDictionary(x => x.MbId, x => x);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occurred while searching available artists with Lidarr: " + ex.Message);
            }

            throw new System.Exception("An error occurred while searching available artists with Lidarr");
        }

        public async Task<ArtistRequestResult> RequestArtistAsync(MusicUserRequester requester, Artist artist)
        {
            try
            {
                if (string.IsNullOrEmpty(artist.DownloadClientId))
                {
                    await CreateArtistInLidarr(artist);
                }
                else
                {
                    await UpdateExistingArtist(artist);
                }

                return new ArtistRequestResult();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"An error while requesting artist \"{artist.Name}\" from Lidarr: " + ex.Message);
            }

            throw new System.Exception("An error occurred while requesting a artist from Lidarr");
        }

        private async Task CreateArtistInLidarr(Artist artist)
        {
            int[] tags = LidarrSettings.MusicTags;

            var response = await HttpPostAsync($"{BaseURL}/artist", JsonConvert.SerializeObject(new
            {
                artistName = artist.Name,
                qualityProfileId = LidarrSettings.MusicProfileId,
                metadataProfileId = LidarrSettings.MusicMetadataProfileId,
                monitored = LidarrSettings.MonitorNewRequests,
                tags = tags,
                images = new string[0],
                foreignArtistId = artist.MbId,
                rootFolderPath = LidarrSettings.MusicRootFolder,
                addOptions = new
                {
                    ignoreEpisodesWithFiles = false,
                    ignoreEpisodesWithoutFiles = false,
                    SearchForMissingAlbums = LidarrSettings.SearchNewRequests
                }
            }));

            await response.ThrowIfNotSuccessfulAsync("LidarrArtistCreation failed", x => x.error);
        }

        private async Task UpdateExistingArtist(Artist artist)
        {
            var lidarrArtistId = int.Parse(artist.DownloadClientId);
            var response = await HttpGetAsync($"{BaseURL}/artist/{lidarrArtistId}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await CreateArtistInLidarr(artist);
                    return;
                }

                await response.ThrowIfNotSuccessfulAsync("LidarrGetArtist failed", x => x.error);
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            dynamic lidarrArtist = JObject.Parse(jsonResponse);

            lidarrArtist.tags = JToken.FromObject(LidarrSettings.MusicTags);
            lidarrArtist.qualityProfileId = LidarrSettings.MusicProfileId;
            lidarrArtist.metadataProfileId = LidarrSettings.MusicMetadataProfileId;
            lidarrArtist.monitored = LidarrSettings.MonitorNewRequests;

            response = await HttpPutAsync($"{BaseURL}/artist/{lidarrArtistId}", JsonConvert.SerializeObject(lidarrArtist));

            await response.ThrowIfNotSuccessfulAsync("LidarrUpdateArtist failed", x => x.error);

            if (LidarrSettings.SearchNewRequests)
            {
                try
                {
                    response = await HttpPostAsync($"{BaseURL}/command", JsonConvert.SerializeObject(new
                    {
                        name = "artistSearch",
                        artistId = lidarrArtistId,
                    }));

                    await response.ThrowIfNotSuccessfulAsync("LidarrArtistSearchCommand failed", x => x.error);
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, $"An error while sending search command for artist \"{artist.Name}\" to Lidarr: " + ex.Message);
                }
            }
        }

        private Artist Convert(JSONArtist jsonArtist)
        {
            var isMonitored = jsonArtist.monitored;

            var downloadClientId = jsonArtist.id?.ToString();

            return new Artist
            {
                DownloadClientId = downloadClientId,
                Name = jsonArtist.artistName,
                Disambiguation = jsonArtist.disambiguation,
                Overview = jsonArtist.overview,
                MbId = jsonArtist.foreignArtistId.ToString(),
                Quality = "",
                PlexUrl = "",
                EmbyUrl = "",
                Banner = GetPosterImageUrl(jsonArtist.images),
                IsRequested = isMonitored
            };
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

        private async Task<JSONArtist> FindExistingArtistByMBIdAsync(string mbId)
        {
            try
            {
                var response = await HttpGetAsync($"{BaseURL}/artist?mbid={mbId}");
                await response.ThrowIfNotSuccessfulAsync($"Could not search aritst by musicbrainz id {mbId}", x => x.error);

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var jsonArtists = JsonConvert.DeserializeObject<List<JSONArtist>>(jsonResponse).ToArray();

                if (jsonArtists.Any())
                {
                    return jsonArtists.First();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred finding an existing artist by musicbrainz id \"{mbId}\" with Lidarr: " + ex.Message);
            }

            return null;
        }

        private Task<HttpResponseMessage> HttpGetAsync(string url)
        {
            return HttpGetAsync(_httpClientFactory.CreateClient(), LidarrSettings, url);
        }

        private static async Task<HttpResponseMessage> HttpGetAsync(HttpClient client, LidarrSettings settings, string url)
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
            postRequest.Headers.Add("X-Api-Key", LidarrSettings.ApiKey);

            var client = _httpClientFactory.CreateClient();
            return await client.PostAsync(url, postRequest);
        }

        private async Task<HttpResponseMessage> HttpPutAsync(string url, string content)
        {
            var putRequest = new StringContent(content);
            putRequest.Headers.Clear();
            putRequest.Headers.Add("Content-Type", "application/json");
            putRequest.Headers.Add("X-Api-Key", LidarrSettings.ApiKey);

            var client = _httpClientFactory.CreateClient();
            return await client.PutAsync(url, putRequest);
        }

        private static string GetBaseURL(LidarrSettings settings)
        {
            var protocol = settings.UseSSL ? "https" : "http";

            return $"{protocol}://{settings.Hostname}:{settings.Port}{settings.BaseUrl}/api/v1";
        }

        private class JSONImage
        {
            public string coverType { get; set; }
            public string url { get; set; }
            public string remoteUrl { get; set; }
        }

        private class JSONArtist
        {
            public int? id { get; set; }
            public string artistName { get; set; }
            public string overview { get; set; }
            public List<JSONImage> images { get; set; }
            public string disambiguation { get; set; }
            public bool monitored { get; set; }
            public string foreignArtistId { get; set; }
        }
    }
}