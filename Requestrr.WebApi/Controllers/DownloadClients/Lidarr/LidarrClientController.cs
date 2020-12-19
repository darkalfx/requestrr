using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Requestrr.WebApi.config;
using Requestrr.WebApi.RequestrrBot.DownloadClients;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Lidarr;

namespace Requestrr.WebApi.Controllers.DownloadClients.Lidarr
{
    [ApiController]
    [Authorize]
    [Route("/api/music/lidarr")]
    public class LidarrClientController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<LidarrClient> _logger;

        public LidarrClientController(
            IHttpClientFactory httpClientFactory,
            ILogger<LidarrClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpPost("test")]
        public async Task<IActionResult> TestLidarrSettings([FromBody]TestLidarrSettingsModel model)
        {
            try
            {
                await LidarrClient.TestConnectionAsync(_httpClientFactory.CreateClient(), _logger, ConvertToLidarrSettings(model));

                return Ok(new { ok = true });
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("rootpath")]
        public async Task<IActionResult> GetLidarrRootPaths([FromBody]TestLidarrSettingsModel model)
        {
            try
            {
                var paths = await LidarrClient.GetRootPaths(_httpClientFactory.CreateClient(), _logger, ConvertToLidarrSettings(model));

                return Ok(paths.Select(x => new LidarrPath
                {
                    Id = x.id,
                    Path = x.path
                }));
            }
            catch (System.Exception)
            {
                return BadRequest($"Could not load the paths from Lidarr, check your settings.");
            }
        }

        [HttpPost("profile")]
        public async Task<IActionResult> GetLidarrProfiles([FromBody]TestLidarrSettingsModel model)
        {
            try
            {
                var profiles = await LidarrClient.GetProfiles(_httpClientFactory.CreateClient(), _logger, ConvertToLidarrSettings(model));

                return Ok(profiles.Select(x => new LidarrProfile
                {
                    Id = x.id,
                    Name = x.name
                }));
            }
            catch (System.Exception)
            {
                return BadRequest($"Could not load the profiles from Lidarr, check your settings.");
            }
        }

        [HttpPost("metadataprofile")]
        public async Task<IActionResult> GetLidarrMetadataProfiles([FromBody] TestLidarrSettingsModel model)
        {
            try
            {
                var profiles = await LidarrClient.GetMetadataProfiles(_httpClientFactory.CreateClient(), _logger, ConvertToLidarrSettings(model));

                return Ok(profiles.Select(x => new LidarrMetadataProfile
                {
                    Id = x.id,
                    Name = x.name
                }));
            }
            catch (System.Exception)
            {
                return BadRequest($"Could not load the metadata profiles from Lidarr, check your settings.");
            }
        }

        [HttpPost("tag")]
        public async Task<IActionResult> GetLidarrTags([FromBody]TestLidarrSettingsModel model)
        {
            try
            {
                var tags = await LidarrClient.GetTags(_httpClientFactory.CreateClient(), _logger, ConvertToLidarrSettings(model));

                return Ok(tags.Select(x => new LidarrTag
                {
                    Id = x.id,
                    Name = x.label
                }));
            }
            catch (System.Exception)
            {
                return BadRequest($"Could not load the tags from Lidarr, check your settings.");
            }
        }

        [HttpPost()]
        public async Task<IActionResult> SaveAsync([FromBody]LidarrSettingsModel model)
        {
            var tvShowsSettings = new MusicSettings
            {
                Client = DownloadClient.Lidarr,
                Command = model.Command.Trim(),
            };

            var sonarrSetting = new LidarrSettingsModel
            {
                Hostname = model.Hostname.Trim(),
                Port = model.Port,
                ApiKey = model.ApiKey.Trim(),
                BaseUrl = model.BaseUrl.Trim(),
                MusicPath = model.MusicPath,
                MusicProfile = model.MusicProfile,
                MusicMetadataProfile = model.MusicMetadataProfile,
                MusicTags = model.MusicTags ?? Array.Empty<int>(),
                MusicUseAlbumFolders = model.MusicUseAlbumFolders,
                SearchNewRequests = model.SearchNewRequests,
                MonitorNewRequests = model.MonitorNewRequests,
                UseSSL = model.UseSSL
            };

            DownloadClientsSettingsRepository.SetLidarr(tvShowsSettings, sonarrSetting);

            return Ok(new { ok = true });
        }

        private static RequestrrBot.DownloadClients.Lidarr.LidarrSettings ConvertToLidarrSettings(TestLidarrSettingsModel model)
        {
            return new RequestrrBot.DownloadClients.Lidarr.LidarrSettings
            {
                ApiKey = model.ApiKey.Trim(),
                Hostname = model.Hostname.Trim(),
                BaseUrl = model.BaseUrl.Trim(),
                Port = model.Port,
                UseSSL = model.UseSSL
            };
        }
    }
}
