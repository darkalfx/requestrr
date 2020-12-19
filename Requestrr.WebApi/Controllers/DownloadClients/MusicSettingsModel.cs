using System.ComponentModel.DataAnnotations;
using Requestrr.WebApi.Controllers.DownloadClients.Lidarr;

namespace Requestrr.WebApi.Controllers.DownloadClients
{
    public class MusicSettingsModel
    {
        [Required]
        public string Client { get; set; }
        [Required]
        public LidarrSettingsModel Lidarr { get; set; }
        [Required]
        public string Command { get; set; }
    }
}