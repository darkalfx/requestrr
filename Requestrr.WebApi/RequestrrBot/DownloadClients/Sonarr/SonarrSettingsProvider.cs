namespace Requestrr.WebApi.RequestrrBot.DownloadClients.Sonarr
{
    public class SonarrSettingsProvider
    {
        public SonarrSettings Provide()
        {
            dynamic settings = SettingsFile.Read();

            return new SonarrSettings
            {
                Hostname = settings.DownloadClients.Sonarr.Hostname,
                BaseUrl = settings.DownloadClients.Sonarr.BaseUrl,
                Port = (int)settings.DownloadClients.Sonarr.Port,
                ApiKey = settings.DownloadClients.Sonarr.ApiKey,
                Categories =  settings.DownloadClients.Sonarr.Categories.ToObject<SonarrCategory[]>(),
                SearchNewRequests  = settings.DownloadClients.Sonarr.SearchNewRequests,
                MonitorNewRequests  = settings.DownloadClients.Sonarr.MonitorNewRequests,
                UseSSL = (bool)settings.DownloadClients.Sonarr.UseSSL,
                Version = settings.DownloadClients.Sonarr.Version,
            };
        }
    }
}