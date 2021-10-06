using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Requestrr.WebApi.config;
using Requestrr.WebApi.Controllers.DownloadClients.Ombi;
using Requestrr.WebApi.Controllers.DownloadClients.Overseerr;
using Requestrr.WebApi.Controllers.DownloadClients.Radarr;
using Requestrr.WebApi.RequestrrBot.DownloadClients;
using Requestrr.WebApi.RequestrrBot.Movies;
using RadarrSettingsCategory = Requestrr.WebApi.Controllers.DownloadClients.Radarr.RadarrSettingsCategory;

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
            MoviesSettingsProvider moviesSettingsProvider,
            DownloadClientsSettingsProvider downloadClientsSettingsProvider)
        {
            _moviesSettings = moviesSettingsProvider.Provide();
            _downloadClientsSettings = downloadClientsSettingsProvider.Provide();
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
                    Categories = _downloadClientsSettings.Radarr.Categories.Select(x => new RadarrSettingsCategory
                    {
                        Id = x.Id,
                        Name = x.Name,
                        MinimumAvailability = x.MinimumAvailability,
                        ProfileId = x.ProfileId,
                        RootFolder = x.RootFolder,
                        Tags = x.Tags
                    }).ToArray(),
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
                Overseerr = new OverseerrSettingsModel
                {
                    Hostname = _downloadClientsSettings.Overseerr.Hostname,
                    Port = _downloadClientsSettings.Overseerr.Port,
                    ApiKey = _downloadClientsSettings.Overseerr.ApiKey,
                    DefaultApiUserID = _downloadClientsSettings.Overseerr.DefaultApiUserID,
                    UseSSL = _downloadClientsSettings.Overseerr.UseSSL,
                    Version = _downloadClientsSettings.Overseerr.Version
                }
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
