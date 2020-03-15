namespace Requestrr.WebApi.Requestrr.DownloadClients
{
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
        public bool SearchNewRequests { get; set; }
        public bool MonitorNewRequests { get; set; }
        public bool UseSSL { get; set; }
        public string Version { get; set; }
    }
}