using System.ComponentModel.DataAnnotations;

namespace Requestrr.WebApi.Controllers.DownloadClients.Ombi
{
    public class TestOmbiSettingsModel
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
        [Required]
        public string Version { get; set; }
    }
}