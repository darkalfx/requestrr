using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Requestrr.WebApi.config;
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
        public async Task<IActionResult> TestOverseerrSettings([FromBody]TestOverseerrSettingsModel model)
        {
            try
            {
                await OverseerrClient.TestConnectionAsync(_httpClientFactory.CreateClient(), _logger, new RequestrrBot.DownloadClients.Overseerr.OverseerrSettings
                {
                    ApiKey = model.ApiKey.Trim(),
                    Hostname = model.Hostname.Trim(),
                    Port = model.Port,
                    UseSSL = model.UseSSL,
                    DefaultApiUserID = model.DefaultApiUserID,
                    Version = model.Version,
                });

                return Ok(new { ok = true });
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("/api/movies/overseerr")]
        public async Task<IActionResult> SaveMoviesAsync([FromBody]SaveOverseerrMoviesSettingsModel model)
        {
            var movieSettings = new MoviesSettings
            {
                Client = DownloadClient.Overseerr
            };

            var overseerrSettings = Sanitize(model);

            DownloadClientsSettingsRepository.SetOverseerr(movieSettings, overseerrSettings);

            return Ok(new { ok = true });
        }

        [HttpPost("/api/tvshows/overseerr")]
        public async Task<IActionResult> SaveTvShowsAsync([FromBody]SaveOverseerrTvShowsSettingsModel model)
        {
            var tvShowsSettings = new TvShowsSettings
            {
                Client = DownloadClient.Overseerr,
                Restrictions = model.Restrictions
            };

            var overseerrSettings = Sanitize(model);

            DownloadClientsSettingsRepository.SetOverseerr(tvShowsSettings, overseerrSettings);

            return Ok(new { ok = true });
        }

        private static OverseerrSettingsModel Sanitize(OverseerrSettingsModel model)
        {
            return new OverseerrSettingsModel
            {
                Hostname = model.Hostname.Trim(),
                ApiKey = model.ApiKey.Trim(),
                DefaultApiUserID = model.DefaultApiUserID.Trim(),
                Port = model.Port,
                UseSSL = model.UseSSL,
                Version = model.Version
            };
        }
    }
}