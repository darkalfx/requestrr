using System.ComponentModel.DataAnnotations;

namespace Requestrr.WebApi.Controllers.DownloadClients.Ombi
{
    public class SaveOmbiTvShowsSettingsModel : OmbiSettingsModel
    {
        [Required]
        public string Restrictions { get; set; }
    }
}