using System.ComponentModel.DataAnnotations;

namespace Requestrr.WebApi.Controllers.DownloadClients.Sonarr
{
    public class SonarrSettingsModel : TestSonarrSettingsModel
    {
        [Required]
        public string TvPath { get; set; }
        [Required]
        public int TvProfile { get; set; }
        public int[] TvTags { get; set; }
        [Required]
        public int TvLanguage { get; set; }
        public bool TvUseSeasonFolders { get; set; }
        public string AnimePath { get; set; }
        [Required]
        public int AnimeProfile { get; set; }
        public int[] AnimeTags { get; set; }
        [Required]
        public int AnimeLanguage { get; set; }
        public bool AnimeUseSeasonFolders { get; set; }
        public bool SearchNewRequests { get; set; }
        public bool MonitorNewRequests { get; set; }
    }
}
