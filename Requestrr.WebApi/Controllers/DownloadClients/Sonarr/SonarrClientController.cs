using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Requestrr.WebApi.config;
using Requestrr.WebApi.RequestrrBot.DownloadClients;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Sonarr;

namespace Requestrr.WebApi.Controllers.DownloadClients.Sonarr
{
    [ApiController]
    [Authorize]
    [Route("/api/tvshows/sonarr")]
    public class SonarrClientController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SonarrClient> _logger;

        public SonarrClientController(
            IHttpClientFactory httpClientFactory,
            ILogger<SonarrClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpPost("test")]
        public async Task<IActionResult> TestSonarrSettings([FromBody]TestSonarrSettingsModel model)
        {
            try
            {
                await SonarrClient.TestConnectionAsync(_httpClientFactory.CreateClient(), _logger, ConvertToSonarrSettings(model));

                return Ok(new { ok = true });
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("rootpath")]
        public async Task<IActionResult> GetSonarrRootPaths([FromBody]TestSonarrSettingsModel model)
        {
            try
            {
                var paths = await SonarrClient.GetRootPaths(_httpClientFactory.CreateClient(), _logger, ConvertToSonarrSettings(model));

                return Ok(paths.Select(x => new SonarrPath
                {
                    Id = x.id,
                    Path = x.path
                }));
            }
            catch (System.Exception)
            {
                return BadRequest($"Could not load the paths from Sonarr, check your settings.");
            }
        }

        [HttpPost("profile")]
        public async Task<IActionResult> GetSonarrProfiles([FromBody]TestSonarrSettingsModel model)
        {
            try
            {
                var profiles = await SonarrClient.GetProfiles(_httpClientFactory.CreateClient(), _logger, ConvertToSonarrSettings(model));

                return Ok(profiles.Select(x => new SonarrProfile
                {
                    Id = x.id,
                    Name = x.name
                }));
            }
            catch (System.Exception)
            {
                return BadRequest($"Could not load the profiles from Sonarr, check your settings.");
            }
        }

        [HttpPost("language")]
        public async Task<IActionResult> GetSonarrLanguages([FromBody]TestSonarrSettingsModel model)
        {
            try
            {
                var profiles = await SonarrClient.GetLanguages(_httpClientFactory.CreateClient(), _logger, ConvertToSonarrSettings(model));

                return Ok(profiles.Select(x => new SonarrLanguage
                {
                    Id = x.id,
                    Name = x.name
                }));
            }
            catch (System.Exception)
            {
                return BadRequest($"Could not load the profiles from Sonarr, check your settings.");
            }
        }

        [HttpPost("tag")]
        public async Task<IActionResult> GetSonarrTags([FromBody]TestSonarrSettingsModel model)
        {
            try
            {
                var tags = await SonarrClient.GetTags(_httpClientFactory.CreateClient(), _logger, ConvertToSonarrSettings(model));

                return Ok(tags.Select(x => new SonarrTag
                {
                    Id = x.id,
                    Name = x.label
                }));
            }
            catch (System.Exception)
            {
                return BadRequest($"Could not load the tags from Sonarr, check your settings.");
            }
        }

        [HttpPost()]
        public async Task<IActionResult> SaveAsync([FromBody]SonarrSettingsModel model)
        {
            var tvShowsSettings = new TvShowsSettings
            {
                Client = DownloadClient.Sonarr,
                Command = model.Command.Trim()
            };

            var sonarrSetting = new SonarrSettingsModel
            {
                Hostname = model.Hostname.Trim(),
                Port = model.Port,
                ApiKey = model.ApiKey.Trim(),
                BaseUrl = model.BaseUrl.Trim(),
                TvPath = model.TvPath,
                TvProfile = model.TvProfile,
                TvTags = model.TvTags ?? Array.Empty<int>(),
                TvLanguage = model.TvLanguage,
                TvUseSeasonFolders = model.TvUseSeasonFolders,
                AnimePath = model.AnimePath,
                AnimeProfile = model.AnimeProfile,
                AnimeTags = model.AnimeTags ?? Array.Empty<int>(),
                AnimeLanguage = model.AnimeLanguage,
                AnimeUseSeasonFolders = model.AnimeUseSeasonFolders,
                SearchNewRequests = model.SearchNewRequests,
                MonitorNewRequests = model.MonitorNewRequests,
                UseSSL = model.UseSSL,
                Version = model.Version
            };

            DownloadClientsSettingsRepository.SetSonarr(tvShowsSettings, sonarrSetting);

            return Ok(new { ok = true });
        }

        private static RequestrrBot.DownloadClients.Sonarr.SonarrSettings ConvertToSonarrSettings(TestSonarrSettingsModel model)
        {
            return new RequestrrBot.DownloadClients.Sonarr.SonarrSettings
            {
                ApiKey = model.ApiKey.Trim(),
                Hostname = model.Hostname.Trim(),
                BaseUrl = model.BaseUrl.Trim(),
                Port = model.Port,
                UseSSL = model.UseSSL,
                Version = model.Version
            };
        }
    }
}
