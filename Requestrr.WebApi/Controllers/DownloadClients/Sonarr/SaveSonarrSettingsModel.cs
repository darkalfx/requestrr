using System.ComponentModel.DataAnnotations;

namespace Requestrr.WebApi.Controllers.DownloadClients.Sonarr
{
    public class SaveSonarrSettingsModel : SonarrSettingsModel
    {
        [Required]
        public string Restrictions { get; set; }
    }
}