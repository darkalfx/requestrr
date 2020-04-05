using System.ComponentModel.DataAnnotations;
using Requestrr.WebApi.Controllers.DownloadClients.Ombi;
using Requestrr.WebApi.Controllers.DownloadClients.Sonarr;

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
        public string Command { get; set; }
    }
}