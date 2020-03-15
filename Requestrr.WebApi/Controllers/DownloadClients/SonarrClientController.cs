using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Requestrr.WebApi.Config;
using Requestrr.WebApi.Requestrr.DownloadClients;

namespace Requestrr.WebApi.Controllers
{
    public class TestSonarrSettingsModel
    {
        [Required]
        public string Hostname { get; set; }
        [Required]
        public int Port { get; set; }
        [Required]
        public string ApiKey { get; set; }
        [Required]
        public string BaseUrl { get; set; }
        [Required]
        public bool UseSSL { get; set; }
        [Required]
        public string Version { get; set; }
    }

    public class SaveSonarrSettingsModel
    {
        [Required]
        public string Hostname { get; set; }
        [Required]
        public int Port { get; set; }
        [Required]
        public string ApiKey { get; set; }
        [Required]
        public string BaseUrl { get; set; }
        [Required]
        public string TvPath { get; set; }
        [Required]
        public int TvProfile { get; set; }
        public int[] TvTags { get; set; }
        [Required]
        public int TvLanguage { get; set; }
        public bool TvUseSeasonFolders { get; set; }
        public string AnimePath { get; set; }
        [Required]
        public int AnimeProfile { get; set; }
        public int[] AnimeTags { get; set; }
        [Required]
        public int AnimeLanguage { get; set; }
        public bool AnimeUseSeasonFolders { get; set; }
        public bool UseSSL { get; set; }
        public bool SearchNewRequests { get; set; }
        public bool MonitorNewRequests { get; set; }
        public bool AllowRequestUpdates { get; set; }
        [Required]
        public string Version { get; set; }
        [Required]
        public string Command { get; set; }
    }

    public class SonarrProfile
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
    }

    public class SonarrLanguage
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
    }

    public class SonarrTag
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
    }

    public class SonarrPath
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Path { get; set; }
    }

    [ApiController]
    [Authorize]
    [Route("/api/tvshows/sonarr")]
    public class SonarrClientController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<Sonarr> _logger;

        public SonarrClientController(
            IHttpClientFactory httpClientFactory,
            ILogger<Sonarr> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpPost("test")]
        public async Task<IActionResult> TestSonarrSettings([FromBody]TestSonarrSettingsModel model)
        {
            try
            {
                await Sonarr.TestConnectionAsync(_httpClientFactory.CreateClient(), _logger, ConvertToSonarrSettings(model));

                return Ok(new { ok = true });
            }
            catch (System.Exception)
            {
                return BadRequest($"The specified settings are invalid");
            }
        }

        [HttpPost("rootpath")]
        public async Task<IActionResult> GetSonarrRootPaths([FromBody]TestSonarrSettingsModel model)
        {
            try
            {
                var paths = await Sonarr.GetRootPaths(_httpClientFactory.CreateClient(), _logger, ConvertToSonarrSettings(model));

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
                var profiles = await Sonarr.GetProfiles(_httpClientFactory.CreateClient(), _logger, ConvertToSonarrSettings(model));

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
                var profiles = await Sonarr.GetLanguages(_httpClientFactory.CreateClient(), _logger, ConvertToSonarrSettings(model));

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
                var tags = await Sonarr.GetTags(_httpClientFactory.CreateClient(), _logger, ConvertToSonarrSettings(model));

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
        public async Task<IActionResult> SaveAsync([FromBody]SaveSonarrSettingsModel model)
        {
            var tvShowsSettings = new TvShowsSettings
            {
                Client = DownloadClient.Sonarr,
                Command = model.Command.Trim()
            };

            var sonarrSetting = new SonarrSettings
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
                AllowRequestUpdates = model.AllowRequestUpdates,
                UseSSL = model.UseSSL,
                Version = model.Version
            };

            DownloadClientsSettingsRepository.SetSonarr(tvShowsSettings, sonarrSetting);

            return Ok(new { ok = true });
        }

        private static Requestrr.DownloadClients.SonarrSettings ConvertToSonarrSettings(TestSonarrSettingsModel model)
        {
            return new Requestrr.DownloadClients.SonarrSettings
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
