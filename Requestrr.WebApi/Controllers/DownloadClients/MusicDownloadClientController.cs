using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Requestrr.WebApi.config;
using Requestrr.WebApi.Controllers.DownloadClients.Lidarr;
using Requestrr.WebApi.RequestrrBot.DownloadClients;
using Requestrr.WebApi.RequestrrBot.Music;

namespace Requestrr.WebApi.Controllers.DownloadClients
{
    [ApiController]
    [Authorize]
    [Route("/api/music")]
    public class MusicDownloadClientController : ControllerBase
    {
        private readonly MusicSettings _musicSettings;
        private readonly DownloadClientsSettings _downloadClientsSettings;
        private readonly IHttpClientFactory _httpClientFactory;

        public MusicDownloadClientController(
            IHttpClientFactory httpClientFactory,
            IOptionsSnapshot<MusicSettings> musicSettingsAccessor,
            IOptionsSnapshot<DownloadClientsSettings> botClientsSettingsAccessor)
        {
            _musicSettings = musicSettingsAccessor.Value;
            _downloadClientsSettings = botClientsSettingsAccessor.Value;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet()]
        public async Task<IActionResult> GetAsync()
        {
            return Ok(new MusicSettingsModel
            {
                Client = _musicSettings.Client,
                Lidarr = new LidarrSettingsModel
                {
                    Hostname = _downloadClientsSettings.Lidarr.Hostname,
                    BaseUrl = _downloadClientsSettings.Lidarr.BaseUrl,
                    Port = _downloadClientsSettings.Lidarr.Port,
                    ApiKey = _downloadClientsSettings.Lidarr.ApiKey,
                    MusicPath = _downloadClientsSettings.Lidarr.MusicRootFolder,
                    MusicProfile = _downloadClientsSettings.Lidarr.MusicProfileId,
                    MusicMetadataProfile = _downloadClientsSettings.Lidarr.MusicMetadataProfileId,
                    MusicTags = _downloadClientsSettings.Lidarr.MusicTags ?? Array.Empty<int>(),
                    MusicUseAlbumFolders = _downloadClientsSettings.Lidarr.MusicUseAlbumFolders,
                    UseSSL = _downloadClientsSettings.Lidarr.UseSSL,
                    SearchNewRequests = _downloadClientsSettings.Lidarr.SearchNewRequests,
                    MonitorNewRequests = _downloadClientsSettings.Lidarr.MonitorNewRequests
                },
                Command = _musicSettings.Command
            });
        }

        [HttpPost("disable")]
        public async Task<IActionResult> SaveAsync()
        {
            _musicSettings.Client = DownloadClient.Disabled;
            DownloadClientsSettingsRepository.SetDisabledClient(_musicSettings);
            return Ok(new { ok = true });
        }
    }
}
