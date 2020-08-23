using System.ComponentModel.DataAnnotations;

namespace Requestrr.WebApi.Controllers.Configuration
{
    public class ApplicationSettingsModel
    {
        [Required]
        public int Port { get; set; }

        public string BaseUrl { get; set; }
    }
}