using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Requestrr.WebApi.RequestrrBot.DownloadClients;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Overseerr;
using Requestrr.WebApi.RequestrrBot.Movies;
using Requestrr.WebApi.RequestrrBot.TvShows;

namespace Requestrr.WebApi.Controllers.DownloadClients.Overseerr
{

    [ApiController]
    [Authorize]
    public class OverseerrClientController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OverseerrClient> _logger;

        public OverseerrClientController(
            IHttpClientFactory httpClientFactory,
            ILogger<OverseerrClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpPost("/api/movies/overseerr/test")]
        [HttpPost("/api/tvshows/overseerr/test")]
        public async Task<IActionResult> TestOverseerrSettings([FromBody] TestOverseerrSettingsModel model)
        {
            try
            {
                await OverseerrClient.TestConnectionAsync(_httpClientFactory.CreateClient(), _logger, new RequestrrBot.DownloadClients.Overseerr.OverseerrTestSettings
                {
                    ApiKey = model.ApiKey.Trim(),
                    Hostname = model.Hostname.Trim(),
                    Port = model.Port,
                    UseSSL = model.UseSSL,
                    DefaultApiUserId = model.DefaultApiUserID,
                    Version = model.Version,
                });

                return Ok(new { ok = true });
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("/api/movies/overseerr/radarr")]
        public async Task<ActionResult<RadarrServiceSettings>> GetOverseerrRadarrServiceSettings([FromBody] TestOverseerrSettingsModel model)
        {
            try
            {
                var radarrServiceSettings = await OverseerrClient.GetRadarrServiceSettingsAsync(_httpClientFactory.CreateClient(), _logger, new RequestrrBot.DownloadClients.Overseerr.OverseerrTestSettings
                {
                    ApiKey = model.ApiKey.Trim(),
                    Hostname = model.Hostname.Trim(),
                    Port = model.Port,
                    UseSSL = model.UseSSL,
                    DefaultApiUserId = model.DefaultApiUserID,
                    Version = model.Version,
                });

                return radarrServiceSettings;
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("/api/tvshows/overseerr/sonarr")]
        public async Task<ActionResult<SonarrServiceSettings>> GetOverseerrSonarrServiceSettings([FromBody] TestOverseerrSettingsModel model)
        {
            try
            {
                var sonarrServiceSettings = await OverseerrClient.GetSonarrServiceSettingsAsync(_httpClientFactory.CreateClient(), _logger, new RequestrrBot.DownloadClients.Overseerr.OverseerrTestSettings
                {
                    ApiKey = model.ApiKey.Trim(),
                    Hostname = model.Hostname.Trim(),
                    Port = model.Port,
                    UseSSL = model.UseSSL,
                    DefaultApiUserId = model.DefaultApiUserID,
                    Version = model.Version,
                });

                return sonarrServiceSettings;
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("/api/movies/overseerr")]
        public async Task<IActionResult> SaveMoviesAsync([FromBody] SaveOverseerrMoviesSettingsModel model)
        {
            var movieSettings = new MoviesSettings
            {
                Client = DownloadClient.Overseerr
            };

            Sanitize(model);

            if (model.Movies.Categories.Any(x => string.IsNullOrWhiteSpace(x.Name)))
            {
                return BadRequest($"A category name is required.");
            }

            foreach (var category in model.Movies.Categories)
            {
                category.Name = category.Name.Trim();
                category.Tags = category.Tags;
            }

            if (new HashSet<string>(model.Movies.Categories.Select(x => x.Name.ToLower())).Count != model.Movies.Categories.Length)
            {
                return BadRequest($"All categories must have different names.");
            }

            if (new HashSet<int>(model.Movies.Categories.Select(x => x.Id)).Count != model.Movies.Categories.Length)
            {
                return BadRequest($"All categories must have different ids.");
            }

            if (model.Movies.Categories.Any(x => !Regex.IsMatch(x.Name, @"^[\w-]{1,32}$")))
            {
                return BadRequest($"Invalid categorie names, make sure they only contain alphanumeric characters, dashes and underscores. (No spaces, etc)");
            }

            DownloadClientsSettingsRepository.SetOverseerr(movieSettings, model);

            return Ok(new { ok = true });
        }

        [HttpPost("/api/tvshows/overseerr")]
        public async Task<IActionResult> SaveTvShowsAsync([FromBody] SaveOverseerrTvShowsSettingsModel model)
        {
            var tvShowsSettings = new TvShowsSettings
            {
                Client = DownloadClient.Overseerr,
                Restrictions = model.Restrictions
            };

            Sanitize(model);

            if (model.TvShows.Categories.Any(x => string.IsNullOrWhiteSpace(x.Name)))
            {
                return BadRequest($"A category name is required.");
            }

            foreach (var category in model.TvShows.Categories)
            {
                category.Name = category.Name.Trim();
                category.Tags = category.Tags;
            }

            if (new HashSet<string>(model.TvShows.Categories.Select(x => x.Name.ToLower())).Count != model.TvShows.Categories.Length)
            {
                return BadRequest($"All categories must have different names.");
            }

            if (new HashSet<int>(model.TvShows.Categories.Select(x => x.Id)).Count != model.TvShows.Categories.Length)
            {
                return BadRequest($"All categories must have different ids.");
            }

            if (model.TvShows.Categories.Any(x => !Regex.IsMatch(x.Name, @"^[\w-]{1,32}$")))
            {
                return BadRequest($"Invalid categorie names, make sure they only contain alphanumeric characters, dashes and underscores. (No spaces, etc)");
            }

            DownloadClientsSettingsRepository.SetOverseerr(tvShowsSettings, model);

            return Ok(new { ok = true });
        }

        private static void Sanitize<T>(T model) where T : OverseerrSettingsModel
        {
            model.Hostname = model.Hostname.Trim();
            model.ApiKey = model.ApiKey.Trim();
        }
    }
}