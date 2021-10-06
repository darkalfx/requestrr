using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Requestrr.WebApi.config;
using Requestrr.WebApi.Controllers.DownloadClients.Ombi;
using Requestrr.WebApi.Controllers.DownloadClients.Overseerr;
using Requestrr.WebApi.Controllers.DownloadClients.Sonarr;
using Requestrr.WebApi.RequestrrBot.DownloadClients;
using Requestrr.WebApi.RequestrrBot.TvShows;
using SonarrSettingsCategory = Requestrr.WebApi.Controllers.DownloadClients.Sonarr.SonarrSettingsCategory;

namespace Requestrr.WebApi.Controllers.DownloadClients
{
    [ApiController]
    [Authorize]
    [Route("/api/tvshows")]
    public class TvShowsDownloadClientController : ControllerBase
    {
        private readonly TvShowsSettings _tvShowsSettings;
        private readonly DownloadClientsSettings _downloadClientsSettings;
        private readonly IHttpClientFactory _httpClientFactory;

        public TvShowsDownloadClientController(
            IHttpClientFactory httpClientFactory,
            TvShowsSettingsProvider tvShowsSettingsProvider,
            DownloadClientsSettingsProvider downloadClientsSettingsProvider)
        {
            _tvShowsSettings = tvShowsSettingsProvider.Provide();
            _downloadClientsSettings = downloadClientsSettingsProvider.Provide();
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet()]
        public async Task<IActionResult> GetAsync()
        {
            return Ok(new TvShowsSettingsModel
            {
                Client = _tvShowsSettings.Client,
                Sonarr = new SonarrSettingsModel
                {
                    Hostname = _downloadClientsSettings.Sonarr.Hostname,
                    BaseUrl = _downloadClientsSettings.Sonarr.BaseUrl,
                    Port = _downloadClientsSettings.Sonarr.Port,
                    ApiKey = _downloadClientsSettings.Sonarr.ApiKey,
                    Categories = _downloadClientsSettings.Sonarr.Categories.Select(x => new SonarrSettingsCategory
                    {
                        Id = x.Id,
                        Name = x.Name,
                        LanguageId = x.LanguageId,
                        ProfileId = x.ProfileId,
                        RootFolder = x.RootFolder,
                        Tags = x.Tags,
                        UseSeasonFolders = x.UseSeasonFolders,
                        SeriesType = x.SeriesType,
                    }).ToArray(),
                    UseSSL = _downloadClientsSettings.Sonarr.UseSSL,
                    SearchNewRequests = _downloadClientsSettings.Sonarr.SearchNewRequests,
                    MonitorNewRequests = _downloadClientsSettings.Sonarr.MonitorNewRequests,
                    Version = _downloadClientsSettings.Sonarr.Version
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
                },
                Restrictions = _tvShowsSettings.Restrictions
            });
        }

        [HttpPost("disable")]
        public async Task<IActionResult> SaveAsync()
        {
            _tvShowsSettings.Client = DownloadClient.Disabled;
            DownloadClientsSettingsRepository.SetDisabledClient(_tvShowsSettings);
            return Ok(new { ok = true });
        }
    }
}
