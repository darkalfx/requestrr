using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Requestrr.WebApi.config;
using Requestrr.WebApi.RequestrrBot.DownloadClients;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Radarr;
using Requestrr.WebApi.RequestrrBot.Movies;

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
        public async Task<IActionResult> TestRadarrSettings([FromBody] TestRadarrSettingsModel model)
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
        public async Task<IActionResult> GetRadarrRootPaths([FromBody] TestRadarrSettingsModel model)
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
        public async Task<IActionResult> GetRadarrProfiles([FromBody] TestRadarrSettingsModel model)
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
        public async Task<IActionResult> GetRadarrTags([FromBody] TestRadarrSettingsModel model)
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
        public async Task<IActionResult> SaveAsync([FromBody] RadarrSettingsModel model)
        {
            var movieSettings = new MoviesSettings
            {
                Client = DownloadClient.Radarr
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

            var radarrSetting = new RadarrSettingsModel
            {
                Hostname = model.Hostname.Trim(),
                ApiKey = model.ApiKey.Trim(),
                BaseUrl = model.BaseUrl.Trim(),
                Port = model.Port,
                Categories = model.Categories,
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
