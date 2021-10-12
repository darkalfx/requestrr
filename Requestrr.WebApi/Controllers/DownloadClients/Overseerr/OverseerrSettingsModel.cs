using System.ComponentModel.DataAnnotations;

namespace Requestrr.WebApi.Controllers.DownloadClients.Overseerr
{
    public class OverseerrSettingsModel
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
}