namespace Requestrr.WebApi.RequestrrBot.DownloadClients.Lidarr
{
    public class LidarrSettingsProvider
    {
        public LidarrSettings Provide()
        {
            dynamic settings = SettingsFile.Read();

            return new LidarrSettings
            {
                Hostname = settings.DownloadClients.Lidarr.Hostname,
                BaseUrl = settings.DownloadClients.Lidarr.BaseUrl,
                Port = (int)settings.DownloadClients.Lidarr.Port,
                ApiKey = settings.DownloadClients.Lidarr.ApiKey,
                MusicProfileId = settings.DownloadClients.Lidarr.MusicProfileId,
                MusicMetadataProfileId = settings.DownloadClients.Lidarr.MusicMetadataProfileId,
                MusicRootFolder = settings.DownloadClients.Lidarr.MusicRootFolder,
                MusicTags = settings.DownloadClients.Lidarr.MusicTags.ToObject<int[]>(),
                MusicUseAlbumFolders = settings.DownloadClients.Lidarr.MusicUseAlbumFolders,
                SearchNewRequests  = settings.DownloadClients.Lidarr.SearchNewRequests,
                MonitorNewRequests  = settings.DownloadClients.Lidarr.MonitorNewRequests,
                UseSSL = (bool)settings.DownloadClients.Lidarr.UseSSL
            };
        }
    }
}