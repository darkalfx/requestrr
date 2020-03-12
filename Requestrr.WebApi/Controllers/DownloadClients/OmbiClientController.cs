using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Requestrr.WebApi.Config;
using Requestrr.WebApi.Requestrr.DownloadClients;

public class TestOmbiSettingsModel
{
    [Required]
    public string Hostname { get; set; }
    [Required]
    public int Port { get; set; }
    [Required]
    public string ApiKey { get; set; }
    [Required]
    public bool UseSSL { get; set; }
    [Required]
    public string Version { get; set; }
}

public class SaveOmbiSettingsModel
{
    [Required]
    public string Hostname { get; set; }
    [Required]
    public int Port { get; set; }
    [Required]
    public string ApiKey { get; set; }
    public string ApiUsername { get; set; }
    [Required]
    public bool UseSSL { get; set; }
    [Required]
    public string Version { get; set; }
    [Required]
    public string Command { get; set; }
}

namespace Requestrr.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    public class OmbiClientController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<Ombi> _logger;

        public OmbiClientController(
            IHttpClientFactory httpClientFactory,
            ILogger<Ombi> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpPost("/api/movies/ombi/test")]
        [HttpPost("/api/tvshows/ombi/test")]
        public async Task<IActionResult> TestOmbiSettings([FromBody]TestOmbiSettingsModel model)
        {
            try
            {
                await Ombi.TestConnectionAsync(_httpClientFactory.CreateClient(), _logger, new Requestrr.DownloadClients.OmbiSettings
                {
                    ApiKey = model.ApiKey.Trim(),
                    Hostname = model.Hostname.Trim(),
                    Port = model.Port,
                    UseSSL = model.UseSSL,
                    Version = model.Version,
                });

                return Ok(new { ok = true });
            }
            catch (System.Exception)
            {
                return BadRequest($"The specified settings are invalid");
            }
        }

        [HttpPost("/api/movies/ombi")]
        public async Task<IActionResult> SaveMoviesAsync([FromBody]SaveOmbiSettingsModel model)
        {
            var movieSettings = new MoviesSettings
            {
                Client = DownloadClient.Ombi,
                Command = model.Command.Trim(),
            };

            var ombiSettings = ConvertToOmbiSettings(model);

            DownloadClientsSettingsRepository.SetOmbi(movieSettings, ombiSettings);

            return Ok(new { ok = true });
        }

        [HttpPost("/api/tvshows/ombi")]
        public async Task<IActionResult> SaveTvShowsAsync([FromBody]SaveOmbiSettingsModel model)
        {
            var tvShowsSettings = new TvShowsSettings
            {
                Client = DownloadClient.Ombi,
                Command = model.Command.Trim(),
            };

            var ombiSettings = ConvertToOmbiSettings(model);

            DownloadClientsSettingsRepository.SetOmbi(tvShowsSettings, ombiSettings);

            return Ok(new { ok = true });
        }

        private static OmbiSettings ConvertToOmbiSettings(SaveOmbiSettingsModel model)
        {
            return new OmbiSettings
            {
                Hostname = model.Hostname.Trim(),
                ApiKey = model.ApiKey.Trim(),
                ApiUsername = model.ApiUsername.Trim(),
                Port = model.Port,
                UseSSL = model.UseSSL,
                Version = model.Version
            };
        }
    }
}
