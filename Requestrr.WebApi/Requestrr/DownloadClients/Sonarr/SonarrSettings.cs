namespace Requestrr.WebApi.Requestrr.DownloadClients
{
    public class SonarrSettings
    {
        public string Hostname { get; set; }
        public int Port { get; set; }
        public string ApiKey { get; set; }
        public string BaseUrl { get; set; }
        public int TvProfileId { get; set; }
        public string TvRootFolder { get; set; }
        public int TvLanguageId { get; set; }
        public int[] TvTags { get; set; }
        public bool TvUseSeasonFolders { get; set; }
        public int AnimeProfileId { get; set; }
        public string AnimeRootFolder { get; set; }
        public int AnimeLanguageId { get; set; }
        public int[] AnimeTags { get; set; }
        public bool AnimeUseSeasonFolders { get; set; }
        public bool SearchNewRequests { get; set; }
        public bool MonitorNewRequests { get; set; }
        public bool UseSSL { get; set; }
        public string Version { get; set; }
    }
}