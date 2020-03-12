using System.ComponentModel.DataAnnotations;

namespace Requestrr.WebApi.Controllers
{
    public class MovieSettingsModel
    {
        [Required]
        public string Client { get; set; }

        [Required]
        public RadarrSettings Radarr { get; set; }

        [Required]
        public OmbiSettings Ombi { get; set; }

        [Required]
        public string Command { get; set; }
    }

    public class RadarrSettings
    {
        [Required]
        public string Hostname { get; set; }
        [Required]
        public int Port { get; set; }
        [Required]
        public string ApiKey { get; set; }
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
        public bool UseSSL { get; set; }
        [Required]
        public string Version { get; set; }
    }
}
