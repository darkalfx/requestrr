namespace Requestrr.WebApi.RequestrrBot.DownloadClients.Lidarr
{
    public class LidarrSettings
    {
        public string Hostname { get; set; }
        public int Port { get; set; }
        public string ApiKey { get; set; }
        public string BaseUrl { get; set; }
        public int MusicProfileId { get; set; }
        public int MusicMetadataProfileId { get; set; }
        public string MusicRootFolder { get; set; }
        public int[] MusicTags { get; set; }
        public bool MusicUseAlbumFolders { get; set; }
        public bool SearchNewRequests { get; set; }
        public bool MonitorNewRequests { get; set; }
        public bool UseSSL { get; set; }
    }
}