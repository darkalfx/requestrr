using System.ComponentModel.DataAnnotations;
using Requestrr.WebApi.Controllers.DownloadClients.Ombi;
using Requestrr.WebApi.Controllers.DownloadClients.Radarr;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Overseerr;

namespace Requestrr.WebApi.Controllers.DownloadClients
{
    public class MovieSettingsModel
    {
        [Required]
        public string Client { get; set; }

        [Required]
        public RadarrSettingsModel Radarr { get; set; }

        [Required]
        public OmbiSettingsModel Ombi { get; set; }

        [Required]
        public OverseerrSettings Overseerr { get; set; }
    }
}
