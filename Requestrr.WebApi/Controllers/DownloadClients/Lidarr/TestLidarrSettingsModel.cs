using System.ComponentModel.DataAnnotations;

namespace Requestrr.WebApi.Controllers.DownloadClients.Lidarr
{
    public class TestLidarrSettingsModel
    {
        [Required]
        public string Hostname { get; set; }
        [Required]
        public int Port { get; set; }
        [Required]
        public string ApiKey { get; set; }
        public string BaseUrl { get; set; }
        [Required]
        public bool UseSSL { get; set; }
    }
}
