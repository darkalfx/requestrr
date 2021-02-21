using System.ComponentModel.DataAnnotations;

namespace Requestrr.WebApi.Controllers.DownloadClients.Overseerr
{
    public class SaveOverseerrMoviesSettingsModel : OverseerrSettingsModel
    {
        [Required]
        public string Command { get; set; }
    }
}