using System.ComponentModel.DataAnnotations;

namespace Requestrr.WebApi.Controllers.DownloadClients
{
    public class TvShowsSettingsModel
    {
        [Required]
        public string Client { get; set; }
        [Required]
        public SonarrSettings Sonarr { get; set; }
        [Required]
        public OmbiSettings Ombi { get; set; }
        [Required]
        public string Command { get; set; }
    }

    public class SonarrSettings
    {
        [Required]
        public string Hostname { get; set; }
        [Required]
        public int Port { get; set; }
        [Required]
        public string ApiKey { get; set; }
        [Required]
        public string BaseUrl { get; set; }
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
        public bool UseSSL { get; set; }
        public bool SearchNewRequests { get; set; }
        public bool MonitorNewRequests { get; set; }
        [Required]
        public string Version { get; set; }
    }
}