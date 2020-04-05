using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Requestrr.WebApi.config;
using Requestrr.WebApi.RequestrrBot.DownloadClients;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Radarr;

namespace Requestrr.WebApi.Controllers.DownloadClients.Radarr
{
    [ApiController]
    [Authorize]
    [Route("/api/movies/radarr")]
    public class RadarrClientController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<RadarrClient> _logger;

        public RadarrClientController(
            IHttpClientFactory httpClientFactory,
            ILogger<RadarrClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpPost("test")]
        public async Task<IActionResult> TestRadarrSettings([FromBody]TestRadarrSettingsModel model)
        {
            try
            {
                await RadarrClient.TestConnectionAsync(_httpClientFactory.CreateClient(), _logger, ConvertToRadarrSettings(model));

                return Ok(new { ok = true });
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("rootpath")]
        public async Task<IActionResult> GetRadarrRootPaths([FromBody]TestRadarrSettingsModel model)
        {
            try
            {
                var paths = await RadarrClient.GetRootPaths(_httpClientFactory.CreateClient(), _logger, ConvertToRadarrSettings(model));

                return Ok(paths.Select(x => new RadarrPath
                {
                    Id = x.id,
                    Path = x.path
                }));
            }
            catch (System.Exception)
            {
                return BadRequest($"Could not load the paths from Radarr, check your settings.");
            }
        }

        [HttpPost("profile")]
        public async Task<IActionResult> GetRadarrProfiles([FromBody]TestRadarrSettingsModel model)
        {
            try
            {
                var profiles = await RadarrClient.GetProfiles(_httpClientFactory.CreateClient(), _logger, ConvertToRadarrSettings(model));

                return Ok(profiles.Select(x => new RadarrProfile
                {
                    Id = x.id,
                    Name = x.name
                }));
            }
            catch (System.Exception)
            {
                return BadRequest($"Could not load the profiles from Radarr, check your settings.");
            }
        }

        [HttpPost("tag")]
        public async Task<IActionResult> GetRadarrTags([FromBody]TestRadarrSettingsModel model)
        {
            try
            {
                var tags = await RadarrClient.GetTags(_httpClientFactory.CreateClient(), _logger, ConvertToRadarrSettings(model));

                return Ok(tags.Select(x => new RadarrTag
                {
                    Id = x.id,
                    Name = x.label
                }));
            }
            catch (System.Exception)
            {
                return BadRequest($"Could not load the tags from Radarr, check your settings.");
            }
        }

        [HttpPost()]
        public async Task<IActionResult> SaveAsync([FromBody]RadarrSettingsModel model)
        {
            var movieSettings = new MoviesSettings
            {
                Client = DownloadClient.Radarr,
                Command = model.Command.Trim()
            };

            var radarrSetting = new RadarrSettingsModel
            {
                Hostname = model.Hostname.Trim(),
                ApiKey = model.ApiKey.Trim(),
                BaseUrl = model.BaseUrl.Trim(),
                Port = model.Port,
                MoviePath = model.MoviePath,
                MovieProfile = model.MovieProfile,
                MovieMinAvailability = model.MovieMinAvailability,
                MovieTags = model.MovieTags ?? Array.Empty<int>(),
                AnimePath = model.AnimePath,
                AnimeProfile = model.AnimeProfile,
                AnimeMinAvailability = model.AnimeMinAvailability,
                AnimeTags = model.AnimeTags ?? Array.Empty<int>(),
                SearchNewRequests = model.SearchNewRequests,
                MonitorNewRequests = model.MonitorNewRequests,
                UseSSL = model.UseSSL,
                Version = model.Version
            };

            DownloadClientsSettingsRepository.SetRadarr(movieSettings, radarrSetting);

            return Ok(new { ok = true });
        }

        private static RequestrrBot.DownloadClients.Radarr.RadarrSettings ConvertToRadarrSettings(TestRadarrSettingsModel model)
        {
            return new RequestrrBot.DownloadClients.Radarr.RadarrSettings
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
