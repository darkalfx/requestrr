using System.ComponentModel.DataAnnotations;

namespace Requestrr.WebApi.Controllers.DownloadClients.Ombi
{
    public class OmbiSettingsModel : TestOmbiSettingsModel
    {
        public string ApiUsername { get; set; }
        [Required]
        public string Command { get; set; }
    }
}