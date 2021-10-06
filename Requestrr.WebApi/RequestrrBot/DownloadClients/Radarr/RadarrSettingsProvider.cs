namespace Requestrr.WebApi.RequestrrBot.DownloadClients.Radarr
{
    public class RadarrSettingsProvider
    {
        public RadarrSettings Provide()
        {
            dynamic settings = SettingsFile.Read();

            return new RadarrSettings
            {
                Hostname = settings.DownloadClients.Radarr.Hostname,
                BaseUrl = settings.DownloadClients.Radarr.BaseUrl,
                Port = (int)settings.DownloadClients.Radarr.Port,
                ApiKey = settings.DownloadClients.Radarr.ApiKey,
                Categories =  settings.DownloadClients.Radarr.Categories.ToObject<RadarrCategory[]>(),
                SearchNewRequests  = settings.DownloadClients.Radarr.SearchNewRequests,
                MonitorNewRequests  = settings.DownloadClients.Radarr.MonitorNewRequests,
                UseSSL = (bool)settings.DownloadClients.Radarr.UseSSL,
                Version = settings.DownloadClients.Radarr.Version,
            };
        }
    }
}