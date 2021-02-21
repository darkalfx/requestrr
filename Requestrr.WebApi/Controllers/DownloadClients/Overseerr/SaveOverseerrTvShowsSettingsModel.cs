using System.ComponentModel.DataAnnotations;

namespace Requestrr.WebApi.Controllers.DownloadClients.Overseerr
{
    public class SaveOverseerrTvShowsSettingsModel : OverseerrSettingsModel
    {
        [Required]
        public string Command { get; set; }
        [Required]
        public string Restrictions { get; set; }
    }
}