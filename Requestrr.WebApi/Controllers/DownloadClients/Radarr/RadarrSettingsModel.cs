using System.ComponentModel.DataAnnotations;

namespace Requestrr.WebApi.Controllers.DownloadClients.Radarr
{
    public class RadarrSettingsModel : TestRadarrSettingsModel
    {
        [Required]
        public string MovieMinAvailability { get; set; }
        [Required]
        public string MoviePath { get; set; }
        [Required]
        public int MovieProfile { get; set; }
        [Required]
        public int[] MovieTags { get; set; }
        [Required]
        public string AnimeMinAvailability { get; set; }
        [Required]
        public string AnimePath { get; set; }
        [Required]
        public int AnimeProfile { get; set; }
        [Required]
        public int[] AnimeTags { get; set; }
        public bool SearchNewRequests { get; set; }
        public bool MonitorNewRequests { get; set; }
    }
}
