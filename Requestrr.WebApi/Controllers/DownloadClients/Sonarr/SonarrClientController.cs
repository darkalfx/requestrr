using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Requestrr.WebApi.RequestrrBot.DownloadClients;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Sonarr;
using Requestrr.WebApi.RequestrrBot.TvShows;

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
        public async Task<IActionResult> TestSonarrSettings([FromBody] TestSonarrSettingsModel model)
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
        public async Task<IActionResult> GetSonarrRootPaths([FromBody] TestSonarrSettingsModel model)
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
        public async Task<IActionResult> GetSonarrProfiles([FromBody] TestSonarrSettingsModel model)
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
        public async Task<IActionResult> GetSonarrLanguages([FromBody] TestSonarrSettingsModel model)
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
        public async Task<IActionResult> GetSonarrTags([FromBody] TestSonarrSettingsModel model)
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
        public async Task<IActionResult> SaveAsync([FromBody] SaveSonarrSettingsModel model)
        {
            var tvShowsSettings = new TvShowsSettings
            {
                Client = DownloadClient.Sonarr,
                Restrictions = model.Restrictions
            };

            if (!model.Categories.Any())
            {
                return BadRequest($"At least one category is required.");
            }

            if (model.Categories.Any(x => string.IsNullOrWhiteSpace(x.Name)))
            {
                return BadRequest($"A category name is required.");
            }

            foreach (var category in model.Categories)
            {
                category.Name = category.Name.Trim();
                category.Tags = category.Tags;
            }

            if (new HashSet<string>(model.Categories.Select(x => x.Name.ToLower())).Count != model.Categories.Length)
            {
                return BadRequest($"All categories must have different names.");
            }

            if (new HashSet<int>(model.Categories.Select(x => x.Id)).Count != model.Categories.Length)
            {
                return BadRequest($"All categories must have different ids.");
            }

            if (model.Categories.Any(x => !Regex.IsMatch(x.Name, @"^[\w-]{1,32}$")))
            {
                return BadRequest($"Invalid categorie names, make sure they only contain alphanumeric characters, dashes and underscores. (No spaces, etc)");
            }

            var sonarrSetting = new SonarrSettingsModel
            {
                Hostname = model.Hostname.Trim(),
                Port = model.Port,
                ApiKey = model.ApiKey.Trim(),
                BaseUrl = model.BaseUrl.Trim(),
                Categories = model.Categories,
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
