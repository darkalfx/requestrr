namespace Requestrr.WebApi.config
{
    public class DownloadClientsSettings
    {
        public OmbiSettings Ombi { get; set; }
        public RadarrSettings Radarr { get; set; }
        public SonarrSettings Sonarr { get; set; }
        public LidarrSettings Lidarr { get; set; }
    }

    public class OmbiSettings
    {
        public string Hostname { get; set; }
        public int Port { get; set; }
        public string BaseUrl { get; set; }
        public string ApiKey { get; set; }
        public string ApiUsername { get; set; }
        public bool UseSSL { get; set; }
        public string Version { get; set; }
    }

    public class RadarrSettings
    {
        public string Hostname { get; set; }
        public int Port { get; set; }
        public string ApiKey { get; set; }
        public string BaseUrl { get; set; }
        public int MovieProfileId { get; set; }
        public string MovieRootFolder { get; set; }
        public string MovieMinimumAvailability { get; set; }
        public int[] MovieTags { get; set; }
        public int AnimeProfileId { get; set; }
        public string AnimeRootFolder { get; set; }
        public string AnimeMinimumAvailability { get; set; }
        public int[] AnimeTags { get; set; }
        public bool UseSSL { get; set; }
        public bool SearchNewRequests { get; set; }
        public bool MonitorNewRequests { get; set; }
        public string Version { get; set; }
    }

    public class SonarrSettings
    {
        public string Hostname { get; set; }
        public int Port { get; set; }
        public string ApiKey { get; set; }
        public string BaseUrl { get; set; }
        public string TvRootFolder { get; set; }
        public int[] TvTags { get; set; }
        public int TvProfileId { get; set; }
        public int TvLanguageId { get; set; }
        public bool TvUseSeasonFolders { get; set; }
        public string AnimeRootFolder { get; set; }
        public int[] AnimeTags { get; set; }
        public int AnimeProfileId { get; set; }
        public int AnimeLanguageId { get; set; }
        public bool AnimeUseSeasonFolders { get; set; }
        public bool UseSSL { get; set; }
        public bool SearchNewRequests { get; set; }
        public bool MonitorNewRequests { get; set; }
        public string Version { get; set; }
    }

    public class LidarrSettings
    {
        public string Hostname { get; set; }
        public int Port { get; set; }
        public string ApiKey { get; set; }
        public string BaseUrl { get; set; }
        public string MusicRootFolder { get; set; }
        public int[] MusicTags { get; set; }
        public int MusicProfileId { get; set; }
        public int MusicMetadataProfileId { get; set; }
        public bool MusicUseAlbumFolders { get; set; }

        public bool UseSSL { get; set; }
        public bool SearchNewRequests { get; set; }
        public bool MonitorNewRequests { get; set; }
        public string Version { get; set; }
    }
}