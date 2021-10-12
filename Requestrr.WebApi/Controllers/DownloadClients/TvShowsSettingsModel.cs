using System.ComponentModel.DataAnnotations;
using Requestrr.WebApi.Controllers.DownloadClients.Ombi;
using Requestrr.WebApi.Controllers.DownloadClients.Overseerr;
using Requestrr.WebApi.Controllers.DownloadClients.Sonarr;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Overseerr;

namespace Requestrr.WebApi.Controllers.DownloadClients
{
    public class TvShowsSettingsModel
    {
        [Required]
        public string Client { get; set; }
        [Required]
        public SonarrSettingsModel Sonarr { get; set; }
        [Required]
        public OmbiSettingsModel Ombi { get; set; }
        [Required]
        public OverseerrSettings Overseerr { get; set; }
        [Required]
        public string Restrictions { get; set; }
    }
}