using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Requestrr.WebApi.config;
using Requestrr.WebApi.Controllers.DownloadClients.Ombi;
using Requestrr.WebApi.Controllers.DownloadClients.Radarr;
using Requestrr.WebApi.RequestrrBot.DownloadClients;

namespace Requestrr.WebApi.Controllers.DownloadClients
{
    [ApiController]
    [Authorize]
    [Route("/api/movies")]
    public class MovieDownloadClientController : ControllerBase
    {
        private readonly MoviesSettings _moviesSettings;
        private readonly DownloadClientsSettings _downloadClientsSettings;
        private readonly IHttpClientFactory _httpClientFactory;

        public MovieDownloadClientController(
            IHttpClientFactory httpClientFactory,
            IOptionsSnapshot<MoviesSettings> moviesSettingsAccessor,
            IOptionsSnapshot<DownloadClientsSettings> botClientsSettingsAccessor)
        {
            _moviesSettings = moviesSettingsAccessor.Value;
            _downloadClientsSettings = botClientsSettingsAccessor.Value;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet()]
        public async Task<IActionResult> GetAsync()
        {
            return Ok(new MovieSettingsModel
            {
                Client = _moviesSettings.Client,
                Radarr = new RadarrSettingsModel
                {
                    Hostname = _downloadClientsSettings.Radarr.Hostname,
                    BaseUrl = _downloadClientsSettings.Radarr.BaseUrl,
                    Port = _downloadClientsSettings.Radarr.Port,
                    ApiKey = _downloadClientsSettings.Radarr.ApiKey,
                    MoviePath = _downloadClientsSettings.Radarr.MovieRootFolder,
                    MovieProfile = _downloadClientsSettings.Radarr.MovieProfileId,
                    MovieMinAvailability = _downloadClientsSettings.Radarr.MovieMinimumAvailability,
                    MovieTags = _downloadClientsSettings.Radarr.MovieTags ?? Array.Empty<int>(),
                    AnimePath = _downloadClientsSettings.Radarr.AnimeRootFolder,
                    AnimeProfile = _downloadClientsSettings.Radarr.AnimeProfileId,
                    AnimeMinAvailability = _downloadClientsSettings.Radarr.AnimeMinimumAvailability,
                    AnimeTags = _downloadClientsSettings.Radarr.AnimeTags ?? Array.Empty<int>(),
                    UseSSL = _downloadClientsSettings.Radarr.UseSSL,
                    SearchNewRequests = _downloadClientsSettings.Radarr.SearchNewRequests,
                    MonitorNewRequests = _downloadClientsSettings.Radarr.MonitorNewRequests,
                    Version = _downloadClientsSettings.Radarr.Version
                },
                Ombi = new OmbiSettingsModel
                {
                    Hostname = _downloadClientsSettings.Ombi.Hostname,
                    BaseUrl = _downloadClientsSettings.Ombi.BaseUrl,
                    Port = _downloadClientsSettings.Ombi.Port,
                    ApiKey = _downloadClientsSettings.Ombi.ApiKey,
                    ApiUsername = _downloadClientsSettings.Ombi.ApiUsername,
                    UseSSL = _downloadClientsSettings.Ombi.UseSSL,
                    Version = _downloadClientsSettings.Ombi.Version
                },
                Command = _moviesSettings.Command
            });
        }

        [HttpPost("disable")]
        public async Task<IActionResult> SaveAsync()
        {
            _moviesSettings.Client = DownloadClient.Disabled;
            DownloadClientsSettingsRepository.SetDisabledClient(_moviesSettings);
            return Ok(new { ok = true });
        }
    }
}
